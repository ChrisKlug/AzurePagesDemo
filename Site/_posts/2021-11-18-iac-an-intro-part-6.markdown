---
layout: post
current: post
cover:  /assets/images/covers/pulumi.jpg
smallcover:  /assets/images/covers/pulumi-small.jpg
navigation: True
title: Infrastructure as Code - An intro - Part 6 - Using Pulumi
date: 2021-11-18 14:50:00
tags: [infrastructure, infrastructure as code, pulumi]
class: post-template
subclass: 'post'
author: zerokoll
---
The 6th entry in my blog series about IaC is dedicated to Pulumi.
 
Pulumi is a very different beast, compared to the previously covered technologies ([ARM](/iac-an-intro-part-3), [Bicep](/iac-an-intro-part-4) and [Terraform](/iac-an-intro-part-5)), in that it is not based on a Domain Specific Language. Instead, Pulumi allows you to write your IaC in your language of choice. As long as your language of choice is JavaScript/TypeScript, Python, Go or .NET Core (C#, F# and VB). This makes the Pulumi experience a lot different from using a technology that uses a DSL (or JSON). 

DSL:s are often heavily tailored towards the task they are meant to solve. However, they are often lacking quite a bit of the flexibility you get from using a "full" programming language. Because of this, Pulumi can often offer us a lot more flexibility when defining our infrastructure.

On top of that, Pulumi is also provider based like Terraform, allowing us to set up resources outside of Azure as well. And even if there aren't as many providers as there is for Terraform at the moment, more are being added continuously. And if you can't find a provider for your scenario, you can quite easily extend Pulumi to support unsupported platforms or features.

__Caveat:__ The support for extending Pulumi is partly dependent on language of choice. Different languages support a slightly different feature set. Generally based on possibilities/limitations within the languages. For example, JavaScript/TypeScript can offer some features that .NET Core can't, due to the static nature of the .NET Core languages.
 
## What is Pulumi
 
Pulumi is built by Pulumi Corp, aiming to allow for a more developer focused approach to IaC than the existing tools allow for. It does this, as mentioned before, by allowing us to write the IaC using a variety of languages instead of using a DSL or JSON.

In this post, I will focus on using Pulumi together with TypeScript. I'm not quite sure why I lean towards TypeScript when using Pulumi. As a C# dev, I assume I should be leaning towards using C#. However, I feel very comfortable with TypeScript, and it offers a couple of features that the C# version doesn't. It also happened be the language I was using when I started working with Pulumi, and I never really felt a need to switch to C#.

The Pulumi architecture is a bit "interesting". It is layered in a way that allows us to write our code in different languages, while at the same time being able to share a lot of the core functionality. The way it does this, is by using a flow that looks like this.

You tell the Pulumi CLI that you want to set up some infrastructure. The CLI then loads the correct language runtime based on the language used by the project. The language runtime then executes your code, which generates a "desired state". This desired state is then "diff:ed" against the current state, which is stored in a state store of your choice. The result of this "diff", basically a definition of what resources need to be created, deleted or updated, is then passed to a provider plugin. This plugin is then responsible for talking to whatever system it needs to talk to, to get your infrastructure set up.

As you can see, "our" code is a pretty small part of this. And it is only used to generate the desired state. So, as long as the language specific SDK can generate the desired state, the rest of the tool can be generic.

__Note:__ This is very much a simplification, and it has technical details that are left out for simplicity.

The code, you as developers write, is written using a Pulumi SDK that knows how to interact with Pulumi. So, for TypeScript for example, it consists of a bunch of TypeScript/JavaScript classes that you can use to define the resources you want. These classes in turn generate the desired state that is handed to Pulumi.

__Note:__ Even if it looks like you are creating resources in our code, you are really just defining a desired state. The actual resource creation is handled by the provider, based off of the desired state that your code generates.

The SDK:s (and provider plugins) are distributed using the package management solution used by the selected language/platform. So, for example, for C# it is distributed using NuGet, and for JavaScript/TypeScript it uses npm. And since each SDK targets a specific language and provider, they can be written in way that feels natural in your chosen language. In this post, the SDK of choice is the Azure SDK for TypeScript.

It might be worth noting that there are actually 2 sets of SDK:s for Azure, a "classic" one and a "native" one. The classic one is hand crafted, and because of this, suffers from a bit of feature lag. Just like Terraform does. The "native" one, which is definitely the recommended one, is automatically generated from the OpenAPI definition for the Azure API. This removes most of the feature lag, as new versions can be released pretty much as soon a new feature is published to the Azure API. I personally find the API:s in the "classic" SDK nicer to work with, but I still gravitate towards the "native" one as they generally have newer features. They also have naming parity with the API, allowing your to find information about the types in other places than the Pulumi docs.
 
### State
 
Pulumi, just as Terraform, keeps track of the current state of the infrastructure using a state storage. This is basically just JSON-files that keeps track of what resources were available at the end of the last run, and their defined properties. This is state is then used to figure out what changes need to be performed to get the infrastructure to correspond to a new desired state.

And just as with Terraform, the state can be stored in a variety of different places, such as Azure blob storage, Amazon S3 or Pulumi's own cloud service. However, with Pulumi, the CLI is responsible for keeping track of the store information. This is a big difference from Terraform where that information is stored in the Terraform files. And to be honest, as a consultant with a lot of clients, I'm not sure that having the CLI keep track of the back-end seems like the best solution. I can easily see this cause problems...

There is actually another big difference between Terraform and Pulumi when it comes to the state. Terraform will compare the desired state against a combination of the Terraform state and the "real" infrastructure, which allows it to reset all properties based on the desired state, including setting properties that haven't been explicitly defined, back to their default values. Pulumi on the other hand, only uses its own state. This means that only properties specifically set during configuration is updated/reset, allowing for non-defined properties to drift in the infrastructure.

__Note:__ The state management might not be a big deal if you lock down your environment, making sure that individuals can't make changes to it. However, it is definitely worth mentioning, and keeping in the back of your mind when looking at IaC tools. 
 
## Pulumi pre-requisites
 
The tools used for working with Pulumi is a bit different depending on what language you are using. However, for TypeScript, it looks like this
 
-	Azure CLI
-	The Pulumi CLI
- Node.js - any currently supported version
-	An editor of some sort. I recommend VS Code, or at least one that supports TypeScript definitions

The fact that TypeScript is a strongly typed programming language, as opposed to a DSL, means that we don't need any extra extensions to get tooling support. Instead, as long as you editor of choice supports TypeScript, you will automatically get tooling support based on the types!

I'm going to assume that you have the Azure CLI installed already as it has been used in the previous posts. If you haven't, I suggest going back to the [post about the Azure CLI](/iac-an-intro-part-2) and having a look at how to install it.

I'm also going to skip out on explaining how to install Node. If you don't have it installed, have a look at https://nodejs.org/en/.

### Installing Pulumi
 
As I am skipping out on how to install Azure CLI and Node, the only thing you need to install is Pulumi. And in this brave, new, cross platform world, you can obviously install it on Windows, Linux and Mac. Each having its own way of doing the install.

On Windows, the recommended approach is to use [Chocolatey](https://chocolatey.org/), which is a fantastic package manager for Windows. And even if package managers might feel a bit odd for Windows people, I would definitely recommend trying it out.

__Note:__ Package managers for Windows _is_ going to be a thing. Just get used to it. And right now, Chocolatey is the best one available. But maybe, just maybe, [WinGet](https://docs.microsoft.com/en-us/windows/package-manager/winget/) might become a "thing" in the future.

To install Pulumi using Chocolatey, you just need start a Terminal with Administrator privileges and run
 
```bash
> choco install pulumi
```

During the installation you will probably have to accept that it runs a PowerShell script, which is fine.

__Comment:__ No, in general it is not a good idea to run PowerShell scripts you found on the interwebs without making sure that they aren't doing anything bad. But in this case, I'm pretty sure you can trust it!
 
Once the install has completed, you can verify that everything has worked as it should, by running
 
```bash
> pulumi version  
v3.17.1
```
 
If it says that it can’t find the Pulumi executable, try restarting your terminal to reload the PATH variable.

__Note:__ If you are on Linux or Mac, have a look at https://www.pulumi.com/docs/get-started/install/

I would also recommend verifying that the Azure CLI is set up to use the correct subscription if you have multiple ones. This is easily done by running 

```bash
> az account show
```

If the selected Azure subscription isn't the one you want to use, you can use `az account list` to locate the ID of the one you want to use, and then use `az account set -s <SUBSCRIPTION ID>` to set it as the selected one.

That's it! You are ready to go!
 
## Creating up a Pulumi project
 
Just as in the previous posts, the target infrastructure looks like this
 
- A Resource Group to hold all of the resources
-	An App Service Plan using the Free tier
-	A Web App, inside the App Service Plan
-	Application Insights (connected to the Web App)
-	A Log Analytics Workspace to store the Application Insights telemetry
-	A SQL Server Database
 
The Web App also needs to be connected to the SQL Server, using a connection string in the Connect Strings part of the app configuration, and to the Application Insights resource using the required settings in the App Settings part.
 
All of this should be fairly familiar if you have read the previous posts. However, if you haven’t, you will probably be able to pick it up along the way anyway!
 
So...let's go ahead and create a Pulumi project.

The Pulumi CLI has some really nice features. One of them is that it can easily create starter projects for you. However, before we can do this, we need to "log in". This basically means telling Pulumi where you want to store the state.

In this case, I suggest simply storing the state locally, together with the application you are about to create. So, let's go ahead and create a directory for the project, and tell Pulumi to store the state there

```bash
> mkdir dulumi-demo
> cd pulumi-demo
> pulumi login file://.
```

This will create a directory called __PulumiDemo__, and then tell Pulumi to store any generated state inside it by "logging in to" `file://.`.

The next step is to tell Pulumi to create a starter project for you to work with. Luckily, this is as simple as running

```bash
> pulumi new azure-typescript
```

This will start an interactive walk through that will ask you for a project name, a project description, a stack name (more about that later), a passphrase and an Azure location to use as the default location when creating Azure resources.

You can set most of them to their default values by simply pressing `Enter`. However, you will have to select a passphrase and a location.

__Note:__ The passphrase is used to protect your configuration, allowing you to store encrypted secrets in your config. This can be replaced by another secret store such as Azure KeyVault. However, that is outside of the scope for this post.

__Comment:__ Choose whatever Azure location makes most sense to you. It might not be the default, which is __WestUS__. In my case for example, __WestEurope__ makes more sense.

Once you have entered all the values, Pulumi will go ahead and set up a new TypeScript starter project, and run `npm install` to get all the dependencies installed. 

__Note:__ Running `npm install` can take a while. But that is nothing special for Pulumi, that's just the way npm works...

Once the project set up has completed, I suggest opening the folder in VS Code, or whatever editor you want to use. For VS Code, you just run

```bash
> code .
```

The starter project that the Pulumi CLI creates is a fairly standard TypeScript project that looks like this

![Azure/TypeScript Pulumi project in VS Code](/assets/images/iac/pulumi-project.png "Azure/TypeScript Pulumi project in VS Code")
 
As this is a fairly standard TypeScript project, I'm not going to say too much about it. However, it is worth noting the `Pulumi.yaml` and `Pulumi.dev.yaml`. These YAML-files make this a Pulumi project. In the `Pulumi.yaml` file, you will find some very basic information about the project

```yaml
name: pulumi-demo
runtime: nodejs
description: A minimal Azure Native TypeScript Pulumi program
```

and in the `Pulumi.dev.yaml` file, you will find the configuration for the __dev__ stack

```yaml
encryptionsalt: v1:IykIQhHawfU=:v1:+I7tf853kbGI2RmW:iLKunFMrscgDDwEAV93fn3KwqRw0CA==
config:
  azure-native:location: WestEurope
```

As you can see, there is a property called `encryptionsalt` that was generated using your "passphrase" during set up. This allows you to have encrypted configuration values inside this file, even if you store it in source control, which is really nice for things like credentials or connection strings.

__Note:__ I will talk more about __stacks__ later on. For now, all you need to understand is that your configuration is stored per stack. This means that you can have several different stacks, one for each environment you want to be able to set up, storing a specific set of configuration for that specific environment.
 
## Cleaning up the starter project

Now that you have a project to work with, you can start defining your infrastructure. However, the starter project comes with a couple of pre-defined resources. Some of which you don't need. So, let's start by removing the unnecessary resources.

If you open up index.ts, you will see that it creates a resource group and a storage account. And that it then uses some "weird" code to export a `primaryStorageKey`. 

__Note:__ Even if you haven't worked with Pulumi before, you are probably still able to understand most of the stuff going on, which is really cool! This is one of the benefits from using a standard programming language instead of a DSL.

You aren't going to use the storage account, or the export, so let's go ahead and remove that.

Looking at the imports, you can probably see that the `azure-native` SDK does a great job at modularizing its content. However, this tends to lead to a lot of imports, as we tend to use several modules. To make this a little less annoying, you can replace the current `@pulumi/azure-native/XXX` imports with a single import that imports all of `@pulumi/azure-native` as `azure`.

Once the imports have been updated, you also need to update the resource group definition from `resources.ResourceGroup` to `azure.resources.ResourceGroup`

In the end, it should look like this

```typescript
import * as pulumi from "@pulumi/pulumi";
import * as azure from "@pulumi/azure-native";

const resourceGroup = new azure.resources.ResourceGroup("resourceGroup");
```

As you can see, defining a resource using Pulumi is just a matter of creating an instance of a class. However, it is very important to understand that this instantiation does not actually go and create a resource in Azure. Instead, it adds a resource definition in the desired state, which is then used by Pulumi, and the provider plugin, to create the resource group.

However, the name `resourceGroup` doesn't seem like the best name for a resource group. So, go ahead and change that to __PulumiDemo__.

```typescript
const resourceGroup = new azure.resources.ResourceGroup("PulumiDemo");
```

You can now go ahead and ask Pulumi what resources it thinks it needs to create based on this new desired state. All you have to do, to get this information is to run

```
> pulumi preview
```

This command will ask you for the passphrase you added to the __dev__ stack during project set up. This is so that Pulumi can decrypt any encrypted values in the configuration.

Once you have provided the correct passphrase, you should see something like this

```
Previewing update (dev):
      Type                                     Name             Plan       
  +   pulumi:pulumi:Stack                      pulumi-demo-dev  create     
  +   └─ azure-native:resources:ResourceGroup  PulumiDemo       create     
  
Resources:
+ 2 to create
```

As you can see, Pulumi has decided that it needs to create a stack and a resource group, which is correct.

__Note:__ Yes, the stack is considered a resource of its own, under which, all other resources in this particular stack is placed.

Once you have verified that Pulumi has correctly figured our what to create, you can go ahead and "deploy" the new infrastructure by running

```
> pulumi up
```

Once again, you are asked to input your passphrase, and then presented with preview just like the one you just looked at. However, this time you also get an interactive menu that allows you to approve or cancel the update, or view "details".

__Note:__ Since Pulumi always runs a `pulumi preview` during `pulumi up`, there is generally no reason to run `pulumi preview` separately.

If you go ahead and select __details__, you will be faced by a more detailed description of what is about to be deployed

```
+ pulumi:pulumi:Stack: (create)
    [urn=urn:pulumi:dev::PulumiDemo2::pulumi:pulumi:Stack::PulumiDemo2-dev]
    + azure-native:resources:ResourceGroup: (create)
        [urn=urn:pulumi:dev::PulumiDemo2::azure-native:resources:ResourceGroup::PulumiDemo]
        [provider=urn:pulumi:dev::PulumiDemo2::pulumi:providers:azure-native::default_1_45_0::04da6b54-80e4-46f7-96ec-b56ff0331ba9]
        location            : "WestEurope"
        resourceGroupName   : "PulumiDemo0a17be63"
```

This is a really helpful view if you are interested in the details of what is going to happen. In this case, it actually highlights a potential problem. It says `resourceGroupName : "PulumiDemo0a17be63"`. Wait, what!? That isn't the resource group name that's in the code.

Pulumi is very opinionated when it comes to naming resources. It is a firm believer of using "cattle-based" naming. Because of this, all resource names are suffixed with 8 random characters by default. This is a nice feature, as it makes it a lot less likely to cause naming conflicts. However, for resource groups that don't need to be globally unique, I generally like to use "proper" names. Mainly because they are often used by humans to find things.

Luckily, Pulumi has no problems allowing you to set a "proper" name. You just need to take responsibility for the naming by setting manually.

So, go ahead and select __no__ to cancel the update, and go back to the __index.ts__ file. 

During the creation of a resource, you can supply a second constructor parameter, containing all the properties you want to set on that resource. Using VS Code's TypeScript tooling, we can see that the second parameter is of type `ResourceGroupArgs`

![Pulumi TypeScript tooling](/assets/images/iac/pulumi-intellisense-1.png "Pulumi TypeScript tooling")

The `ResourceGroupArgs` type is an interface that defines, among other things, a `resourceGroupName` property

![Pulumi TypeScript tooling](/assets/images/iac/pulumi-intellisense-2.png "Pulumi TypeScript tooling")

__Note:__ To get the drop down to open in VS Code, just add the curly braces (`{}`), put the caret in between them, and press `Ctrl + Space`

Setting this property will override Pulumi's default, allowing us to set a name manually like this

```typescript
const resourceGroup = new azure.resources.ResourceGroup("PulumiDemo", {
  resourceGroupName: "PulumiDemo"
});
```

With that update in place, you can go back to the terminal and run 

```bash
pulumi up
```

However, this time you can go ahead and select __yes__ to perform the update. 

After a short time, it should output something like this

```
...
Updating (dev):
      Type                                     Name             Status      
  +   pulumi:pulumi:Stack                      pulumi-demo-dev  created     
  +   └─ azure-native:resources:ResourceGroup  PulumiDemo       created     
  
Resources:
    + 2 created

Duration: 12s
```

Now, you will have a resource group called __PulumiDemo__ in the Azure subscription selected in the Azure CLI.

While deploying the defined infrastructure (the single resource group), Pulumi created some state information to keep track of the fact that this resource group has been created. And since you "logged in" to `file://.`, this state is stored in a file located at __/.pulumi/stacks/dev.json__, where the filename __dev.json__ corresponds to the name of the stack that it stores state for.

![Pulumi state file](/assets/images/iac/pulumi-stack-state.png "Pulumi state file")

Now what we have a resource group to put the rest of the resources in, you can move on to the SQL Server database.
 
## Adding the SQL Database

However, before you can set up the actual database, you need to set up the SQL Server that it should be hosted on.

In Pulumi, that means instantiating another class. In this case a ´new azure.sql.Server´ that looks like this

```typescript
const sql = new azure.sql.Server("mydemosqlserver123", {
  resourceGroupName: resourceGroup.name,
  administratorLogin: "server_admin",
  administratorLoginPassword: "P@ssw0rd1!"
});
```

First, this declaration sets the `resourceGroupName` property to tell it what resource group to put the server in. And instead of manually setting the name to a string, it uses the `resourceGroup.name` property.

Next, it sets the username and password for the admin user. 

For now, this is hard coded into the application, which I know is a __really__ bad idea. However, it is only a temporary solution until you get a bit further into this post. I promise!

But what about the name? Well, for a SQL Server instance, it needs to be globally unique. And as mentioned before, Pulumi solves this by adding a random suffix to the name. So, all in all __mydemosqlserver123__, with a suffix added, should be good enough. However, I would definitely recommend implementing some form of naming standard for your resources. Luckily, with all the features of TypeScript available for use, that is easily solved by declaring a function like this

```typescript
function getName(type: string) {
  return `${ pulumi.getProject().toLowerCase() }-${ type }`;
}
```

This function concatenates the project name, which was defined when setting up the project, with the type of resource, which is passed in as a parameter.

__Comment:__ With JavaScript "hoisting" it doesn't matter where in the TypeScript file you declare the function. It will be "hoisted", and available throughout the entire file just because it is a function. So, if you prefer putting it out of the way at the bottom of the file, that's fine. You can find more information about it at https://developer.mozilla.org/en-US/docs/Glossary/Hoisting

__Note:__ This is probably a too simple naming convention for most projects, as it assumes a single instance of each resource type. It also does not include a location or environment, which is very useful in projects that are used to deploy multiple environments across multiple regions. But for this demo, it will just have to do!

Once the `getName()` method has been added, you can update the SQL Server name like this

```typescript
const sql = new azure.sql.Server(getName("sql"), {
  ...
});

```

This should give the SQL Server a name that looks something like `pulumidemo-sqlXXXXXXXX`, which should be good enough for the current project.

Now that the server has been defined, you can go ahead and define the database.

```typescript
const db = new azure.sql.Database(getName("db"), {
  databaseName: "MyDemoDb",
  resourceGroupName: resourceGroup.name,
  serverName: sql.name,
  sku: {
    name: config.require("sqlSize")
  },
});
```

Once again, the `resourceGroupName` is set, as well as the resource specific properties. In this case that means the name of the server to host the database and the SKU to use. However, since this resource does not need to have a unique name, and the fact that the name of the database should probably be somewhat consistent across deployments, you can also set the `databaseName` to a "proper" name like __MyDemoDb__.

The last part of setting up the database, is to set up the SQL Server firewall to allow access from your application. In this case, for the sake of simplicity, I suggest opening it for all Azure services. This is "easily" done by adding a firewall rule that opens an IP range from 0.0.0.0 to 0.0.0.0. 

In Pulumi, that means instantiating an `azure.sql.FirewallRule` that looks like this

```typescript
new azure.sql.FirewallRule("allowAllAzureIps", {
  firewallRuleName: "AllowAllWindowsAzureIps",
  resourceGroupName: resourceGroup.name,
  serverName: sql.name,
  startIpAddress: "0.0.0.0",
  endIpAddress: "0.0.0.0"
});
```

That’s it! Let’s see what Pulumi thinks it needs to do to get to this new desired state by running

```bash
> pulumi up
```

Unfortunately, running this command fails with an error that looks like this

> error: constructing secrets manager of type "passphrase": unable to find either `PULUMI_CONFIG_PASSPHRASE` or `PULUMI_CONFIG_PASSPHRASE_FILE` when trying to access the Passphrase Secrets Provider; please ensure one of these environment variables is set to allow the operation to continue

What? Why is that? Did you do something wrong?

No, not at all! The first time you run `pulumi up`, you are allowed to manually input the passphrase in the terminal. However, as soon as it has run once, it requires the passphrase to be provided either as an environment variable called `PULUMI_CONFIG_PASSPHRASE`, or through a file, whose path is added to an environment variable called `PULUMI_CONFIG_PASSPHRASE_FILE`. 

In this case, it is just a simple matter or defining an environment variable called `PULUMI_CONFIG_PASSPHRASE`, containing the passphrase for the stack.

In my case, I used the passphrase `test123!`, and I'm using PowerShell, so that means that I need to run a command like this

```bash
> $env.PULUMI_CONFIG_PASSPHRASE = "test123!"
```

However, it does depend on what terminal you are using. For example, if you use bash, you would instead have to run

```bash
> export PULUMI_CONFIG_PASSPHRASE=test123!
```

Once this environment variable has been defined, you can re-run `pulumi up`

```bash
> pulumi up

Previewing update (dev):
      Type                              Name                  Plan       
      pulumi:pulumi:Stack               pulumi-demo-dev                  
  +   ├─ azure-native:sql:Server        pulumi-demo-sql-demo  create     
  +   ├─ azure-native:sql:Database      pulumi-demo-db-demo   create     
  +   └─ azure-native:sql:FirewallRule  allowAllAzureIps      create     
  
Resources:
    + 3 to create
    2 unchanged
```

Ok, so Pulumi thinks it needs to add 3 resources, which looks correct. 

However, if you look at the output, it is presented in a tree structure. This indicates that Pulumi has a parent/child relationship between all its resources. And by default, resources will be created as children to the stack. Luckily, it is quite easy to update the relationships to get a better semantic representation. All you have to do, is to use a 3rd constructor parameter for the resources.

The 3rd parameter allows you to set a ton of options for the resource. Things like parent/child relationships and resource dependencies that Pulumi can't figure out on its own, as well as a ton of more advanced things.

In this case, it's enough to update the `parent` property to set up the correct parent/child relationships.

```typescript
const sql = new azure.sql.Server(getName("sql"), {
  ...
}, { parent: resourceGroup });

const db = new azure.sql.Database(getName("db"), {
  ...
}, { parent: sql });

new azure.sql.FirewallRule("allowAllAzureIps", {
  ...
}, { parent: sql });

```

This adds the resource group as the parent for the SQL Server, and the SQL Server as the parent for the database and firewall rule. Just as it should be.

Not only does this mean that child resources can never be orphaned, as the TypeScript code will fail to compile if the parent is removed. But it will also enable Pulumi to give us a better view of the relationships between the resources when running `pulumi up`

```typescript
> pulumi up

Previewing update (dev):
     Type                                     Name                  Plan       
     pulumi:pulumi:Stack                      pulumi-demo-dev                  
     └─ azure-native:resources:ResourceGroup  PulumiDemo                       
 +      └─ azure-native:sql:Server            pulumi-demo-sql-demo  create     
 +        ├─ azure-native:sql:FirewallRule   allowAllAzureIps      create     
 +         └─ azure-native:sql:Database       pulumi-demo-db-demo   create     
 
Resources:
    + 3 to create
    2 unchanged
```

This time the resource relationships look much better, so go ahead and select __yes__ to deploy the update.

Once the update has been deployed, you should have an environment that looks something like this

![Environment with database deployed](/assets/images/iac/pulumi-environment-database.png "Environment with database deploye")

Now that the database has been created, you can start focusing on the web app!

## Creating the Azure Web App

The first thing you need to host a web app, is an app service plan. And just as with all other resources, it is just a matter of creating an instance of one of the classes in the SDK. In this case the `azure.web.AppServicePlan` class.

```typescript
const svcPlan = new azure.web.AppServicePlan(getName("plan"), {
  resourceGroupName: resourceGroup.name,
  sku: {
    name: "F1"
    tier: "Free",
  },
});
```

All you need to define is the resource group to place it in, and the SKU.

Although...you also need to remember to set the correct parent, which in this case is the resource group

```typescript
const svcPlan = new azure.web.AppServicePlan(getName("plan"), {
  ...
}, { parent: resourceGroup });
```

Cool, now you have an App Service Plan! The next step is the actual web app.

To add a Web App, you need to instantiate yet another resource. In this case an `azure.web.WebApp`, which needs to be configured with a resource group and an App Service Plan. However, instead of using a property called `appServicePlanId`, as you expect, it uses one called `serverFarmId`. The reason for this is that this is what it is called in ARM for some reason. And since Pulumi is auto generated from the Azure API's OpenApi spec, it picks up that name.

It should look something like this

```typescript
const web = new azure.web.WebApp(getName("web"), {
  resourceGroupName: resourceGroup.name,
  serverFarmId: svcPlan.id
}, { parent: svcPlan })
```

Oh, yeah, don't forget to add the `parent` in there as well.

However, if you want to make it run ASP&#46;NET Core applications, you also need to update the web app's site configuration, and set the correct .NET Framework version.

__Note:__ Yes, weirdly enough you need to set the .NET Framework version, even if it is .NET Core you want to use. However, you need to set it to "v5.0", which doesn't even exist in .NET Framework... Slightly confusing, but there is nothing we can do about it. On the other hand, if you want to configure a Linux-based web app to use .NET Core, you need to set the "Linux Fx version", which makes even less sense to be honest...

Anyhow, to set the app's site configuration, you use the `azure.web.WebApp`'s `siteConfig` property. This is set using an object, inside which you can set the `netFrameworkVersion` property to `v5.0` to configure it to use .NET Core. It looks like this

```typescript
const web = new azure.web.WebApp(getName("web"), {
  ...
  siteConfig: {
    netFrameworkVersion: "v5.0"
  }
}, ...)
```

That should take care of the .NET Core stuff. However, you also need to add a connection string to it, to allow the application to talk to the database. This is quite easily done by setting the `siteConfig`'s `connectionStrings` property.

The `connectionStrings` property is defined as an array, containing all the connection strings that the app needs. In this case, you only need a single connection string called __connectionstring__. However, you can't just the connection string to the array. Instead, you need to add an object that also contains a `type` and `name` property, like this

```typescript
const web = new azure.web.WebApp(getName("web"), {
  ...,
  siteConfig: {
    ...,
    connectionStrings: [
      {
        name: "connectionstring",
        type: "SQLAzure",
        connectionString: `Data Source=tcp:${sql.fullyQualifiedDomainName},1433;Initial Catalog=${db.name};User Id=server_admin;Password='P@ssw0rd1!';`
      }
    ]
  },
}, ...)
```

As you can see, the actual connection string is created by using string interpolation. 

__Note:__ Yes, that's another set of hard coded credentials... But I promise that that will be fixed later in this post!

The problem with this string interpolation is that the `sql.fullyQualifiedDomainName` and `db.name` properties are not actually strings. Instead, they are defined as `pulumi.Output<string>`. This is Pulumi's way of handling asynchronous properties. 

So, what do I mean by asynchronous properties? Well, some property values aren't actually known at "compile time". Instead, these values are generated by Azure, and returned as part of the resource creation. So, to be able to use these values, Pulumi needs to wait for the resource to be created, before it can get hold of the values, and carry on creating and configuring the resource that depend on them. To handle this asynchronous situation, Pulumi uses `pulumi.Output<T>`, which is a lot like TypeScript/JavaScript's `Promise<T>`. Pulumi can then give all `pulumi.Output<T>` instances special handling inside Pulumi.

Now that you know that the `sql.fullyQualifiedDomainName` and `db.name` are defined as `pulumi.Output<string>`, it probably makes sense that the string interpolation won't work, as TypeScript has no idea of how to handle `pulumi.Output<T>`. Becasue of this, you need to tell Pulumi to handle the interpolation for you. This can be achieved in several ways, but when you need to use more than one output, you need to use `pulumi.all()` to turn multiple outputs into a single combined output. And once you are down to a single output, you can use the `apply()` method from the `pulumi.Output<T>` class to asynchronously get hold of the resolved values, by passing in a callback that will be called when all the values have been resolved.

It comes out looking like this

```typescript
const web = new azure.web.WebApp(getName("web"), {
  ...,
  siteConfig: {
    ...,
    connectionStrings: [
      {
        ...,
        connectionString: pulumi.all([sql.fullyQualifiedDomainName, db.name])
                            .apply(([fqdn, dbName]) => `Data Source=tcp:${fqdn},1433;Initial Catalog=${dbName};User Id=server_admin;Password='P@ssw0rd1!';`)
      }
    ]
  },
}, ...)
```

This might look a bit complicated, and I completely agree! But I do suggest studying the above code a bit, to make sure that you at least get the general gist of what it does. It is a very important part of Pulumi, so understanding it is a bit important.

On the other hand, I think it might be easier to understand, if I also provide a more basic example with just a single `pulumi.Output<T>`. The `pulumi.all()` method unfortunately makes it extra complicated...

If you imagine that you only cared about the `sql.fullyQualifiedDomainName` property, the syntax becomes a bit simpler

```typescript
sql.fullyQualifiedDomainName.apply(fqdn => `Data Source=tcp:${fqdn},1433;`)
```

If you are familiar with JavaScript/TypeScript's `Promise<T>`, it is definitely very similar, and should be somewhat easy to understand. However, if you are coming from a different background, it might take a little while to get used to. But trust me, it will make sense quite quickly, even it looks weird now!

Luckily, when using `pulumi.Output<T>`, Pulumi can automatically figure out dependencies between different resources. In this case, it automatically figures out that the web app depends on both the SQL Server and database resources. However, in some cases, it isn't that simple. In those cases, Pulumi requires you to explicitly configure the dependencies by setting a `dependsOn` property on the third constructor parameter. Like this

```typescript
const web = new azure.web.WebApp(getName("web"), {
  ...
}, { 
  parent: svcPlan,
  dependsOn: [ sql, db ]
})
```

Yes, in this case it isn't necessary, but I thought it was worth showing. And also, it doesn't cause any problems if you do set it explicitly even if Pulumi can figure it out...

Now that you have defined all the resources needed for the application, you can move your focus to the "supporting" resources. And by that, I mean the Application Insights resource, and the Log Analytics Workspace that it will use to store its data.

## Setting up Application Insights using a Log Analytics Workspace

Let's start with the Log Analytics Workspace, which is _really_ simple to define.

```typescript
const workspace = new azure.operationalinsights.Workspace(getName("laws"), {
  resourceGroupName: resourceGroup.name
}, { parent: resourceGroup });
```

It really only needs a name, a resource group and a parent in this case. Sure, it can be made a lot more complex, but for this demo, we can simply use the default values for pretty much all of the settings. However, if you need a more complex set up, this is obviously supported as well!

Setting up the Application Insights resource is a different story though... Unfortunately, the `@pulumi/azure_native` SDK doesn't support setting up Application Insights with a Log Analytics Workspace back-end. There are 2 ways to fix this. One is to manually create a custom resource that sets it up for you. But this is a bit complicated to be honest... The other is to use the "old" `@pulumi/azure` SDK, as this contains a resource that allows you to set up this configuration.

Luckily, the Pulumi state management allows us to combine different SDK:s in the same project. So, all you need to do, to use resources from the "old", hand-crafted SDK, is to add the package, and start using it.

To add the SDK, you just run

```bash
> npm i @pulumi/azure
```

Once that has been added, you can go ahead and add an import for that package in the index.ts file like this

```typescript
import * as az from "@pulumi/azure";
```

That's it! You now have full access to all the resources in that SDK as well. And the resource you need in this case is the `az.appinsights.Insights`. This resource needs a resource group, an application type, a workspace ID and a parent, like this

```typescript
const ai = new az.appinsights.Insights(getName("ai"), {
  resourceGroupName: resourceGroup.name,
  applicationType: "web",
  workspaceId: workspace.id,
}, { parent: resourceGroup });
```

That's all there is to it! 

The ability to combine different SDK:s allows for some really cool things! In this case it was a combination of 2 different Azure SDK:s, but it could just as well have been a combination of the Azure SDK and the AWS SDK for example.

At this point, the __index.ts__ file is getting quite large... However, since we are using TypeScript, we can remedy this quite easily. You could for example go ahead and just move some of the resources into a separate TypeScript file, and reference that file from __index.ts__. But we can do even better with Pulumi!

In this demo, the Log Analytics Workspace and the Application Insights resources are quite tightly coupled. Because of this, you might as well combine them into a single resource.

__Note:__ In this case, this might make sense, but in a real application I would probably not recommend this specific combination of resources. Why? Well, you might want to have several Application Insights resources writing to the same workspace. But the important part right now is to show you how to combine resources, not to do it "the right way".

So, go ahead and create a new TypeScript file called __app-insights.ts__, and add the required imports

```typescript
import * as pulumi from "@pulumi/pulumi";
import * as azure from "@pulumi/azure-native";
import * as az from "@pulumi/azure";
```

Next, you need to "export" a new class called __WorskpaceBasedApplicationInsights__, inheriting from `pulumi.ComponentResource`.

__Note:__ `pulumi.ComponentResource` is a base class specifically made for combining several resources into a reusable unit

The `pulumi.ComponentResource` class has a constructor that accepts a "type name", a name, an "arguments" object and a `pulumi.ResourceOptions`. Basically a "type name" plus the 3 standard resource parameters So, the __WorskpaceBasedApplicationInsights__ class declaration ends up looking like this

```typescript
export class WorskpaceBasedApplicationInsights extends pulumi.ComponentResource {
  constructor(private name: string, private args: any, opts?: pulumi.ResourceOptions) {
    super("pulumidemo:components:WorskpaceBasedApplicationInsights", name, args, opts);
  }
}
```

The constructor arguments are identical to the ones you would use for a built-in resource. So, using this "custom" resource will feel just like using any other resource.

The first parameter to the base constructor (`super()`) is a "type name". This is a string identifier for this specific type, which is used internally by Pulumi to keep track of the type.

Now that you have this new class defined, you can move the workspace and AI resource declarations inside the constructor.

```typescript
export class WorskpaceBasedApplicationInsights extends pulumi.ComponentResource {
  constructor(private name: string, private args: any, opts?: pulumi.ResourceOptions) {
    super("pulumidemo:components:WorskpaceBasedApplicationInsights", name, args, opts);
    
    const workspace = new azure.operationalinsights.Workspace(getName("laws"), {
      resourceGroupName: resourceGroup.name
    }, { parent: resourceGroup });
    
    const ai = new az.appinsights.Insights(getName("ai"), {
      resourceGroupName: resourceGroup.name,
      workspaceId: workspace.id,
      applicationType: "web",
    }, { parent: resourceGroup });
  }
}
```

Unfortunately, this move has some problems. For example that the `resourceGroup` variable and `getName()` function isn't available. To sort this out, you need to use the `args` argument, and get the required values passed into the constructor. However, the `args` parameter is currently typed as `any`, which isn't that great, as it won't tell the consumers of this class what properties they can pass in. This is easy to fix using an interface though

```typescript
interface WorskpaceBasedApplicationInsightsArgs {
  resourceGroupName: pulumi.Input<string>
  workspaceName: string
  insightsName: string
}

export class WorskpaceBasedApplicationInsights extends pulumi.ComponentResource {
  constructor(..., private args: WorskpaceBasedApplicationInsightsArgs, ...) {
    ...
  }
}
```

__Note:__ Take note of the use of `pulumi.Input<string>` for the `resourceGroupName` property. This type allows you to either set the property using a string, or a `pulumi.Output<string>`!

With the `args` "typed", you can update the resource definitions to use the passed in values as follows

```typescript
export class WorskpaceBasedApplicationInsights extends pulumi.ComponentResource {
  constructor(private name: string, private args: any, opts?: pulumi.ResourceOptions) {
    super("pulumidemo:components:WorskpaceBasedApplicationInsights", name, args, opts);
    
    const workspace = new azure.operationalinsights.Workspace(args.workspaceName, {
      resourceGroupName: args.resourceGroupName
    }, { parent: this });
    
    const ai = new az.appinsights.Insights(args.insightsName, {
      resourceGroupName: args.resourceGroupName,
      workspaceId: workspace.id,
      applicationType: "web",
    }, { parent: this });
  }
}
```

As you might see, the resource names and the `resourceGroupName` property have all been updated to use the passed in arguments. On top of that, the `parent` property has been set to `this`, as the resources are now children of the `WorskpaceBasedApplicationInsights` class and not the resource group.

There is only one thing left to do in this class, and that is to expose any properties that you might want to have access to when using this class. In this case, it is very likely that you will want to use the `instrumentationKey` and `connectionString` properties on the `az.appinsights.Insights` instance.

There are 2 ways to expose these properties. One is to simply turn the `ai` instance into a public field, which would allow anyone using this class to access these value, as well as any other value on the `az.appinsights.Insights` instance. The other is to use a bit more encapsulation, and expose individual properties for the values. Like this

```typescript
export class WorskpaceBasedApplicationInsights extends pulumi.ComponentResource {
  private ai: az.appinsights.Insights;

  constructor(private name: string, private args: WorskpaceBasedApplicationInsightsArgs, opts?: pulumi.ResourceOptions) {
    ...
    this.ai = new az.appinsights.Insights(…);
  }

  get instrumentationKey() {
    return this.ai.instrumentationKey;
  }
  get connectionString() {
    return this.ai.connectionString;
  }
}
```

Both options are definitely viable. To me, it depends on how many properties you want to expose. If it is only a few, then I think this approach is a bit cleaner. But, if you want to expose a _lot_ of properties, simply exposing the instance might be easier.

Now that we have this new type defined, you can go back to the __index.ts__ file, add an import it, and then create an instance of `WorskpaceBasedApplicationInsights` using the following code

```typescript
...
import { WorskpaceBasedApplicationInsights } from "./app-insights";
...
const ai = new WorskpaceBasedApplicationInsights(getName("ai"), {
  resourceGroupName: resourceGroup.name,
  workspaceName: getName("laws"),
  insightsName: getName("ai")
}, { parent: resourceGroup });
```

That's pretty nice in my opinion! Imagine all the nice semantics you can show by using this type of code! And how easily you can distribute standardized resources inside your company by adding them to your own, custom npm packages!

Cool, now you have the Application Insights stuff set up! But you still need to add some configuration to the web app to "connect" them. This is done by setting a couple of app settings.

However, before you go and do that, you need to make sure that the `WorskpaceBasedApplicationInsights` instance is moved up above the creation of the web app. Otherwise you won't be able to use it to set the values...

To set the web app's app settings, you use the `appSettings` property on the `siteConfig` object. This property is declared as an array of objects containing `name` and `value` properties. So, to set an app setting, you need to use a syntax that looks like this

```typescript
const web = new azure.web.WebApp(getName("web"), {
    ...,
  siteConfig: {
    ...,
    appSettings: [
      { 
        name: "APP_SETTING_NAME", 
        value: "APP_SETTING_VALUE"
      }
    ]
  },
}, ...)
```

And once again, since you are using TypeScript, the editor should be able to help you to figure it out...

The settings needed for Application Insights are

-	APPINSIGHTS_INSTRUMENTATIONKEY – The unique key generated by the Application Insights resource
-	APPLICATIONINSIGHTS_CONNECTION_STRING – The connection string to the Application Insights resource
-	ApplicationInsightsAgent_EXTENSION_VERSION – Set to `~3` for Linux and `~2` for Windows
-	XDT_MicrosoftApplicationInsights_Mode – Set to `recommended` for...well...recommended data gathering

So, you need to set these settings using the following code

```typescript
const web = new azure.web.WebApp(getName("web"), {
  ...,
  siteConfig: {
    ...,
    appSettings: [
      { 
        name: "APPINSIGHTS_INSTRUMENTATIONKEY", 
        value: ai.instrumentationKey
      },
      {
        name: "APPLICATIONINSIGHTS_CONNECTION_STRING",
        value: ai.connectionString
      },
      {
        name: "ApplicationInsightsAgent_EXTENSION_VERSION",
        value: "~2"
      },
      {
        name: "XDT_MicrosoftApplicationInsights_Mode",
        value: "recommended"
      }
    ]
  },
}, ...)
```
That's "all" there is to it! 

At this point, the infrastructure is defined as needed. However, there are quite a few hard coded values that would make sense to have as configuration instead. This would allow us to re-use this code to create multiple environments with slightly different settings. It might for example be useful to have a smaller app service plan for dev/test, than for production. And with all the power of TypeScript at your fingertips, just imagine how much customization you could easily achieve by adding just some quite basic configuration.

## Configuration using "stacks"

Pulumi comes with a built-in configuration system. It is based around the idea of a __stack__. A stack is basically a named set of configuration settings that you can use when deploying your project. Currently, you have a single stack called __dev__. This is represented by the __Pulumi.dev.yaml__ file in the root of the application.

Right now, that stack contains a single property called `azure-native:location`. This was set during project creation when you answered what default Azure location you wanted to use. You can see it by opening the __Pulumi.dev.yaml__ file

```yaml
encryptionsalt: v1:IykIQhHawfU=:v1:+I7tf853kbGI2RmW:iLKunFMrscgDDwEAV93fn3KwqRw0CA==
config:
  azure-native:location: WestEurope
```

There are a couple of things to note in this file. First of all, the `encryptionsalt` property. This contains the information needed when adding encrypted values to the config. Generally, you can ignore this, but now you at least know what it is.

__Note:__ Secrets can be stored in other locations as well. Your Pulumi project can for example be set up to store them in Azure KeyVault instead. But the ability to just store them in the YAML-file, encrypted with a passphrase, is quite nice.

Next, you should note that all config keys are prefixed in some way. This allows 3rd party packages to define config keys in a way that doesn't interfere with other packages, or your local keys. In the __Pulumi.dev.yaml__ file, the `location` key is prefixed with `azure-native`, indicating that it is a configuration used by the `@pulumi/azure_native` package. Any local config key is prefixed with the current project name.

To add a configuration setting, you can either add it to the YAML-file manually, or by using the Pulumi CLI. The upside to using the CLI, is that it will make sure to correctly prefix the key for you. And secondly, it allows you to easily add encrypted secrets by simply adding a `--secret` parameter.

For this project, you need to add the following configuration settings

- appSvcPlanSkuSize - set to "F1"
- appSvcPlanSkuTier - set to "Free"
- sqlSize - set to "Basic"
- sqlUser - set to "server_admin"
- sqlPassword - set to your password of choice, and encrypted

To add these using the Pulumi CLI, you just need to run the following commands

```bash
> pulumi config set appSvcPlanSkuSize F1
> pulumi config set appSvcPlanSkuTier Free
> pulumi config set sqlSize Basic
> pulumi config set sqlUser server_admin
> pulumi config set sqlPassword P@ssw0rd1! --secret
```

If you open the __Pulumi.dev.yaml__ file after running these commands, you will see something that looks similar to this

```yaml
encryptionsalt: v1:IykIQhHawfU=:v1:+I7tf853kbGI2RmW:iLKunFMrscgDDwEAV93fn3KwqRw0CA==
config:
  azure-native:location: WestEurope
  pulumi-demo:appSvcPlanSkuSize: F1
  pulumi-demo:appSvcPlanSkuTier: Free
  pulumi-demo:sqlPassword:
    secure: v1:Oql4A3yQJ1PJ7W+5:BBSCfpFLr+x8CkQEQqv5r7uASjECSK5f
  pulumi-demo:sqlSize: Basic
  pulumi-demo:sqlUser: server_admin
```

As you can see, all the config keys have been prefixed with the project name `pulumi-demo`. And the `sqlPassword` setting has been encrypted for you, since you used the `--secret` parameter.

Now that the configuration has been set up, you just need to update the code to make use of it.

The first step is to create an instance of `pulumi.Config()`, which is a class that allows you to easily get hold of the configuration values you need, by calling methods like `require()` and `get()`.

The `require()` and `get()` methods will assume the type to be string. For other types, there are corresponding methods, for example `getNumer()` and `requireNumber()` for numbers.

__Note:__ Methods prefixed with __require__, for example `requireNumber()`, will throw an exception if the value is missing, while the corresponding "__get__ method", `getNumber()`, will return undefined if it is missing.

```typescript
const config = new pulumi.Config();
```

The second step is to update all the places where the configuration values should be used

```typescript
const sql = new azure.sql.Server(getName("sql"), {
  ...,
  administratorLogin: config.require("sqlUser"),
  administratorLoginPassword: config.requireSecret("sqlPassword")
}, ...);

const db = new azure.sql.Database(getName("db"), {
  ...,
  sku: {
      name: config.require("sqlSize")
  },
}, ...);

const svcPlan = new azure.web.AppServicePlan(getName("plan"), {
    ...,
    sku: {
        name: config.require("appSvcPlanSkuName"),
        tier: config.require("appSvcPlanSkuTier")
    },
}, ...);

const web = new azure.web.WebApp(getName("web"), {
  ...,
  siteConfig: {
    ...,
    connectionStrings: [
      {
        ...,
        connectionString: `Data Source=tcp:${sql.fullyQualifiedDomainName},1433;Initial Catalog=${db.name};User Id=${config.require(sqlUser)};Password='${config.require(sqlPassword)}';`
      }
    ]
  },
}, ...)
```

Now that the configuration is in place, you can try to deploy the infrastructure by running

```bash
> pulumi up

Previewing update (dev):
      Type                                                           Name                  Plan       
      pulumi:pulumi:Stack                                            pulumi-demo-dev                  
      └─ azure-native:resources:ResourceGroup                        PulumiDemo                       
  +      ├─ pulumidemo:components:WorskpaceBasedApplicationInsights  pulumi-demo-ai    create     
  +      │  ├─ azure-native:operationalinsights:Workspace            pulumi-demo-laws  create     
  +      │  └─ azure:appinsights:Insights                            pulumi-demo-ai    create     
  +      └─ azure-native:web:AppServicePlan                          pulumi-demo-plan  create     
  +         └─ azure-native:web:WebApp                               pulumi-demo-web   create     
  
Resources:
    + 5 to create
    5 unchanged
```

It's worth noting the `pulumidemo:components:WorskpaceBasedApplicationInsights` entry. This is your custom type, underneath which you can see the workspace and Application Insights resources.

I suggest selecting __no__ to update the environment in this case, as there is a little piece missing...

It's very common that we need to get hold of values generated by the IaC code from the outside, like for example the name of a SQL Server database, or the address to a web application. In Pulumi, this is solved by using output properties.

## Output properties

When you need to expose values from the IaC code, you have to rely on JavaScript/TypeScript's ability to "export" values from a module. You simple declare the value you want to export as an exported value. For example, if you want to expose the address of the created web app, you can simply add the following code at the end of the __index.ts__ file

```typescript
export const websiteAddress = web.defaultHostName
```

Once that is in place, you can run `pulumi up`. And since we know that the code looks good, you can go ahead and add the `-y` parameter, which will automatically approve the update without an interactive prompt.

```typescript
pulumi up -y
Previewing update (dev):
  ... 
  
Outputs:
  + websiteAddress: output<string>

Resources:
    + 5 to create
    5 unchanged

Updating (dev):
      Type                                                           Name                  Status      
      pulumi:pulumi:Stack                                            pulumi-demo-dev                   
      └─ azure-native:resources:ResourceGroup                        PulumiDemo                        
  +      ├─ pulumidemo:components:WorskpaceBasedApplicationInsights  pulumi-demo-ai    created     
  +      │  ├─ azure-native:operationalinsights:Workspace            pulumi-demo-laws  created     
  +      │  └─ azure:appinsights:Insights                            pulumi-demo-ai    created     
  +      └─ azure-native:web:AppServicePlan                          pulumi-demo-plan  created     
  +         └─ azure-native:web:WebApp                               pulumi-demo-web   created     
  
Outputs:
  + websiteAddress: "pulumi-demo-dev-webd442b66a.azurewebsites.net"

Resources:
    + 5 created
    5 unchanged
```

Ok, that looks like it worked. It seems like it created the new resources, and it output the new `websiteAddress` output to the console. And if you have a look at the Azure portal, you should now have something that looks like this

![Azure portal with resources created](/assets/images/iac/pulumi-azure-portal.png "Azure portal with resources created")

__Note:__ When running in a non-interactive environment, like for example in a deployment pipeline, the `-y` parameter is _very_ important to add. Without it, the `pulumi up` command will just lock up waiting for input. Or simply just fail, because it is running in a non-interactive session.

However nice it is to have the website address written to the output, the goal is obviously to be able to get hold of the value programmatically. This is done using the `pulumi stack output` command, which clearly indicates that the output is stored per stack. There are two ways to use this command. You either run it without any extra parameters, which will get you all the outputs like this

```bash
> pulumi stack output

Current stack outputs (1):
    OUTPUT          VALUE
    websiteAddress  pulumi-demo-dev-webd442b66a.azurewebsites.net
```

Sure, there is only a single output here. But if you had more than one, they would all show up when using this command.

Or, you can add the name of the output you want to get hold of as the last parameter, and get that specific value. Like this

```bash
> pulumi stack output websiteAddress

pulumi-demo-dev-webd442b66a.azurewebsites.net
```

However, the `websiteAddress` output looks a little naked. It would be really nice if that included the "https://" prefix and "/" suffix...

To add the prefix and suffix to the address, you could use the `apply()` method from `pulumi.Output<string>`, as you did before. Or, you can use this other interpolation syntax that relies on a pretty cool JavaScript feature that allows you to extend JavaScript's own string interpolation syntax.

The syntax for this way of performing interpolation looks like this

```typescript
export const websiteAddress = pulumi.interpolate `https://${web.defaultHostName}/`
```

You simply prefix the native JavaScript "backtick interpolation" with `pulumi.interpolate`, and then you use the `pulumi.Output<string>` as if it was a regular string. Pulumi will then figure out the rest.

With that in place, you can re-deploy the environment, which is basically a no-op

```typescript
> pulumi up -y
...
Outputs:
  ~ websiteAddress: "pulumi-demo-dev-web442b66a.azurewebsites.net" => "https://pulumi-demo-dev-webd442b66a.azurewebsites.net/"
...
```

If you browse to the output address, you should now end up at a website with the default "empty web app screen".

![Azure emtpy app screen](/assets/images/iac/pulumi-empty-app-screen.png "Azure empty app screen")

## Using multiple stacks

If you have multiple environments, you use multiple stacks to store the configuration for the different environments.

To create a new stack, you just need to run the following command

```bash
> pulumi stack init prod
```

where __prod__ is the name of the stack.

This will create a new empty stack, which is represented by a new __Pulumi.prod.yml__ file in the root of your project. It will also set this new stack as the "active" stack. 

__Note:__ By "active" stack, I mean that it is the stack that is used when running Pulumi commands.

However, since this stack is completely empty, you will need to set up the required configuration for the __prod__ stack. There are 2 ways of doing this. Either you open the __Plumi.dev.yml__ and copy the config values across, or you run the `pulumi config set` commands again with this stack selected.

In this case, I suggest copying across the non-encrypted values like this

```yaml
encryptionsalt: ...
config:
  azure-native:location: WestEurope
  pulumi-demo:appSvcPlanSkuName: F1
  pulumi-demo:appSvcPlanSkuTier: Free
  pulumi-demo:sqlSize: Basic
  pulumi-demo:sqlUser: server_admin
```

__Note:__ Do __not__ copy across the `encryptionsalt`, this should be left alone!

Once the non-encrypted values have been set up, you can set up the encrypted values by using the Pulumi CLI

```bash
> pulumi config set sqlPassword P@ssw0rd1! --secret
```

Sure, you probably want different configuration between the stacks, otherwise it doesn't make sense to have multiple stacks. But once again...it is a demo... 

It might also be a good idea to include the stack name in the resource names. Luckily, this is easily fixed by modifying the `getName()` method, and updating the resource group name like this

```typescript
function getName(type: string) {
  return `${ pulumi.getProject().toLowerCase() }-${ pulumi.getStack() }-${ type }`;
}

const resourceGroup = new azure.resources.ResourceGroup("PulumiDemo", {
  resourceGroupName: `PulumiDemo-${ pulumi.getStack() }`
});
```

As you can see, the names now include the stack name, which is retrieved using the `pulumi.getStack()` method.

If you try running `pulumi up`, with the __prod__ stack selected, and select __details__, you should get the following output

```bash
+ pulumi:pulumi:Stack: (create)
  [urn=urn:pulumi:prod::pulumi-demo::pulumi:pulumi:Stack::pulumi-demo-prod]
  + azure-native:resources:ResourceGroup: (create)
    [urn=urn:pulumi:prod::pulumi-demo::azure-native:resources:ResourceGroup::PulumiDemo]
    [provider=urn:pulumi:prod::pulumi-demo::pulumi:providers:azure-native::default_1_45_0::04da6b54-80e4-46f7-96ec-b56ff0331ba9]
    location         : "WestEurope"
    resourceGroupName: "PulumiDemo-prod"
    + azure-native:sql:Server: (create)
      [urn=urn:pulumi:prod::pulumi-demo::azure-native:resources:ResourceGroup$azure-native:sql:Server::pulumi-demo-prod-sql]
      [provider=urn:pulumi:prod::pulumi-demo::pulumi:providers:azure-native::default_1_45_0::04da6b54-80e4-46f7-96ec-b56ff0331ba9]
      administratorLogin        : "server_admin"
      administratorLoginPassword: "[secret]"
      location                  : "WestEurope"
      resourceGroupName         : output<string>
      serverName                : "pulumi-demo-prod-sql765429b5"
```

Here you can see that the stack name is being added to the names. For example, the __serverName__ is now set to `pulumi-demo-prod-sql765429b5`, just as we wanted. But you can also see that the configuration values are being picked up properly, with the __administratorLoginPassword__ set to `[secret]`, as it is a secret, encrypted value that should not be output in any logs.

There is no reason to deploy this stack, so I suggest selecting __no__, when asked if you want to update the deployment.

You can try switching back to the __dev__ stack using the following command

```bash
> pulumi stack select dev
```

Once you have selected the __dev__ stack, you can perform a preview to see what changes Pulumi think it needs to perform

```bash
pulumi preview

Previewing update (dev):
      Type                                                           Name                  Plan        Info
      pulumi:pulumi:Stack                                            pulumi-demo-dev                   
      └─ azure-native:resources:ResourceGroup                        PulumiDemo                        
  +      ├─ pulumidemo:components:WorskpaceBasedApplicationInsights  pulumi-demo-dev-ai    create      
  +      │  ├─ azure-native:operationalinsights:Workspace            pulumi-demo-dev-laws  create      
  +      │  └─ azure:appinsights:Insights                            pulumi-demo-dev-ai    create      
  +      ├─ azure-native:web:AppServicePlan                          pulumi-demo-dev-plan  create      
  +      │  └─ azure-native:web:WebApp                               pulumi-demo-dev-web   create      
  +      ├─ azure-native:sql:Server                                  pulumi-demo-dev-sql   create      
  +-     │  ├─ azure-native:sql:FirewallRule                         allowAllAzureIps      replace     [diff: ~serverName]
  +      │  └─ azure-native:sql:Database                             pulumi-demo-dev-db    create      
  -      ├─ azure-native:web:AppServicePlan                          pulumi-demo-plan      delete      
  -      │  └─ azure-native:web:WebApp                               pulumi-demo-web       delete      
  -      ├─ azure-native:sql:Server                                  pulumi-demo-sql       delete      
  -      │  └─ azure-native:sql:Database                             pulumi-demo-db        delete      
  -      └─ pulumidemo:components:WorskpaceBasedApplicationInsights  pulumi-demo-ai        delete      
  -         ├─ azure:appinsights:Insights                            pulumi-demo-ai        delete      
  -         └─ azure-native:operationalinsights:Workspace            pulumi-demo-laws      delete      
  
Outputs:
  ~ websiteAddress: "https://pulumi-demo-webd442b66a.azurewebsites.net/" => output<string>

Resources:
    + 7 to create
    - 7 to delete
    +-1 to replace
    15 changes. 2 unchanged

```

As you can see, Pulumi thinks that all the resources have to be recreated. This is because you have changed the names, which causes Pulumi to find a bunch of resources that aren't available anymore, and a bunch of new ones, that it hasn't seen before...

There is no need to run this update, so select __no__ to the update question.

The last step is to remove any resource that you have created. Luckily, this is as simple as running

```bash
> pulumi destroy
```

and selecting __yes__.

Your Azure subscription should now be back to the state it was before you started!

## Conclusion

Pulumi is definitely a different way of doing IaC, than what you have seen in the previous posts. Instead of using a DSL, you get a full programming language to work with. This gives us a ton more flexibility when defining our environment. Couple that with a built-in configuration system that allows you to configure the code using different stacks, and you have a very powerful tool to put in your toolbelt!

I personally find Pulumi to be very interesting way of doing IaC. And as a person with a development background, it feels very natural to me. However, I do completely understand that people with a more operations inspired background might find it a bit awkward and weird. And in those cases, a DSL might feel a bit more natural. Having that said, a lot of operations people are quite comfortable using for example PowerShell and Bash scripts. So, I don't think it is impossible for people with a background like that to like Pulumi.

One thing that I'm not too thrilled about, when it comes to Pulumi, is the state management. Pulumi only uses with its own state when evaluating the desired state, which means that only properties explicitly defined in your code will be updated during a deployment. This unfortunately means that there could be quite a bit of configuration drift in your environment without Pulumi reversing it. ARM/Bicep solves this by not storing any "local" state at all, and instead looks at the actual environment. Terraform on the other hand, has the ability to combine its own state with the actual environment state before trying to figure out what has changed. This allows it to reset, not only the explicitly defined properties, but also the ones left as defaults, making the risk for configuration drift less.

Having that said, I still find the Pulumi paradigm really cool, and comfortable to work with. So even if I find the state handling slightly flawed, I still find it a very good option when looking for an IaC solution.

The next part, ["Conclusion"](/iac-an-intro-part-7), contains my final thoughts on these technologies. It takes a look at them side-by-side, in an effort to highlight the pros and cons of them. 

Feel free to reach out and give feedback or ask questions! I’m available on Twitter [@ZeroKoll](https://twitter.com/zerokoll).