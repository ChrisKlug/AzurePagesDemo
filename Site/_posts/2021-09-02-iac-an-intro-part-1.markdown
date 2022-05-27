---
layout: post
current: post
cover:  /assets/images/covers/blueprint.jpg
smallcover:  /assets/images/covers/blueprint-small.jpg
navigation: True
title: Infrastructure as Code - An intro - Part 1
date: 2021-09-02 10:37:00
tags: [infrastructure, infrastructure as code, iac]
class: post-template
subclass: 'post'
author: zerokoll
---
Infrastructure as Code is a really interesting concept! But what is it...really? Well, let's have a look at it and see if we can't make it less of a "sales pitch". And maybe even make it a thing that you might want to have a look at for your next project! Or maybe even your current one...

For the last many months, I have worked on an infrastructure project for a client. We are basically replicating their current environment using Infrastructure as Code (IaC). In this case, there are a few different reason for doing so. Mainly, that they are setting up a brand new environment in a new subscription in Azure, and felt that doing it in code would give them a lot of extra benefits. Secondly, it enables them to fairly easily spin up new environments to test out features. 

After having worked on that project for a while, I thought it might be a good idea to write a few blog posts about IaC so that people who aren't using it right now, can get an overview of why it might be a good idea, what options are available out there, and also get an intro to some of those options.

## What is Infrastructure as Code?

The first question to answer I think is "What is Infrastructure as Code?". And well, it is exactly what it sounds like...ish. With the advent of cloud computing, we (IT people) all of the sudden had the ability to provision infrastructure (hardware, network etc) without having to go and order physical servers and switches and stuff. Instead, all of that was available through API calls to cloud providers. And this in turn made it possible to write scripts to provision and configure the infrastructure that we need. For example, we can quite easily write a PowerShell script that provisions a webserver and some storage for us, in just a few lines of PowerShell code.

However, being that developers and IT people really enjoy pushing the boundaries of new technology, we now have quite a few more advanced options available to us than just writing PowerShell or bash scripts. But in the end, IaC is basically just the practice of defining the infrastructure that we need in some form of code, or code like format, and then use that to tell our cloud provider to set up the environment we need for our solution.

There are a lot of different options when it comes to how you do this practically, and I will get back to some of the those later on. But there are a few tings that I want to cover before that. Such as _why_ you might want to have your infrastructure defined as code.

## Why Infrastructure as Code?

There actually quite a few reasons for why you might want to have your infrastructure defined as code. So let's have a look at a couple of them.

__Note:__ These are definitely not all of the reasons that you might have. And they definitely are not mutually exclusive. But I thought I would mention a couple of common reasons at least...

### Version management

A really interesting feature of IaC is the ability to version manage not only the code for the application, but also the definition of the infrastructure needed for the code to run. All you have to do, is to check in the IaC code in source control, and boom - your infrastructure is version managed. 

This means not only that there is a track record of the changes that have been made to the infrastructure over time, as well as of who made those changes, but also that you are always able to make sure that the required infrastructure is set up for the specific version of the code that you are trying to run. 

Source control and version management has given us a lot of really good structure and security when it comes to our application code. And using it together with IaC, we get those same benefits when it comes to our infrastructure. Something that was pretty much impossible when we had to buy and install physical infrastructure manually.

### "Recreateability"

Another cool feature of IaC is the "recreateability" (_not a real word, I know, but you know what I mean_) of our infrastructure... By having our infrastructure defined in code, we can quite easily spin up a new environment, or verify the configuration of the current one. This in turn can be used in quite a few ways that make it really useful.

