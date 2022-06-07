using System;
using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Storage;

namespace Infrastructure
{
    public class WebsiteEnvironmentArgs : ResourceArgs
    {
        public Pulumi.Input<string> ResourceGroupName { get; set; } = "";
        public TimeSpan? CdnCacheDuration { get; set; }
    }
    
    public class WebsiteEnvironment : Pulumi.ComponentResource
    {
        public WebsiteEnvironment(string name, WebsiteEnvironmentArgs args, ComponentResourceOptions? options = null) 
            : base("azurepagesdemo:resource:WebsiteEnvironment", name, args, options)
        {
            StorageAccount = new Pulumi.AzureNative.Storage.StorageAccount(name, new Pulumi.AzureNative.Storage.StorageAccountArgs{
                AccountName = name.ToLower().Replace("-", ""),
                ResourceGroupName = args.ResourceGroupName,
                Kind = Pulumi.AzureNative.Storage.Kind.StorageV2,
                Sku = new Pulumi.AzureNative.Storage.Inputs.SkuArgs {
                    Name = "Standard_LRS"
                },
                EnableHttpsTrafficOnly = true
            }, new CustomResourceOptions { Parent = this });

            new Pulumi.AzureNative.Storage.StorageAccountStaticWebsite(name, new StorageAccountStaticWebsiteArgs {
                AccountName = StorageAccount.Name,
                ResourceGroupName = args.ResourceGroupName,
                Error404Document = "404.html",
                IndexDocument = "index.html"
            }, new CustomResourceOptions { Parent = StorageAccount });
            
            var cdnProfile =  new Pulumi.AzureNative.Cdn.Profile(name, new Pulumi.AzureNative.Cdn.ProfileArgs {
                ProfileName = name,
                ResourceGroupName = args.ResourceGroupName,
                Sku = new Pulumi.AzureNative.Cdn.Inputs.SkuArgs {
                    Name = "Standard_Microsoft"
                }
            }, new CustomResourceOptions { Parent = this });
            
            var hostname = StorageAccount.PrimaryEndpoints.Apply(x => x.Web.Replace("https://", "").Replace("/",""));
            
            var endpoint = new Pulumi.AzureNative.Cdn.Endpoint(name, new Pulumi.AzureNative.Cdn.EndpointArgs
            {
                EndpointName = name.ToLower(),
                ResourceGroupName = args.ResourceGroupName,
                ProfileName = cdnProfile.Name,
                OriginHostHeader = hostname,
                Origins = new List<Pulumi.AzureNative.Cdn.Inputs.DeepCreatedOriginArgs>
                {
                    new Pulumi.AzureNative.Cdn.Inputs.DeepCreatedOriginArgs {
                        Name = "blobstorage",
                        HostName = hostname
                    }
                },
                DeliveryPolicy = new Pulumi.AzureNative.Cdn.Inputs.EndpointPropertiesUpdateParametersDeliveryPolicyArgs {
                    Rules = new List<Pulumi.AzureNative.Cdn.Inputs.DeliveryRuleArgs> {
                        new Pulumi.AzureNative.Cdn.Inputs.DeliveryRuleArgs {
                            Order = 0,
                            Name = "CacheExpiration",
                            Actions = new List<object> {
                                new Pulumi.AzureNative.Cdn.Inputs.DeliveryRuleCacheExpirationActionArgs
                                {
                                    Name = "CacheExpiration",
                                    Parameters = new Pulumi.AzureNative.Cdn.Inputs.CacheExpirationActionParametersArgs {
                                        CacheBehavior = args.CdnCacheDuration.HasValue ? "Override" : "BypassCache",
                                        CacheDuration = args.CdnCacheDuration.HasValue ? (Input<string>)args.CdnCacheDuration.Value.ToString() : null,
                                        CacheType = "All",
                                        OdataType = "#Microsoft.Azure.Cdn.Models.DeliveryRuleCacheExpirationActionParameters",
                                    }
                                }
                            }
                        },
                        new Pulumi.AzureNative.Cdn.Inputs.DeliveryRuleArgs {
                            Order = 1,
                            Name = "HttpToHttpsRedirect",
                            Actions = new List<object> {
                                new Pulumi.AzureNative.Cdn.Inputs.UrlRedirectActionArgs {
                                    Name = "UrlRedirect",
                                    Parameters = new Pulumi.AzureNative.Cdn.Inputs.UrlRedirectActionParametersArgs {
                                        DestinationProtocol = Pulumi.AzureNative.Cdn.DestinationProtocol.Https,
                                        RedirectType = Pulumi.AzureNative.Cdn.RedirectType.PermanentRedirect,
                                        OdataType = "#Microsoft.Azure.Cdn.Models.DeliveryRuleUrlRedirectActionParameters"
                                    }
                                }
                            },
                            Conditions = new List<object> {
                                new Pulumi.AzureNative.Cdn.Inputs.DeliveryRuleRequestSchemeConditionArgs {
                                    Name = "RequestScheme",
                                    Parameters = new Pulumi.AzureNative.Cdn.Inputs.RequestSchemeMatchConditionParametersArgs {
                                        MatchValues = new [] { "HTTP" },
                                        NegateCondition = false,
                                        Operator = "Equal",
                                        OdataType = "#Microsoft.Azure.Cdn.Models.DeliveryRuleRequestSchemeConditionParameters"
                                    }
                                }
                            }
                        }
                    }
                }
            }, new CustomResourceOptions { Parent = cdnProfile });
            
            var cnameRecord = new Pulumi.AzureNative.Network.RecordSet(name, new Pulumi.AzureNative.Network.RecordSetArgs {
                ResourceGroupName = "DNS",
                ZoneName = "zerokoll.com",
                RecordType = "CNAME",
                CnameRecord = new Pulumi.AzureNative.Network.Inputs.CnameRecordArgs {
                    Cname = name.ToLower()
                },
                Ttl = 30,
                TargetResource = new Pulumi.AzureNative.Network.Inputs.SubResourceArgs { Id = endpoint.Id }
            }, new CustomResourceOptions { Parent = endpoint });
            
            var customDomain = new Pulumi.AzureNative.Cdn.CustomDomain(name, new Pulumi.AzureNative.Cdn.CustomDomainArgs {
                CustomDomainName = name.ToLower(),
                HostName = name.ToLower() + ".zerokoll.com",
                ProfileName = cdnProfile.Name,
                EndpointName = endpoint.Name,
                ResourceGroupName = args.ResourceGroupName
            }, new CustomResourceOptions { Parent = cnameRecord });
            
            new Pulumi.Command.Local.Command(name + "-SslSetUp", new Pulumi.Command.Local.CommandArgs {
                Create = Output.Tuple(args.ResourceGroupName.ToOutput(), cdnProfile.Name, endpoint.Name, customDomain.Name)
                                        .Apply(names => $"az cdn custom-domain enable-https -g {names.Item1} --profile-name {names.Item2} --endpoint-name {names.Item3} -n {names.Item4}")
            }, new CustomResourceOptions { Parent = customDomain });

            // Required as Custom Domain cannot be removed while CNAME is in place.
            // Warning: Does not work all the time, as DNS records are cached unfortunately
            // Warning: Only works on Windows! Replace "&& powershell Start-Sleep 40" with corresponding *nix command for Linux or Mac
            new Pulumi.Command.Local.Command(name + "-CnameRemoval", new Pulumi.Command.Local.CommandArgs {
                Delete = cnameRecord.Name.Apply(name => $"az network dns record-set cname remove-record -g DNS -z zerokoll.com -n {name} -c \"-\" && powershell Start-Sleep 40")
            }, new CustomResourceOptions { Parent = cnameRecord, DependsOn = new [] { customDomain } });
            
            Url = cnameRecord.Fqdn.Apply(x => $"https://{x.Substring(0, x.Length - 1)}/");

            AzCdnPurgeCommand = Output.Tuple(args.ResourceGroupName.ToOutput(), endpoint.Name, cdnProfile.Name)
                    .Apply(names => $"az cdn endpoint purge -g {names.Item1} -n {names.Item2} --profile-name {names.Item3} --content-paths /*");

            this.RegisterOutputs();
        }

        public Output<string> Url { get; private set; }
        public Output<string> AzCdnPurgeCommand { get; private set; }
        public StorageAccount StorageAccount { get; }
    }
}
