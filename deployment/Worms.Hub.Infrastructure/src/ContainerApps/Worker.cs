using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.Resources;

namespace Worms.Hub.Infrastructure.ContainerApps;

public static class Worker
{
    public static Job Config(
        ResourceGroup resourceGroup,
        Config config,
        ManagedEnvironment managedEnvironment,
        ManagedEnvironmentsStorage managedEnvironmentStorage,
        Output<string> databaseConnectionString,
        Output<string> queueConnectionString)
    {
        var image = config.Require("gateway-image");
        var storageAccountName = Utils.GetResourceNameAlphaNumericOnly("wormstest");
        var queueName = "replays-to-update";

        var containerApp = new Job(
            "worms-hub-worker",
            new()
            {
                JobName = "worms-worker",
                ResourceGroupName = resourceGroup.Name,
                EnvironmentId = managedEnvironment.Id,
                Configuration =
                    new JobConfigurationArgs
                    {
                        EventTriggerConfig = new JobConfigurationEventTriggerConfigArgs
                        {
                            Parallelism = 1,
                            ReplicaCompletionCount = 1,
                            Scale = new JobScaleArgs
                            {
                                MaxExecutions = 10,
                                MinExecutions = 0,
                                PollingInterval = 60,
                                Rules =
                                    new[]
                                    {
                                        new JobScaleRuleArgs
                                        {
                                            Auth = new[]
                                            {
                                                new ScaleRuleAuthArgs
                                                {
                                                    SecretRef = "queue-connection",
                                                    TriggerParameter = "connection",
                                                },
                                            },
                                            Metadata =
                                                new Dictionary<string, string>
                                                {
                                                    { "accountName", storageAccountName },
                                                    { "queueName", queueName },
                                                    { "queueLength", "1" }
                                                },
                                            Name = "queue",
                                            Type = "azure-queue",
                                        },
                                    },
                            },
                        },
                        ReplicaRetryLimit = 10,
                        ReplicaTimeout = 3600,
                        TriggerType = TriggerType.Event,
                        Secrets =
                        [
                            new SecretArgs
                            {
                                Name = "database-connection",
                                Value = databaseConnectionString,
                            },
                            new SecretArgs
                            {
                                Name = "queue-connection",
                                Value = queueConnectionString,
                            }
                        ]
                    },
                Template = new JobTemplateArgs
                {
                    Containers =
                    {
                        new ContainerArgs
                        {
                            Name = "wa-runner",
                            Image = image,
                            Env =
                            [
                                new EnvironmentVarArgs
                                {
                                    Name = "WORMS_STORAGE__TempReplayFolder",
                                    Value = "/storage/temp-replays",
                                },
                                new EnvironmentVarArgs
                                {
                                    Name = "WORMS_BATCH",
                                    Value = "true",
                                },
                                new EnvironmentVarArgs
                                {
                                    Name = "WORMS_CONNECTIONSTRINGS__DATABASE",
                                    SecretRef = "database-connection",
                                },
                                new EnvironmentVarArgs
                                {
                                    Name = "WORMS_CONNECTIONSTRINGS__STORAGE",
                                    SecretRef = "queue-connection",
                                },
                                new EnvironmentVarArgs
                                {
                                    Name = "WORMS_HUB_DISTRIBUTED",
                                    Value = "true",
                                },
                                new EnvironmentVarArgs
                                {
                                    Name = "WORMS_HUB_WORKER",
                                    Value = "true",
                                },
                            ],
                            VolumeMounts =
                            {
                                new VolumeMountArgs
                                {
                                    VolumeName = "worms-hub-volume",
                                    MountPath = "/storage",
                                }
                            }
                        }
                    },
                    Volumes =
                    {
                        new VolumeArgs
                        {
                            Name = "worms-hub-volume",
                            StorageType = StorageType.AzureFile,
                            StorageName = managedEnvironmentStorage.Name,
                        }
                    }
                }
            });

        return containerApp;
    }
}