Most software projects use more than one environment. You might for example have a development environment, a QA environment and a production environment. Or maybe you have different environments for different geographical regions. Or even different environments for different clients. Either way, these all need be more or less identical. And sure, you could set them up by painstakingly clicking around in the Azure portal, or by running a bunch of terminal commands that's documented in some readme file somewhere. However, doing it manually several times, has a high potential of getting environments that are "almost identical". Something that can have catastrophic results when moving the application code between environments. With IaC, your environments will always be set up in the same way. Sure, you might have environment differences, such as smaller or fewer instances etc, but with IaC, these are well known differences, created deliberately by design. Not unknown differences caused by mishaps...

Besides the ability to spin up multiple environments for the development cycle etc, the ability to spin up new environments can actually be used for quite a few more things.

You can for example use it to spin up a new environment, or at least a part of an environment, to try out new features. Or maybe even spin up a new environment for each one of your source code branches, or for each one of your PRs. This allows people to review features and PRs in a real environment, instead of having to figure out how to run it locally before being able to try it out. Something that can make a QA's life a LOT easier.

It can also be used for disaster recovery. Sure, having a second environment stand by 24/7 is nice, but it can also be quite expensive. Not to mention that it might pose a few architectural challenges depending on your solution. But having your infrastructure as code isn't that hard, cheap, and can allow you to spin up a new environment quite easily if disaster were to strike. It would still take some time to get it back up and running, depending on the complexity and cloud resource in use, but it would be way faster, and a LOT less stressful, than having to re-create the environment manually. Especially with management breathing down your neck, yelling at you for letting the environment go down. Even if the environment went down because some illegally cloned dinosaurs decided to wreak havoc with your data center in the middle of the night during the Christmas holidays. Something that you should obviously have taken into account when you chose data center...

## Imperative or Declarative IaC

Now that we know some of the reasons for _why_ you might want to define your infrastructure as code, let's have a look at _how_. However, the first thing we need to understand, is that there are 2 different paradigms that can be used, __imperative__ and __declarative__ code.

Both of these paradigms are solving the same problem, however they do it in very different ways.

__Note:__ It might be worth mentioning that there are also some options that are hybrids between the two. For example Pulumi, which I will talk about later on.

### Imperative IaC

With the imperative style, you tell the environment what actions to perform. For example, you can use PowerShell to tell Azure to create a resource group, a service plan and an app service like this

```bash
New-AzResourceGroup -Name "MyRG" -Location "West Europe"
New-AzAppServicePlan -ResourceGroupName "MyRG" -Name "MyAppSvcPlan" -Location "West Europe" -Tier "Free"
New-AzWebApp -ResourceGroupName "MyRG" -Name "MyWebApp" -Location "West Europe" -AppServicePlan "MyAppSvcPlan"
```

This can be put into a PowerShell script file and added to source control, giving you infrastructure as code.

This type of IaC is quite easy to get started with, as it feels very familiar to most developers. And a lot of the command line tools that are made for this, are built to be idempotent, making it easier to run the scripts multiple times. It basically turns a _create_ into an _update_ if the resource already exists, instead of throwing an exception telling you that the resource you are trying to create already exists.

__Note:__ _Idempotent_ means that you can run the command over and over again, and the result will still be the same. So you don't have to check if the command has run before. You can just run it again and again, and it automatically figures out how to handle multiple runs.

Azure PowerShell is unfortunately not idempotent. Well..actually...some of the commands are, some asks you to confirm if you want to turn your create into an update, and some are straight up not. Unfortunately, it seems to be fairly random which commands are, or can be, idempotent, and which aren't/can't. So if you are writing PowerShell IaC scripts that will run multiple times, make sure to verify that the commands you are using commands are idempotent, or if you have to add a `-Force` parameter to make it so. Or if the command isn't idempotent at all, and requires you to check for the existence of a resource before issuing a _create_ or _update_.

Because of this idempotency problem with Azure PowerShell, and some other things, I would recommend using the Azure CLI instead. It is not only idempotent, but it is also cross platform, and much more predictable to work with in my opinion.

The above Azure PowerShell commands correspond to the following Azure CLI commands

