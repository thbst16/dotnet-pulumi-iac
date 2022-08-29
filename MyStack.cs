using Pulumi;
using Pulumi.AzureNative.CognitiveServices;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using System;
using System.IO;

class MyStack : Stack
{
    public MyStack()
    {
        // Create beckshome resource group
        var resourceGroup = new ResourceGroup("beckshome-pulumi-rg", new ResourceGroupArgs{
            ResourceGroupName = "beckshome-pulumi-rg"
        },
        new CustomResourceOptions { DeleteBeforeReplace = true }
        );

        // Create Linux / Docker app service plan
        var plan = new AppServicePlan("beckshome-pulumi-plan", new AppServicePlanArgs{
            ResourceGroupName = resourceGroup.Name,
            Kind = "Linux",
            Name = "beckshome-pulumi-plan",
            // Reserved must be set to true for Linux plan
            Reserved = true,
            Sku = new SkuDescriptionArgs
            {
                Name = "B2",
                Tier = "Basic"
            }
        },
        new CustomResourceOptions { DeleteBeforeReplace = true }
        );

        // Create Windows app service plan
        var windowsPlan = new AppServicePlan("beckshome-pulumi-windows-plan", new AppServicePlanArgs{
            ResourceGroupName = resourceGroup.Name,
            Kind = "App",
            Name = "beckshome-pulumi-windows-plan",
            Sku = new SkuDescriptionArgs
            {
                Name = "B1",
                Tier = "Basic"
            }
        },
        new CustomResourceOptions { DeleteBeforeReplace = true }    
        );

        // Create a storage account for docker configurations
        var storageAccount = new StorageAccount("beckhome-storage", new StorageAccountArgs
        {
            AccountName = "beckshomestorage",
            ResourceGroupName = resourceGroup.Name,
            Sku = new Pulumi.AzureNative.Storage.Inputs.SkuArgs
            {
                Name = Pulumi.AzureNative.Storage.SkuName.Standard_LRS
            },
            Kind = Kind.StorageV2
        });

        // Create storage containers
        var blazorChatContainer = new BlobContainer("beckshome-storage-blazor-chat", new BlobContainerArgs
        {
            AccountName = storageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            ContainerName = "blazor-chat",
            PublicAccess = PublicAccess.None,
        });
        var containerContainer = new BlobContainer("beckshome-storage-container", new BlobContainerArgs
        {
            AccountName = storageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            ContainerName = "container",
            PublicAccess = PublicAccess.None,
        });
        var privateContainer = new BlobContainer("beckshome-storage-private", new BlobContainerArgs
        {
            AccountName = storageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            ContainerName = "private",
            PublicAccess = PublicAccess.None,
        });
        var publicContainer = new BlobContainer("beckshome-storage-public", new BlobContainerArgs
        {
            AccountName = storageAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            ContainerName = "public",
            PublicAccess = PublicAccess.Blob,
        });

        // Upload Blobs to containers
        var blazorChatConfigBlob = new Blob("beckshome-storage-blazor-chat-appsettings", new BlobArgs
        {
            AccountName = storageAccount.Name,
            ContainerName = blazorChatContainer.Name,
            ResourceGroupName = resourceGroup.Name,
            BlobName = "appsettings.json",
            Source = new FileAsset("./blobs/blazor-chat/appsettings.json"),
        });
        
        var containerConfigBlob = new Blob("beckshome-storage-container-appsettings", new BlobArgs
        {
            AccountName = storageAccount.Name,
            ContainerName = containerContainer.Name,
            ResourceGroupName = resourceGroup.Name,
            BlobName = "appsettings.json",
            Source = new FileAsset("./blobs/container/appsettings.json"),
        });

        var containerSecretBlob = new Blob("beckshome-storage-container-secrets", new BlobArgs
        {
            AccountName = storageAccount.Name,
            ContainerName = containerContainer.Name,
            ResourceGroupName = resourceGroup.Name,
            BlobName = "client_secrets.json",
            Source = new FileAsset("./blobs/container/client_secrets.json"),
        });
        
        var privateConfigBlob = new Blob("beckshome-storage-private-appsettings", new BlobArgs
        {
            AccountName = storageAccount.Name,
            ContainerName = privateContainer.Name,
            ResourceGroupName = resourceGroup.Name,
            BlobName = "appsettings.json",
            Source = new FileAsset("./blobs/private/appsettings.json"),
        });

        var rosslynClassesBlob = new Blob("beckshome-storage-public-rosslyn-classes", new BlobArgs
        {
            AccountName = storageAccount.Name,
            ContainerName = publicContainer.Name,
            ResourceGroupName = resourceGroup.Name,
            BlobName = "rosslyn-classes.txt",
            Source = new FileAsset("./blobs/public/rosslyn-classes.txt"),
        });

        this.PrimaryStorageKey = GetStorageAccountPrimaryKey(resourceGroup.Name, storageAccount.Name);
        this.PrimaryConnectionString = Output.Format($"DefaultEndpointsProtocol=https;AccountName={storageAccount.Name};AccountKey={this.PrimaryStorageKey};EndpointSuffix=core.windows.net");
        this.RoslynClassUrl = Output.Format($"https://{storageAccount.Name}.blob.core.windows.net/public/rosslyn-classes.txt");

        // Create Cognitive / Storage Service
        var cogntiveAccount = new Pulumi.AzureNative.CognitiveServices.Account("beckshome-translation-2", new Pulumi.AzureNative.CognitiveServices.AccountArgs
        {
            AccountName = "beckshome-translation-2",
            ResourceGroupName = resourceGroup.Name,
            Kind = "TextTranslation",
            Location = "eastus",
            Sku = new Pulumi.AzureNative.CognitiveServices.Inputs.SkuArgs
            {
                Name = "S1",
            }
        },
        new CustomResourceOptions { DeleteBeforeReplace = true }
        );

        this.PrimaryCognitiveKey = GetCognitiveAccontPrimaryKey(resourceGroup.Name, cogntiveAccount.Name);
        
        // App 1: Blazor CRUD application from Docker Compose file
        Byte[] blazorCrudBytes = File.ReadAllBytes("./docker/docker-compose-blazorcrud.yml");
        string blazorcrudComposeBase64 = Convert.ToBase64String(blazorCrudBytes);

        var blazorCrudApp = new WebApp("becksblazor", new WebAppArgs
        {
            Name = "becksblazor",
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = plan.Id,
            SiteConfig = new SiteConfigArgs
            {
                AppSettings = new[]
                {
                    new NameValuePairArgs
                    {
                        Name = "WEBSITES_ENABLE_APP_SERVICE_STORAGE",
                        Value = "false"
                    }
                },
                AlwaysOn = true,
                LinuxFxVersion = $"COMPOSE|{blazorcrudComposeBase64}"
            },
            HttpsOnly = true
        },
        new CustomResourceOptions { DeleteBeforeReplace = true }
        );

        this.BlazorCrudEndpoint = Output.Format($"https://{blazorCrudApp.DefaultHostName}");

        // App 2: Roesetta Stone application from Docker Compose file
        Byte[] rosettaStoneBytes = File.ReadAllBytes("./docker/docker-compose-rosetta-stone.yml");
        string rosettaStoneComposeBase64 = Convert.ToBase64String(rosettaStoneBytes);

        var rosettaStoneApp = new WebApp("dotnet-signalr-rosetta-stone", new WebAppArgs
        {
            Name = "dotnet-signalr-rosetta-stone",
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = plan.Id,
            SiteConfig = new SiteConfigArgs
            {
                AppSettings = new[]
                {
                    new NameValuePairArgs
                    {
                        Name = "WEBSITES_ENABLE_APP_SERVICE_STORAGE",
                        Value = "false"
                    },
                    new NameValuePairArgs
                    {
                        Name = "HubConfiguration__Url",
                        Value = "https://dotnet-signalr-rosetta-stone.azurewebsites.net"
                    },
                    new NameValuePairArgs
                    {
                        Name = "AzureSpeech__SubscriptionKey",
                        Value = PrimaryCognitiveKey.Apply(x => x.ToString())
                    },
                    new NameValuePairArgs
                    {
                        Name = "AzureSpeech__Location",
                        Value = "eastus"
                    },
                },
                AlwaysOn = true,
                LinuxFxVersion = $"COMPOSE|{rosettaStoneComposeBase64}"
            },
            HttpsOnly = true
        },
        new CustomResourceOptions { DeleteBeforeReplace = true }
        );

        var rosettaStoneStorage = new WebAppAzureStorageAccounts("dotnet-signalr-rosetta-stone-storage",
            new WebAppAzureStorageAccountsArgs
            {
                 Name = rosettaStoneApp.Name,
                 ResourceGroupName = resourceGroup.Name,
                 Properties = new InputMap<AzureStorageInfoValueArgs>
                 {
                    {
                        "azure-mount",
                        new AzureStorageInfoValueArgs
                        {
                            AccessKey = PrimaryStorageKey.Apply(x => x.ToString()),
                            AccountName = "beckshomestorage",
                            MountPath = "/app/config",
                            ShareName = "blazor-chat",
                            Type = AzureStorageType.AzureBlob
                        }
                    }
                }
            }
        );

        this.RosettaStoneEndpoint = Output.Format($"https://{rosettaStoneApp.DefaultHostName}");

        // App 3: Roslyn Dynamic API application from Docker Compose file

        // Need to dynamically modify app config to tell app where to look to find the public rosslyn classses file it needs since this was created dynamically
        // Use dynamically defined app settings to specify / override the connection string in the static appsettings file??

        Byte[] roslynApiBytes = File.ReadAllBytes("./docker/docker-compose-roslyn-api.yml");
        string roslynApiComposeBase64 = Convert.ToBase64String(roslynApiBytes);

        var roslynApiApp = new WebApp("dotnet-roslyn-dynamic-api", new WebAppArgs
        {
            Name = "dotnet-roslyn-dynamic-api",
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = plan.Id,
            SiteConfig = new SiteConfigArgs
            {
                AppSettings = new[]
                {
                    new NameValuePairArgs
                    {
                        Name = "WEBSITES_ENABLE_APP_SERVICE_STORAGE",
                        Value = "false"
                    },
                    new NameValuePairArgs
                    {
                        Name = "AzureBlob__ConnectionString",
                        Value = PrimaryConnectionString.Apply(x => x.ToString())
                    },
                    new NameValuePairArgs
                    {
                        Name = "AzureBlob__Url",
                        Value = RoslynClassUrl.Apply(x => x.ToString())
                    }
                },
                AlwaysOn = true,
                LinuxFxVersion = $"COMPOSE|{roslynApiComposeBase64}"
            },
            HttpsOnly = true
        },
        new CustomResourceOptions { DeleteBeforeReplace = true }
        );

        var roslynApiStorage = new WebAppAzureStorageAccounts("dotnet-roslyn-dynamic-api-storage",
            new WebAppAzureStorageAccountsArgs
            {
                 Name = roslynApiApp.Name,
                 ResourceGroupName = resourceGroup.Name,
                 Properties = new InputMap<AzureStorageInfoValueArgs>
                 {
                    {
                        "azure-mount",
                        new AzureStorageInfoValueArgs
                        {
                            AccessKey = PrimaryStorageKey.Apply(x => x.ToString()),
                            AccountName = "beckshomestorage",
                            MountPath = "/app/config",
                            ShareName = "private",
                            Type = AzureStorageType.AzureBlob
                        }
                    }
                }
            }
        );

        this.RoslynApiEndpoint = Output.Format($"https://{roslynApiApp.DefaultHostName}");

        // App 4: Sheets Notification application from Docker Compose file
        Byte[] sheetsNotificationBytes = File.ReadAllBytes("./docker/docker-compose-sheets-notification.yml");
        string sheetsNotificationComposeBase64 = Convert.ToBase64String(sheetsNotificationBytes);

        var sheetsNotificationApp = new WebApp("dotnet-sheets-notification", new WebAppArgs
        {
            Name = "dotnet-sheets-notification",
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = plan.Id,
            SiteConfig = new SiteConfigArgs
            {
                AppSettings = new[]
                {
                    new NameValuePairArgs
                    {
                        Name = "WEBSITES_ENABLE_APP_SERVICE_STORAGE",
                        Value = "false"
                    }
                },
                AlwaysOn = true,
                LinuxFxVersion = $"COMPOSE|{sheetsNotificationComposeBase64}"
            },
            HttpsOnly = true
        },
        new CustomResourceOptions { DeleteBeforeReplace = true }
        );

        var sheetsNotificationStorage = new WebAppAzureStorageAccounts("dotnet-sheets-notification-storage",
            new WebAppAzureStorageAccountsArgs
            {
                 Name = sheetsNotificationApp.Name,
                 ResourceGroupName = resourceGroup.Name,
                 Properties = new InputMap<AzureStorageInfoValueArgs>
                 {
                    {
                        "azure-mount",
                        new AzureStorageInfoValueArgs
                        {
                            AccessKey = PrimaryStorageKey.Apply(x => x.ToString()),
                            AccountName = "beckshomestorage",
                            MountPath = "/app/config",
                            ShareName = "container",
                            Type = AzureStorageType.AzureBlob
                        }
                    }
                }
            }
        );

        this.SheetsNotificationEndpoint = Output.Format($"https://{sheetsNotificationApp.DefaultHostName}");

        // App 5: Windows Web App for Statiq Blog
        var beckshomeBlogApp = new WebApp("dotnet-statiq-beckshome-blog", new WebAppArgs
        {
            Name = "dotnet-statiq-beckshome-blog",
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = windowsPlan.Id,
        },
        new CustomResourceOptions { DeleteBeforeReplace = true }
        );

        this.BeckshomeBlogEndpoint = Output.Format($"https://{beckshomeBlogApp.DefaultHostName}");
    }
    [Output] public Output<string> BlazorCrudEndpoint { get; set; }
    [Output] public Output<string> RosettaStoneEndpoint { get; set; }
    [Output] public Output<string> RoslynApiEndpoint { get; set; }
    [Output] public Output<string> RoslynClassUrl { get; set; }
    [Output] public Output<string> SheetsNotificationEndpoint { get; set; }
    [Output] public Output<string> BeckshomeBlogEndpoint { get; set; }
    [Output] public Output<string> PrimaryStorageKey { get; set; }
    [Output] public Output<string> PrimaryConnectionString {get; set;}
    [Output] public Output<string> PrimaryCognitiveKey {get; set;}

    private static Output<string> GetStorageAccountPrimaryKey(Input<string> resourceGroupName, Input<string> accountName)
    {
        return ListStorageAccountKeys.Invoke(new ListStorageAccountKeysInvokeArgs
        {
            ResourceGroupName = resourceGroupName,
            AccountName = accountName
        }).Apply(accountKeys => accountKeys.Keys[0].Value);
    }

    private static Output<string> GetCognitiveAccontPrimaryKey(Input<string> resourceGroupName, Input<string> accountName)
    {
        return ListAccountKeys.Invoke(new ListAccountKeysInvokeArgs
        {
            ResourceGroupName = resourceGroupName,
            AccountName = accountName
        }).Apply(accountKeys => accountKeys.Key1);
    } 
}