# Azure Pages Demo

This repo contains the code used in my presentation called "Building a Better GitHub Pages Experience Using Azure Services, How Hard Can It be?".

Feel free to browse around and look at the code, and even clone and test it out. Just remember that before you publish the Pulumi-based infrastructure, you need to run `npm i` in the [Functions](./Functions/) directory. And you will also have to set up your own DNS zone in Azure, and update the DNS configuration in the [WebsiteEnvironment](./Infrastructure/WebsiteEnvironment.cs) class.

__Note:__ The password for the Pulumi dev stack is `Password1!`

## Contact

If you have any questions or comments, feel free to reach out. I am available on Twitter under the name [@ZeroKoll](https://twitter.com/zerokoll)