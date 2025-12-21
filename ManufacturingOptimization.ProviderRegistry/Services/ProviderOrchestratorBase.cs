using Docker.DotNet;
using Docker.DotNet.Models;

namespace ManufacturingOptimization.ProviderRegistry.Services;

/// <summary>
/// Base class for provider orchestrators with shared Docker functionality.
/// </summary>
public abstract class ProviderOrchestratorBase
{
    protected readonly ILogger _logger;
    protected readonly DockerClient _dockerClient;

    protected ProviderOrchestratorBase(ILogger logger)
    {
        _logger = logger;

        var dockerUri = Environment.OSVersion.Platform == PlatformID.Unix
            ? "unix:///var/run/docker.sock"
            : "npipe://./pipe/docker_engine";

        _dockerClient = new DockerClientConfiguration(new Uri(dockerUri)).CreateClient();
    }

    public async Task CleanupOrphanedContainersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allContainers = await _dockerClient.Containers.ListContainersAsync(
                new ContainersListParameters { All = true },
                cancellationToken);

            var orphanedContainers = allContainers
                .Where(c =>
                {
                    var labels = c.Labels ?? new Dictionary<string, string>();
                    return labels.TryGetValue("orchestration-mode", out var mode) && mode == "production";
                })
                .ToList();

            foreach (var container in orphanedContainers)
            {
                await _dockerClient.Containers.RemoveContainerAsync(
                    container.ID,
                    new ContainerRemoveParameters { Force = true },
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup orphaned containers");
        }
    }
}
