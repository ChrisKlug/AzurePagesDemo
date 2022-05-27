---
layout: post
current: post
cover:  /assets/images/covers/blueprint-2.jpg
smallcover:  /assets/images/covers/blueprint-2-small.jpg
navigation: True
title: Infrastructure as Code - An intro - Part 2 - Using Azure CLI
date: 2021-09-08 14:07:00
tags: [infrastructure, infrastructure as code, iac, azure cli]
class: post-template
subclass: 'post'
author: zerokoll
---
As I mentioned in my previous [post](/iac-an-intro-part-1), that there are a lot of tools out there for doing IaC. In this post, I want to show how to set up infrastructure in an imperative way using the Azure CLI.

I may not agree with it being the best way to do IaC, but it is a way that is used by quite a lot of people. And since it is so easy to get started with, and easy to understand, I definitely understand why.

Let's start out by having a look at the Azure CLI!

## CLI Installation

Installing the Azure CLI is a piece of cake. Just browse to https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows, download the MSI-package for the latest version, and install it!

__Note:__ For Linux or MacOS, you can head to https://docs.microsoft.com/en-us/cli/azure/install-azure-cli to get information about how to install it on your system.

Once installed, you can verify that the installation was successful by running

```bash
> az --version
```

This will give you information about the currently installed version, as well as tell you if your version is old and needs updating. 

If you already have a version installed, and the above command tells you that have updates available, just run

```bash
> az upgrade
```

Once that is done, you are good to go!

## Authenticating

However, before you can start modifying things in Azure, you obviously have to authenticate yourself in some way. Using the Azure CLI, you do this by running

```bash
> az login
```

This will pop-up a browser that asks you to authenticate, or it will tell you to use the _device code flow_ by adding the `--use-device-code` parameter. It depends on whether or not the CLI can open a browser or not. In most cases you get a browser windows popping up.

__Comment:__ You actually get the information about the _device code flow_ in both cases, but most of the time the browser will sort of make you miss that output... 

__Note:__ For automated scenarios, you might want to use a service principal instead of a regular user account. In those scenarios, you can add the `--service-principal` parameter. More information about this can be found by running `az login --help`

The response from this command, after authenticating, is a list of all the subscriptions that you were given access to. 

However, you might also be faced with some warnings before the list of available subscription. Some of these warnings might look like this

> The following tenants require Multi-Factor Authentication (MFA) Use 'az login --tenant TENANT_ID' to explicitly login to a tenant.

The reason for this is that the used account has access to multiple tenants, and one or more of them require MFA authentication. To get access subscriptions in those tenants, you need to follow the advise in the warning. That is, you need to find the tenant ID(s) for the locked down tenants, and run

```bash
> az login --tenant <TENANT_ID>
```

Once that is sorted, or if you didn't get any warnings, you need to select the subscription that you want to work with. THis is done by running

```bash
> az account set -s <SUBSCRIPTION ID>
```

That's it! Now you are logged in and ready to create some infrastructure! But before we do that, I just want to cover some simple basics when it comes to the Azure CLI.

## CLI Basics

The Azure CLI is pretty predictable. You just define what type of resource you want to work with, and then you use `list`, `show`, `create`, `delete` or `update` to perform the action you need.

For example, to list all resource groups, you use the `az group list` command

```bash
> az group list

[
  {
    "id": "/subscriptions/bec02fc1-d197-4e23-8d83-XXXXXXXXXXX/resourceGroups/k8s4devs",
    "location": "northeurope",
    "managedBy": null,
    "name": "k8s4devs",
    "properties": {
      "provisioningState": "Succeeded"
    },
    "tags": null,
    "type": "Microsoft.Resources/resourceGroups"
  }
]
```

In this case, I only had one. But if you know you JSON, you can see that it is actually a JSON array that is being returned.

To look at a _specific_ resource group, you use the `az group show` command

```bash
> az group show --name k8s4devs

{
  "id": "/subscriptions/bec02fc1-d197-4e23-8d83-XXXXXXXXXXX/resourceGroups/k8s4devs",
  "location": "northeurope",
  "managedBy": null,
  "name": "k8s4devs",
  "properties": {
    "provisioningState": "Succeeded"
  },
  "tags": null,
  "type": "Microsoft.Resources/resourceGroups"
}
```

Once again, I get a JSON response, but in this case I get only the one that corresponds to the name i provided using the `--name` parameter.

