using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeController
{
    public class ReconciliationLoop<TResourceDefinition> where TResourceDefinition : CustomResourceDefinition
    {
        private readonly ILogger<ReconciliationLoop<TResourceDefinition>> _logger;
        private readonly TResourceDefinition _resourceDefinition;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly CustomResourceDefnitionAvailability<TResourceDefinition> _availability;

        public ReconciliationLoop(
            ILogger<ReconciliationLoop<TResourceDefinition>> logger, 
            TResourceDefinition resourceDefinition, 
            IServiceScopeFactory serviceScopeFactory,
            CustomResourceDefnitionAvailability<TResourceDefinition> availability
            )
        {
            _logger = logger;
            _resourceDefinition = resourceDefinition;
            _serviceScopeFactory = serviceScopeFactory;
            _availability = availability;
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reconciliation loop will run every {ReconciliationInterval} seconds.",
                    _resourceDefinition.ReconciliationCheckInterval);

            while(!stoppingToken.IsCancellationRequested && await _availability.IsAvailableAsync() == false)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200), stoppingToken);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_resourceDefinition.ReconciliationCheckInterval * 1000, stoppingToken);

                using var scope = _serviceScopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IOperationHandler<TResourceDefinition>>();
                await handler.CheckCurrentState(stoppingToken);
            }
        }
    }
}
