---
layout: post
current: post
cover:  /assets/images/covers/iac-conclusion.jpg
smallcover:  /assets/images/covers/iac-conclusion-small.jpg
navigation: True
title: Infrastructure as Code - An intro - Part 7 - "Conclusion"
date: 2021-12-09 12:34:00
tags: [infrastructure, infrastructure as code, pulumi, terraform, bicep, arm]
class: post-template
subclass: 'post'
author: zerokoll
---
I've finally come to the "conclusion" part of my blog series about infrastructure as code. The part I thought was going to be the easiest one to write...

I was one hundred percent sure that I knew what I was going to write in this final post. I was going to say that ARM is too verbose and complex, Bicep is nice, but a bit too limited, Terraform is awkward to work with, and Pulumi is the "beez neez" that we should all use, as it is awesome in every way. However...I'm not sure it is that simple unfortunately...

__Note:__ I'm not saying that Pulumi isn't the "beez neez", I'm just saying that there is more to it...

As I have gone through and written the previous posts, about [Azure CLI](/iac-an-intro-part-2), [ARM Templates](/iac-an-intro-part-3), [Bicep](/iac-an-intro-part-4), [Terraform](/iac-an-intro-part-5) and [Pulumi](/iac-an-intro-part-6), I have realized that they all have their strengths and weaknesses. There is, as usual no "silver bullet". There is no tool to rule them all. There are just different options, that fit different people and different scenarios.

As a result of this epiphany, I have decided to look at some of the different factors that you might want to consider when choosing your IaC tool, and how the different options stack up in each one of these areas.

## Getting started and usability

The first thing to look at in my opinion is how hard it is to get started with the tool, and how user friendly it is.

I know that this is not defining which tool is the best in any way. A lot of really powerful tools are very hard to get started with. But due to their power, they might still be worth learning. But I still find this category quite important. Finding a tool that is limited in functionality, but easy to get started with, can definitely be the best way to get started with a new technology. And once you reach the limits of that tool, you know a bit more about the technology in question, and how you want to work with it, which helps you when you need to upgrade to a more advanced tool. A tool that might have been too complicated to start with, when you didn't have that knowledge...

Anyhow... In this category, it is really hard to fault the Azure CLI together with PowerShell or Bash scripts.

You probably already have the CLI installed. And you probably already know a bit about how to write a PowerShell or Bash script. Because of this, getting started with IaC using the Azure CLI, probably offers the least resistance. Just open up Notepad, and start writing. Or start by running the commands manually, and then copy them step by step into a script file when they seem to work. Nice and simple!

ARM on the other hand, is probably not the best starting point. Sure, the templates can be written in any text, or JSON editor. And it can be deployed using the Azure CLI which, as I said before, is probably already on your machine. However, the syntax is just so confusing and verbose. Sure, you can export some existing templates from the portal, but...yeah...no... I would not recommend it to a beginner.

Bicep removes a lot of the complexity of ARM, which makes it easier to get started with. It also has an excellent VS Code extension, that really helps you along. And just as with ARM, the CLI is already there on your machine, ready to deploy the Bicep template for you. And If you are a Microsoft person, deploying to Azure (which you are if Bicep is an option), you have probably already embraced "the Microsoft toolchain". This makes Bicep a nice and "safe" option for Microsoft people, that is fairly easy to get started with.

Terraform on the other hand, is a third-party tool that you need to install. And even if that is quite easy to do, using Chocolatey, it is an extra step. On top of that, you have to read a little to understand how to set it up, how it works, and how it handles state etc, to get started. Having that said, it isn't that hard to get started with. And there are great "getting started" tutorials on their website.

The last option, Pulumi, is very similar to Terraform, in that is third party, requires some reading to understand, and so on. But it too, has great "getting started" information on their website. However, as it uses a programming language that you probably already know, it might feel like it is easier to understand, as there is no new syntax to learn. Instead, you can focus on learning the IaC part of things.

To sum up. Leave ARM alone! It is just too complicated. However, if you like your Microsoft tools, and only use Azure, both the Azure CLI and Bicep might be easy ways to get started. Having that said, Azure CLI isn't really a real IaC solution. Because of this, I would probably suggest only using that in the very early stages, if it feels more comfortable for you. Otherwise, Bicep is a better option.

Pulumi and Terraform on the other hand, are slightly more complicated to get started with, but they are also more powerful tools to have in your toolbelt in my opinion. On top of that, Pulumi might also feel more natural to work with, if you are a programmer, and not an "ops" person.

## Vendor support

Another important part when selecting an IaC solution, is obviously the "vendor support". And what to I mean by that? Well, I mean, where can you use the tool. What clouds, or other "systems", can you set up using the tool. Can we set up cross cloud solutions, "non-cloud" infrastructure etc.

In this category, all the Microsoft specific tools (Azure CLI, ARM and Bicep), are obviously going to suffer. They are all focused on Azure, even if I think Bicep might expand a little bit in the future, and might start supporting some other Microsoft technologies as well.