```bash
az group create --name MyRG --location westeurope
az appservice plan create --resource-group MyRG --name MyAppSvcPlan --sku FREE
az webapp create --resource-group MyRG --plan MyAppSvcPlan --name MyWebApp
```

These commands can be run over and over again as many times as you want. If the resource already exists, it will just update it with the values provided. If not, it will create it with those values.

Using imperative code to set up an environment feels very natural to most developers. It's also very easy to test and debug. Just run the commands in a terminal and see what happens.

However, it does have a few downsides. The main one, in my opinion, being what happens when someone modifies the resource outside of the IaC code, using an ad-hoc CLI command, or by manually changing the settings in the Azure portal for example. And the annoying answer to that question is, as with so many questions in the IT industry, "it depends". If the changed setting is part of the settings being passed to the CLI, the setting is "reverted" to the provided value. However, if it is a setting that isn't provided in the CLI call, it is left at it's current, modified, state.

For example, imagine that you create an App Service using the commands above, and somebody goes into the portal and adds a system assigned managed identity. This system assigned managed identity will still be assigned to the app if you re-run your script. However, if you were to include the parameter `--assign-identity [system]` in the `az webapp create` command, and somebody manually removed it in the portal, it would be added back in by running these commands again.

This somewhat ambiguous handling of settings can either be considered a benefit or a problem. It is a benefit if you just want to use your IaC code to set up the basics of the environment, and then be able to manually make adjustments to it. On the other hand, that way of working with the environment negates a lot of the benefits of using IaC to begin with to be honest. And it would be a downside if you expected the environment to always look as configured, and didn't expect it to change over time.

Having that said, if the environment isn't open to manual changes, this approach works fine. And as I said, it feels very comfortable and natural for most developers and IT pros.

### Declarative IaC

Writing IaC in a declarative manner is a bit different. Instead of defining what changes need to be performed in the target environment, you define what you want the environment to look like. So instead of telling it to create a resource group, an app service plan and an app service, you tell it that you would like to have an environment that contains these resources. The "system" will then figure out what needs to change to make it so. 

This definition of what you want the environment to look like is referred to as the "desired state". You define a desired state for your environment, and the "system" magically makes it so. It might involve creating resources, reconfiguring existing resources, or even delete resources. The cool thing is that you don't have to now. You can focus on what the end result should be.

For example, to set up the same environment that we set up previously, using a declarative approach with Terraform, we would write the following desired state

```
resource "azurerm_resource_group" "my_rg" {
  name     = "MyRG"
  location = "West Europe"
}

resource "azurerm_app_service_plan" "my_app_svc" {
  name                = "MyAppSvcPlan"
  location            = azurerm_resource_group.my_rg.location
  resource_group_name = azurerm_resource_group.my_rg.name

  sku {
    tier = "Free"
    size = "F1"
  }
}

resource "azurerm_app_service" "example" {
  name                = "MyWebApp"
  location            = azurerm_resource_group.my_rg.location
  resource_group_name = azurerm_resource_group.my_rg.name
  app_service_plan_id = azurerm_app_service_plan.my_app_svc.id
}
```

As you can see, there is nothing in there that defines how to set up the resources. That is up to Terraform, or rather the Azure Terraform provider, to figure out. All the code does, is defining what resources should be there at the end, and how they should be configured.

__Note:__ Sure, there is a bit more to type, and it is much more verbose than the CLI commands, but that just makes it easier to read. Not to mention that the declarative approach comes with extra benefits.

A somewhat big difference from the imperative approach is that we don't really interact with the cloud provider when we create our infrastructure. Instead, we leave that up to some other tool, like Terraform, to sort that out for us.

A lot of these tools will also compare any existing resources with the desired state, and reset them back to the desired state if they have been changed. And for any setting that you haven't provided, it will have a default value that it will use for the comparison, resetting it back to that default if it has been changed. A feature that removes the question of what happens if a manual change is made, as the answer is that it will reset it.

