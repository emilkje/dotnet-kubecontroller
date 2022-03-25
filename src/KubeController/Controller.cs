using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeController
{
    public class Controller<TResourceDefinition> : BackgroundService
        where TResourceDefinition : CustomResourceDefinition
    {
        private readonly ILogger<Controller<TResourceDefinition>> _logger;
        private readonly EventWatcher<TResourceDefinition> _eventWatcher;
        private readonly ReconciliationLoop<TResourceDefinition> _reconciliationLoop;
        private readonly TResourceDefinition _resourceDefinition;
        private readonly CustomResourceDefnitionAvailability<TResourceDefinition> _crdAvailability;
        private readonly IOptions<KubeControllerOptions> _options;

        public Controller(
            ILogger<Controller<TResourceDefinition>> logger,
            EventWatcher<TResourceDefinition> controller,
            ReconciliationLoop<TResourceDefinition> reconciliationLoop,
            TResourceDefinition resourceDefinition,
            CustomResourceDefnitionAvailability<TResourceDefinition> defnitionAvailability,
            IOptions<KubeControllerOptions> options)
        {
            _logger = logger;
            _eventWatcher = controller;
            _reconciliationLoop = reconciliationLoop;
            _resourceDefinition = resourceDefinition;
            _crdAvailability = defnitionAvailability;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RunAsync(stoppingToken);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("=== {ResourceDefinition} controller STARTED for namespace {Namespace} ===",
                    _resourceDefinition.DefinitionDisplayName,
                    _options.Value.Namespace);

                while (!await _crdAvailability.IsAvailableAsync())
                {
                    LogWaitingForResourceDefinition();
                    await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken).ConfigureAwait(false);
                }

                await Task.WhenAll(
                    _eventWatcher.StartAsync(cancellationToken),
                    _reconciliationLoop.StartAsync(cancellationToken))
                    .ConfigureAwait(false);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogCritical(0, ex, "{ResourceDefinition} controller exited unexpectedly",
                    _resourceDefinition.DefinitionDisplayName);

                throw;
            }
            finally
            {
                _logger.LogInformation("=== {ResourceDefinition} controller TERMINATING ===",
                    _resourceDefinition.DefinitionDisplayName);
            }
        }

        private static DateTime LastLogged = DateTime.UtcNow;

        private void LogWaitingForResourceDefinition()
        {
            if(LastLogged.AddSeconds(2) < DateTime.UtcNow)
            {
                _logger.LogInformation("Controller waiting for CRD {ResourceDefinition}", 
                    _resourceDefinition.DefinitionDisplayName);

                LastLogged = DateTime.UtcNow;
            }
        }
    }
}