With those tools "out of the running" in this category, we are left with Terraform and Pulumi. 

Both of these tools have an provider model that allows vendors to add support for their systems. This means that more and more systems will be supported over time. And since Terraform has been around much longer than Pulumi, their eco system is also much larger. 

At the time of writing, Terraform has 1634 providers, and Pulumi 78. But do keep in mind, that the number of providers doesn't really say that much. It all depends on what providers you need.

## Extensibility

Once again, Microsoft's offerings are a bit limited. Having that said, when it comes to Azure, they already support pretty much everything you need. And on top of that, both ARM and Bicep support the ability to add arbitrary scripts that run during deployment. This should cover most scenarios...as long as the scenario is based on Azure.

Terraform also supports the idea of running scripts during deployments, using ["provisioners"](https://www.terraform.io/docs/language/resources/provisioners/syntax.html). These scripts can be set up to run during both deployment and removal of resources, which can be really powerful. In some cases, it can also make up for the lack of "zero-day" support, that Bicep for example offers.

With Pulumi, you would expect that you could just write whatever you wanted. It does use a programming language after all. However, that isn't really the case... The code we write, is only used to generate a desired state. The actual infrastructure changes are performed by the Pulumi provider. Because of this, we can't really run scripts during deployment in the same way as we would in ARM/Bicep and Terraform. 

However, Pulumi supports the creation of our own "custom" resources, which in turn can do pretty much whatever you want during deployment. They also allow us to plug into Pulumi's state management, which none of the other tools really do. So, even if it is a bit more complicated to add custom functionality to Pulumi, in the end, I think it comes out looking a little less like...well...an afterthought...

## Syntax

When it comes to syntax, I personally find that there is pretty much one massive looser, and that is ARM. Bicep and Terraform have _very_ similar syntax in my opinion, leaving them tied in this category. And Pulumi, with its proper programming language, might feel a lot more comfortable, and powerful, than a DSL, if you are a developer.

I think this comes down to personal preference, and experience. If you are an "ops" person, a DSL might feel more natural for different reasons. Whereas a developer, Pulumi might feel a lot more natural. In the end, they will all get the job done, but Pulumi definitely offers more complex set ups due to the use of proper programming languages.

The fact that Terraform doesn't provide an `if` statement, to conditionally create a resource, and instead force us to use a `count` of 1 or zero, is a clear indication of some of its limitations in my mind. Sure, the outcome is the same, but semantically, it is not a conditional creation. Not to mention that the Terraform resource is turned into a list with 0 or 1 entries, forcing us to use an indexer when referencing it.

## Feature support

When it comes to feature support, I will stick to Azure support, to make it a level playing field. For any other vendor, it would just end up being a discussion of Pulumi vs Terraform. Which in turn, would end up being a discussion of which provider you need...

For Azure, all the Microsoft tools obviously support new features as soon as they are released in most cases. That is the upside of being maintained by the same organization that builds the thing you are trying to manage.

For Terraform, the situation is definitely not the same... The Azure provider for Terraform is built on top of the Go SDK for Azure. Unfortunately, it can take a little while for that SDK to get the latest API features added. On top of that, once the Go SDK has added support for a new feature, someone in the Terraform community needs to go in and add support for it in the Terraform provider. Because of this, the Terraform Azure provider will always lag behind a bit. How much depends on how popular the new feature is.

Pulumi used to have the same issue as Terraform. Actually, it was even worse... As far as I understand, the original Azure provider for Pulumi, was actually built on top of Terraform. So, it had to wait, not only for the Go SDK, but also the Terraform provider to be updated, before new features could be added to Pulumi.

However, Pulumi now has a "new" SDK, called Azure Native, which is auto generated from the Azure OpenAPI spec. This allows Pulumi Corp to create a new SDK version whenever a new feature is added, which should reduce this delay a whole lot.

It might be worth mentioning that both Terraform and Pulumi will choose the Azure API version for you automatically. This might not be a problem, but sometimes there are breaking changes in the Azure API, which is why it is versioned. This means that updating the Terraform/Pulumi SDK/provider version, might actually introduce bugs, or at least unexpected changes to the infrastructure. This should be fairly rare though...but still...

## State management

Another thing to take into account when it comes to selecting an IaC technology, is the state management. It might not be the most important thing, but it is definitely still worth looking at.

For Azure CLI, ARM and Bicep, there is no separate state management at all. Instead, the Azure Resource Manager just look at the resources currently available. This has some nice benefits. Mostly that there is no state that can get corrupted. And also, you don't need to import existing resources into the solution if you aren't starting from scratch. On the other hand, it also seems to make ARM-based deployments a little slower than Terraform- and Pulumi-based ones.

However, there is also a major downside to not having state. Without state, these solutions have no record of what resources they have created. Because of this, they are also not able to remove the resources they created for you. So, there is no "destroy" or "remove" command that can be used to reset the environment. Instead, you have to rely on grouping the resources in resource groups, and use these to remove the created infrastructure.

When it comes to Terraform and Pulumi, state is stored separate from the infrastructure. Either in JSON-files, or in their cloud services. When storing the files yourself, you can choose to store them pretty much wherever you want, for example locally on your machine, in Azure blob storage or maybe in an S3 bucket. And access to these files is then used to control who is allowed to work with the infrastructure.

__Note:__ If you instead decide to use their cloud services, which involves a cost, they will give you a bunch of nice features like for example fine grained access control etc.

However, the way that they handle state differs a bit between the technologies. Terraform will "refresh" its state, using the real infrastructure when run. This allows it to revert pretty much any changes that have been made to the resources outside of the IaC code. This means that, as long as you run Terraform somewhat frequently, there will be very little configuration drift.

For example, imagine that you use Terraform to create a Web App, without setting any tags on the resource. You then go in and add a tag to the app in the Azure portal. If you now re-run Terraform, it will refresh the state and notice the difference, and remove the tag.

Pulumi on the other hand will only look at its own state. This means that it only knows about the information available in this location. This means that any property defined by the Pulumi code will be kept in check. However, if you change other properties in the infrastructure, outside of the IaC code, it is likely that it is not going to be reverted when you run Pulumi.

Let's look at the above example with the web app and tags. With Pulumi, this will work a little different. Because Pulumi knows nothing about the tags, it won't actually notice the added tag. And because of this, it won't remove it... If you on the other hand had set a tag in the Pulumi code, and added one manually, I believe it would actually notice the difference, and revert the tags back to the configured ones. This can cause a bit of confusion to be honest.

The risk of configuration drift, caused by manual changes, might not be a huge problem if you have a locked down environment, that doesn't allow manual changes to be made. However, if you haven't, Pulumi's slightly less strict state check might allow for a bit more configuration drift than Terraform.

## Docs, support and community

Being able to find good information about things you are trying to do, is extremely important when working with something as complex as IaC. This means that good documentation is extremely important. However, you should definitely not underestimate how useful a good community can be when you run into problems. Or when you need to implement some fringe thing that isn't properly covered by the docs. 

Luckily, I think all these tools have good documentation. And you are able to find information about most things by simply googling the chosen technology, and the resource type you are working with. This makes it quite easy to work with all of them. Especially, since all of them tend to provide example code for working with the different resource types, which makes it really easy to get going.

However, when it comes to community, it varies a bit more. Obviously, there will be a lot of information available online for the Microsoft options. However, the I don't find that you get that "open source community vibe" from them. Sure, there are repos where you can ask questions, and lots of support available on StackOverflow, but there doesn't really seem to be these diehard fans, that really love the tools, and stay really active, helping people.

Terraform and Pulumi on the other hand, both seem to have this community vibe going. There is lots of stuff going on, on their GitHub repos. There are lots of active people on StackOverflow. Lots of blogging about how to do things. And good interaction with the people maintaining the tools. It just seems a bit more active than it is with the Microsoft tools. But that might just be me...

It might be worth mentioning that Pulumi is much younger than Terraform though. Because of this, it doesn't have nearly as many providers, or people using it. Because of this, the Terraform community is obviously bigger. And there more information available out there in the form of blog posts and questions of forums for Terraform. On the other hand, I think Pulumi Corp is doing quite a good job providing examples and information to make up for that at the moment. 

Over time, this community gap will hopefully get smaller and smaller, as Pulumi picks up momentum. But it takes time to build a big community. And it's not like there aren't people answering Pulumi questions on StackOverflow. It's just that Terraform has been around for longer... 

## Verdict

So...what's the verdict? Well, to be honest, it's really hard to say. There is no silver bullet. As always, it depends on a lot of different factors...

First of all, are you targeting Azure? If you are, I definitely believe Bicep is a good option. It has zero-day support for new features. It works with the tools we already use. And the syntax has become quite nice compared to the old ARM syntax. 

However, I dislike the fact that I can't tear down my infrastructure in a simple way. So, for things like feature environments for example, it can be a bit of a letdown.

On the other hand, if you want to do more than just set up Azure resources, you have to look at Terraform or Pulumi. And once again, there are pros and cons with both of them. As a developer, I really love the fact that Pulumi uses real programming languages. This allows for more complex set ups, and more advanced logic to be put inside the IaC code. On the other hand, Terraform might feel a lot more comfortable for ops people who aren't used to programming.

When choosing between Bicep, Terraform and Pulumi, I don't think you can go wrong. They are all good tools!

Yeah...that wasn't much help, I guess. But it's really hard to be honest... They all have strengths and weaknesses. I would personally suggest taking a day and trying them all out. Get a standard infrastructure that you want to set up, and try doing it with all the tools, and see which one fits you the best!

If you have any questions or thoughts, feel free to reach out on Twitter! I'm available at [@ZeroKoll](https://twitter.com/zerokoll) as usual!