That's about all you need to know. Use `list` to get a list, `show` to get a specific resource, and the CRUD ones to manipulate it. As I said, very predictable!

One more thing that might be useful to know is that you can change the response output format using the `--output`, or `-o`, parameter. By default it is JSON, but often it can be nice to use `-o tsv` to get a value as a string. On top of that, you can query the returned information using a JMESPath query. So for example, to get only the provisioning state of the above resource group, as a string, you can run 

```bash
> az group show -n k8s4devs --query properties.provisioningState -o tsv
```

If you are curious about what else you can do with the CLI, there are great tutorials online. Or, you can try out different commands adding the `--help` parameter at the end. This will give you more information about the options available. For example

```bash
> az group show --help

Command
    az group show : Gets a resource group.

Arguments
    --name --resource-group -g -n [Required] : Name of resource group. You can configure the default
                                               group using `az configure --defaults group=<name>`.

...
```

Ok, now that we have the CLI basics down, it might be time to look at creating some infrastructure!

## What are we building?

But before we can create some infrastructure, we need to figure out what we are building. 

For this blog series, I'm going to demo building the same infrastructure using different tools over several posts. So I have decided to create a pretty generic infrastructure, that is simple, but still able to show off the basic features. This should allow us to compare the different tools without getting too bogged down by the nitty gritty of _why_ we need that specific infrastructure. 

With that in mind, I have decided to build an infrastructure based on some of the most common resources I see in my job. These are as follows:

- A Resource Group to hold all of the resources
- An App Service Plan using the Free tier
- An App Service, inside the App Service Plan
- Application Insights (connected to the App Service)
- A Log Analytics Workspace to store the Application Insights telemetry
- A SQL Server Database

And of course, the Application Insights information, as well as the connection string to the database, needs to be added to the App Service as configuration values.

I think that should cover most basic applications I see out there. Sure, we can make it a _LOT_ more complicated, but that's not the goal for this exercise!

I also want to mention that I'm going with Azure CLI instead of Azure PowerShell, as I find the PowerShell commandlets a lot harder to grasp than the CLI. I also find it a lot harder to use PowerShell when trying to be idempotent. However, I will still put my Azure CLI commands in a PowerShell script, and use PowerShell to glue it all together. 

__Note:__ If you feel more comfortable with a Bash script, that would work just as well! But I think I have more Windows-based readers, than Linux-based on my blog. So I think PowerShell makes more sense... And PowerShell Core do work on both platforms

## Creating the IaC script

Now that we have the Azure CLI installed and configured, and we know what we are building, we can get started building the actual infrastructure.

The first step is to create a resource group to hold our resources. It looks something like this

```powershell
$rg = $(az group create --location westeurope --name MyDemoRg) | ConvertFrom-Json
```

This will create a resource group called __MyDemoRg__, parse the returned JSON, and store it in the PowerShell variable __$rg__ for future use.