This desired state idea makes for excellent handling of idempotency. Basically, it is idempotent by design, allowing you to run it as often as you want, resetting the environment to exactly what you want it to look like every time.

### Combining the two

As I mentioned before, there are also options out there that combine the two. Mainly I'm thinking of a tool called Pulumi.

Pulumi merges the two paradigms by creating a desired state for the environment using code. So you end up writing code like you would in the imperative world, but the code creates a desired state that really makes it declarative.

I will talk more about Pulumi in a future post. But to put it on even footing in this context, I want to show you what the Pulumi version of the above created environment would look like.

```typescript
import * as resource from "@pulumi/azure-native/resources";
import * as web from "@pulumi/azure-native/web";

const rg = new resource.ResourceGroup("MyRG");

const appSvcPlan = new web.AppServicePlan("MyAppSvcPlan", {
    resourceGroupName: rg.name,
    kind: "App",
    sku: {
        name: "F1",
        tier: "Free",
    },
});

const app = new web.WebApp("MyWebApp", {
    resourceGroupName: rg.name,
    serverFarmId: appSvcPlan.id
});
```

As you might have noticed, the Pulumi code is written using TypeScript. This enables us to use all the power of a full programming language to create the desired state, which is really powerful!

__Note:__ TypeScript is only one of several languages supported bu Pulumi. Other options include C#, Python and Go.

### Conclusion: Imperative or Declarative

I personally believe that the simplicity of the imperative way of doing IaC makes it very approachable and easy to get started with. And I truly believe that for your first attempt at IaC, or for smaller projects, this might be the easiest way to get started.

However, for anything bigger, or more complex, I honestly think that the declarative approach wins. It is just so much more structured and maintainable. Not to mention that it was created specifically for IaC. I don't feel that the declarative way was really created for this purpose. Sure, it was created for us to be able to manage our infrastructure in the cloud. But there is a difference between that, and IaC in my opinion. The fact that it can be scripted to create a whole environment and turned into IaC just happens to be a nice ability that comes out of that.

With that in mind, my personal preference is definitely Pulumi if I get to choose. I much prefer to be able to use real code to create my state, as opposed to a domain-specific language (DSL) like Terraform or ARM for example. The main reason being that DSLs generally have some annoying limitations. For example, there is no built-in way to conditionally add a resource in Terraform. Instead, the suggestion is to use their built-in `count` functionality. The `count` however, is actually there to enable you to add multiple instances of a resource. And sure, if you use a `count` of 0 or 1, that is pretty much the same as an if-statement. But it feels like an ugly hack, and has some other implications as well.

Having that said, both (all 3) options are completely valid! And we all have our own preferences based on our previous experiences. If you are an IT Pro, you might prefer PowerShell or Azure CLI. And if you are a developer, Pulumi is likely going to feel natural for you. And Terraform is somewhere in the middle, catering to both audiences. 

## Conclusion

Infrastructure as Code is definitely something to keep in mind when building applications. It makes quite a few things a lot easier. Not to mention that it alleviates a lot of the stress that setting up multiple environments can cause. It also makes it much easier to have really identical environments.

Having that said, I would also suggest that you add it from the beginning if possible. In a new project, I definitely recommend not making it an afterthought, and try to get an existing environment defined as IaC after you have already set one up manually. It is __much__ easier to do it using IaC from the beginning. 

On the other hand, if you already have an existing environment, it is obviously hard to turn back time and get the IaC set up first. So you are kind of stuck with defining the existing environment in in code. Something that will definitely take quite a bit of time if it is a somewhat complex environment. But in most cases you can do it resource by resource over time, instead of doing a year long stint of just declaring the environment in code. And in some cases, I would even argue that it might not be worth the cost. However, those cases are somewhat rare in my opinion.

The second part, [Infrastructure as Code - An intro - Part 2 - Using Azure CLI](/iac-an-intro-part-2), is now available if you want to keep reading about IaC