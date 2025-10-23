using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.Resources;

namespace Worms.Hub.Infrastructure.ContainerApps;

public static class WaRunner
{
    public static Job Config(
        ResourceGroup resourceGroup,
        Config config,
        ManagedEnvironment managedEnvironment,
        ManagedEnvironmentsStorage managedEnvironmentStorage,
        Output<string> queueConnectionString)
    {
        var image = config.Require("replay-processor-image");
        var storageAccountName = Utils.GetResourceNameAlphaNumericOnly("wormstest");
        var queueName = "replays-to-process";

        var containerApp = new Job(
            "worms-hub-wa-runner",
            new()
            {
                JobName = "worms-wa-runner",
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
                                    Name = "WORMS_CONNECTIONSTRINGS__STORAGE",
                                    SecretRef = "queue-connection",
                                }
                            ],
                            VolumeMounts =
                            {
                                new VolumeMountArgs
                                {
                                    VolumeName = "worms-hub-volume",
                                    MountPath = "/storage",
                                },
                                new VolumeMountArgs
                                {
                                    VolumeName = "worms-hub-volume",
                                    SubPath = "game/WA/",
                                    MountPath = "/game",
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