__Note:__ In most cases you can replace the `--location` parameter with `-l`, the `--name` one with `-n` and the `--resource-group` (which we'll use in just a second) with `-g` to make the commands shorter.

Now that we have a place to put our resources, we can start adding the resources we need. First up is the SQL Server database. This needs to be created before the rest of the resources as the connection string needs to be put into the Web App configuration.

```powershell
$login = "server_admin"
$password = "P@ssw0rd1!"

$sqlServer = $(az sql server create -g $rg.Name -l $rg.Location -n MyDemoSqlServer123 --admin-user $login --admin-password $password) | ConvertFrom-Json

$sqlDb = $(az sql db create -g $rg.Name --server $sqlServer.Name -n MyDemoDb --service-objective S0) | ConvertFrom-Json
```

As you can probably see, this creates a new SQL Server instance in the __MyDemoRg__ resource group, and stores the result in the __$sqlServer__ variable. Next, it adds a new `S0` sized SQL Database in the newly created SQL Server instance, and stores that response in the __$sqlDb__ variable for future use.

__Warning:__ Do __NOT__ hard code credentials is your scripts. Even if you only add it to a private source control repo, there is always risk involved. In this case, it is only a demo! Not to mention that I will remove it later (before committing it to source control).

Now that we have the database up and running, we need to make sure that the Web App that we are going to create can access it through the firewall. 

In this case, I'm going to keep it simple and just let all Azure IPs access it. Something that can be done by running the following command

```powershell
az mysql server firewall-rule create -g $rg.Name --server $sqlServer.Name --name "AllowAllWindowsAzureIps" --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

This should open up the SQL Server firewall for all Azure-based IP addresses.

With the database in place, we can have a look at the next resource we need. In this case, that would be the Log Analytics Workspace that is used for storing the Application Insights data. 

```powershell
$workspace = $(az monitor log-analytics workspace create -g $rg.Name -n MyDemoWorkspace) | ConvertFrom-Json
```

Once again, it is just a matter of making a single call to create the workspace, and then adding it to a variable for future use.

With the database and the workspace in place, we can start looking at the actual web application. And for that, we need to create an App Service Plan and an App Service. Like this

```powershell
$plan = $(az appservice plan create -g $rg.Name -n MyDemoPlan --sku F1) | ConvertFrom-Json

$app = $(az webapp create -g $rg.Name -p $plan.Name -n MyDemoApp --runtime '"DOTNET|5.0"') | ConvertFrom-Json
```

This creates a new plan using the `F1` SKU, and then adds a new .NET 5.0 Web App to it. And the responses are parsed and stored, just as before.

__Warning:__ Yes, you need both the single quote _and_ double quotes around the `--runtime` value to escape the pipe character...

__Note:__ If you need to find what runtimes are available, you can run `az webapp list-runtimes`

Now that we have the Web App, we can start adding the configuration to it. For example the SQL Database connection string can be added to the Connection String settings by running a few commands that looks like this

```powershell
$connstring=$(az sql db show-connection-string -n $sqlDb.Name --server $sqlServer.Name --client ado.net --output tsv) -replace '<username>', "$login" -replace '<password>', "$password"

az webapp config connection-string set -g $rg.Name -n $app.Name --connection-string-type SQLAzure --settings connectionstring="$connstring" | Out-Null
```

The first part of this code retrieves the connection string for the database, replacing the `<username>` and `<password>` placeholders with their correct values. It then sets a Web App connection string setting called __connectionstring__ to that value.

In the second part, where it sets the connection string setting, I also pipe the output to `Out-Null`. This makes sure that the response is not written to the output. The reason for this, is that the returned value contains the actual connection string, including username and password. This should __not__ be output to any output stream, as it might end up being logged somewhere! Leaking credentials, even to internal build system logs is a very bad idea...

The last part of the puzzle is to add the Application Insights resource, and connect it to the Web App. However, before we can do that, we need to install the `application-insights` extension for the CLI by running

```bash
> az extension add -n application-insights
```

__Note:__ This is a one time thing that should be done on the build agent, or whatever machine will be running the script. Not as part of the IaC.

__Comment:__ If you are missing this extension, you will get an error that says _az : ERROR: az monitor: 'app-insights' is not in the 'az monitor' command group. See 'az monitor --help'_

With that in place, we can create the Application Insights resource, 

```powershell
$ai = $(az monitor app-insights component create -g $rg.Name -l $rg.Location --app $app.Name --workspace $workspace.Id)  | ConvertFrom-Json
```

Finally, we need to connect the Web App to the Application Insights resource by setting a few of application settings called `APPINSIGHTS_INSTRUMENTATIONKEY`, `ApplicationInsightsAgent_EXTENSION_VERSION` and `XDT_MicrosoftApplicationInsights_Mode`.

This is done by running

```powershell
az webapp config appsettings set `
      -g $rg.Name -n $app.Name `
      --settings APPINSIGHTS_INSTRUMENTATIONKEY=$($ai.InstrumentationKey) `
      ApplicationInsightsAgent_EXTENSION_VERSION=~2 `
      XDT_MicrosoftApplicationInsights_Mode=recommended
```

That should be it! By running this script, we should get the required infrastructure to run our application. However, there are a few hardcoded bits in there, as mentioned before. To get away from that, I suggest adding some PowerShell input parameters to the script.

## The PowerShell IaC script

With the parameters added, the full script ends up looking like this

```bash
param (
	[Parameter(Mandatory=$true)][string]$appName,
	[Parameter()][string]$location = "westeurope",
	[Parameter()][string]$sqlSize = "S0",
	[Parameter()][string]$appSvcPlanSku = "F1",
	[Parameter()][string]$sqlUser = "server_admin",
	[Parameter()][string]$sqlPwd = "P@ssw0rd1!"
)

Write-Host "Setting up infrastructure..."

$rg = $(az group create --location $location --name $appName) | ConvertFrom-Json

$sqlServer = $(az sql server create -g $rg.Name -l $rg.Location -n "$($appName)Sql" --admin-user $sqlUser --admin-password $sqlPwd) | ConvertFrom-Json

$sqlDb = $(az sql db create -g $rg.Name --server $sqlServer.Name -n "$($appName)Db" --service-objective $sqlSize) | ConvertFrom-Json

az mysql server firewall-rule create `
          -g $rg.Name `
          --server $sqlServer.Name `
          --name "AllowAllWindowsAzureIps" `
          --start-ip-address 0.0.0.0 `
          --end-ip-address 0.0.0.0

$workspace = $(az monitor log-analytics workspace create -g $rg.Name -n "$($appName)Workspace") | ConvertFrom-Json

$plan = $(az appservice plan create -g $rg.Name -n "$($appName)Plan" --sku $appSvcPlanSku) | ConvertFrom-Json

$app = $(az webapp create -g $rg.Name -p $plan.Name -n "$($appName)App" --runtime '"DOTNET|5.0"') | ConvertFrom-Json

$connstring=$(az sql db show-connection-string -n $sqlDb.Name --server $sqlServer.Name --client ado.net --output tsv) -replace '<username>', "$sqlUser" -replace '<password>', "$sqlPwd"

az webapp config connection-string set -g $rg.Name -n $app.Name --connection-string-type SQLAzure --settings connectionstring="$connstring" | Out-Null

$ai = $(az monitor app-insights component create -g $rg.Name -l $rg.Location --app $app.Name --workspace $workspace.Id)  | ConvertFrom-Json

az webapp config appsettings set `
      -g $rg.Name -n $app.Name `
      --settings APPINSIGHTS_INSTRUMENTATIONKEY=$($ai.InstrumentationKey) `
      ApplicationInsightsAgent_EXTENSION_VERSION=~2 `
      XDT_MicrosoftApplicationInsights_Mode=recommended
```

All you need to do to get the required infrastructure, is to run this script, with at least the required input parameters defined. And, on top of that, it should be idempotent, allowing it to be run over and over again without problems.

__Comment:__ Using default parameter values can make the script very flexible, but still very simple to call in most cases. And also, adding a few `Write-Host` statements in there can give the user a feeling of things happening. Not to mention that the output can bed used for both debugging and historical logging in a build pipeline.

__Note:__ One thing to note is that this script takes a bit of a simplistic approach to naming things. Some resources actually need to have unique names, which would case some problems with the script. But you get the point... This also comes back to the notion of treating your infrastructure as cattle and not pets. They can probably do without perfect names. Adding a random string at the end of the name makes it less likely to collide with existing resources.

To run the script, just run

```bash
> ./IaC.ps1 -appName MyUniqueDemoAppName
```

And since all of the resources are in a single Resource Group in this case, they can easily be removed by running

```bash
> az group delete -n MyUniqueDemoAppName -y
```

In a more complex environment, you might need another script to handle the tear down of the infrastructure you set up.

## Conclusion

As you have seen, building your infrastructure using the Azure CLI (and PowerShell), in a imperative way like this, is fairly simple. And because of this, it is probably the easiest way to get started with IaC. However, it does lack some of the more advanced, and useful features that you get by using a declarative approach, and a tool built specifically for this purpose.

For example, in this case, the script ends up being idempotent without any problems. However, even with the Azure CLI, which is mostly idempotent, you can often end up with somewhat complicated code to handle "create vs update", depending on whether or not it is the first time the scripts is run.

Having that said, I definitely find this to be a decent way to get started with IaC. And I'm pretty sure it will be enough if you have a simple infrastructure, or just need to set up ad-hoc environments with pre-defined resources. Maybe lab environments, or per developer dev environments. But in the long run, I do believe that there are better ways to do it, using purpose-built tools.

The next post will talk about ARM templates, and show how to use them to set up the same infrastructure. However, that is for another day! If you are interested in getting notified when that is published, just follow me on Twitter [@Zerokoll](https://twitter.com/zerokoll).

The third part, [Infrastructure as Code - An intro - Part 3 - Using ARM](/iac-an-intro-part-3), is now available if you want to keep reading about more ways to do IaC