using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Resources;

class MyStack : Stack
{
    public MyStack()
    {
        var resourceGroup = new ResourceGroup(Pulumi.Deployment.Instance.ProjectName, new ResourceGroupArgs {
            ResourceGroupName = Pulumi.Deployment.Instance.ProjectName
        });
        
        var stagingEnvironment = new Infrastructure.WebsiteEnvironment(Pulumi.Deployment.Instance.ProjectName + "-Staging", new Infrastructure.WebsiteEnvironmentArgs {
            ResourceGroupName = resourceGroup.Name
        });
        var prodEnvironment = new Infrastructure.WebsiteEnvironment(Pulumi.Deployment.Instance.ProjectName, new Infrastructure.WebsiteEnvironmentArgs {
            ResourceGroupName = resourceGroup.Name
        });
        
        StagingUrl = stagingEnvironment.Url;
        ProductionUrl = prodEnvironment.Url;
        AzProductionCdnPurgeCommand = prodEnvironment.AzCdnPurgeCommand;

        var functionsContainer = new Pulumi.AzureNative.Storage.BlobContainer("functions", new Pulumi.AzureNative.Storage.BlobContainerArgs {
            ContainerName = "functions",
            ResourceGroupName = resourceGroup.Name,
            AccountName = prodEnvironment.StorageAccount.Name,
        }, new CustomResourceOptions { Parent = prodEnvironment.StorageAccount });

        var functionsBlob = new Pulumi.AzureNative.Storage.Blob("functions", new Pulumi.AzureNative.Storage.BlobArgs {
            BlobName = "functions.zip",
            AccountName = prodEnvironment.StorageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            ContainerName = functionsContainer.Name,
            Source = new Pulumi.FileArchive("../Functions"),
        }, new CustomResourceOptions { Parent = functionsContainer });
        
        var functionsPlan = new Pulumi.AzureNative.Web.AppServicePlan(Pulumi.Deployment.Instance.ProjectName, new Pulumi.AzureNative.Web.AppServicePlanArgs {
            Name = Pulumi.Deployment.Instance.ProjectName,
            ResourceGroupName = resourceGroup.Name,
            Sku = new Pulumi.AzureNative.Web.Inputs.SkuDescriptionArgs {
                Name = "Y1",
                Tier = "Dynamic",
            }
        });
        
        new Pulumi.AzureNative.Web.WebApp(Pulumi.Deployment.Instance.ProjectName, new Pulumi.AzureNative.Web.WebAppArgs {
            Name = Pulumi.Deployment.Instance.ProjectName,
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = functionsPlan.Id,
            Kind = "functionapp",
            SiteConfig = new Pulumi.AzureNative.Web.Inputs.SiteConfigArgs {
                AppSettings = new List<Pulumi.AzureNative.Web.Inputs.NameValuePairArgs> {
                    new Pulumi.AzureNative.Web.Inputs.NameValuePairArgs { 
                        Name = "AzureWebJobsStorage", 
                        Value = GetStorageConnectionString(resourceGroup, prodEnvironment.StorageAccount) 
                    },
                    new Pulumi.AzureNative.Web.Inputs.NameValuePairArgs { Name = "FUNCTIONS_EXTENSION_VERSION", Value = "~3" },
                    new Pulumi.AzureNative.Web.Inputs.NameValuePairArgs { Name = "FUNCTIONS_WORKER_RUNTIME", Value = "node" },
                    new Pulumi.AzureNative.Web.Inputs.NameValuePairArgs { Name = "WEBSITE_NODE_DEFAULT_VERSION", Value = "~14" },
                    new Pulumi.AzureNative.Web.Inputs.NameValuePairArgs { 
                        Name = "WEBSITE_RUN_FROM_PACKAGE", 
                        Value = GetCodeBlobUrl(resourceGroup, prodEnvironment.StorageAccount, functionsContainer , functionsBlob) 
                    }
                },
                ConnectionStrings = new List<Pulumi.AzureNative.Web.Inputs.ConnStringInfoArgs> {
                    new Pulumi.AzureNative.Web.Inputs.ConnStringInfoArgs { 
                        Name = "STAGING", 
                        Type = Pulumi.AzureNative.Web.ConnectionStringType.Custom, 
                        ConnectionString = GetStorageConnectionString(resourceGroup, stagingEnvironment.StorageAccount) 
                    },
                    new Pulumi.AzureNative.Web.Inputs.ConnStringInfoArgs { 
                        Name = "PRODUCTION", 
                        Type = Pulumi.AzureNative.Web.ConnectionStringType.Custom, 
                        ConnectionString = GetStorageConnectionString(resourceGroup, prodEnvironment.StorageAccount)
                    }
                },
                Http20Enabled = true,
                NodeVersion = "~14",
                Cors = new Pulumi.AzureNative.Web.Inputs.CorsSettingsArgs {
                    AllowedOrigins = new[] { StagingUrl.Apply(x => x.TrimEnd('/')), ProductionUrl.Apply(x => x.TrimEnd('/')) }
                }
            },
        }, new CustomResourceOptions { Parent = functionsPlan });
    }

    private Output<string> GetStorageConnectionString(ResourceGroup resourceGroup, Pulumi.AzureNative.Storage.StorageAccount account)
    {
        var primaryStorageKey = Output.Tuple(resourceGroup.Name, account.Name).Apply(
                x => Output.Create(Pulumi.AzureNative.Storage.ListStorageAccountKeys.InvokeAsync(
                                new Pulumi.AzureNative.Storage.ListStorageAccountKeysArgs {
                                    ResourceGroupName = x.Item1,
                                    AccountName = x.Item2
                                }).ContinueWith(x => x.Result.Keys[0].Value)
                )
        );
    
        return Output.Tuple(account.Name, primaryStorageKey).Apply(x => $"DefaultEndpointsProtocol=https;AccountName={x.Item1};AccountKey={x.Item2}");
    }
    
    private Output<string> GetCodeBlobUrl(ResourceGroup resourceGroup, Pulumi.AzureNative.Storage.StorageAccount account, Pulumi.AzureNative.Storage.BlobContainer container, Pulumi.AzureNative.Storage.Blob blob)
    {
        var blobSASServiceSasToken = Output.Tuple(resourceGroup.Name, account.Name, container.Name)
                .Apply(x => Output.Create(Pulumi.AzureNative.Storage.ListStorageAccountServiceSAS.InvokeAsync(
                                new Pulumi.AzureNative.Storage.ListStorageAccountServiceSASArgs {
                                    ResourceGroupName = x.Item1,
                                    AccountName = x.Item2,
                                    Protocols = Pulumi.AzureNative.Storage.HttpProtocol.Https,
                                    SharedAccessStartTime = System.DateTime.UtcNow.ToString("yyyy-MM-dd"),
                                    SharedAccessExpiryTime = "2030-01-01",
                                    Resource = Pulumi.AzureNative.Storage.SignedResource.C,
                                    Permissions = Pulumi.AzureNative.Storage.Permissions.R,
                                    CanonicalizedResource = $"/blob/{x.Item2}/{x.Item3}"
                                }
                        ).ContinueWith(x => x.Result.ServiceSasToken)));
    
        return Output.Tuple(account.Name, container.Name, blob.Name, blobSASServiceSasToken)
                            .Apply(x => $"https://{x.Item1}.blob.core.windows.net/{x.Item2}/{x.Item3}?{x.Item4}");
    }
    

    [Output]
    public Output<string> StagingUrl { get; set; }
    [Output]
    public Output<string> ProductionUrl { get; set; }
    [Output]
    public Output<string> AzProductionCdnPurgeCommand { get; set; }    
}
