using k8s;
using k8s.Autorest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeController
{
    public class CustomResourceDefnitionAvailability<TResourceDefinition> where TResourceDefinition : CustomResourceDefinition
    {
        private readonly ILogger<CustomResourceDefnitionAvailability<TResourceDefinition>> _logger;
        private readonly Kubernetes _k8s;
        private readonly TResourceDefinition _resourceDefinition;
        private readonly string _namespace;

        public CustomResourceDefnitionAvailability(
            ILogger<CustomResourceDefnitionAvailability<TResourceDefinition>> logger, 
            Kubernetes k8s, 
            TResourceDefinition resourceDefinition, 
            IOptions<KubeControllerOptions> options
            )
        {
            _logger = logger;
            _k8s = k8s;
            _resourceDefinition = resourceDefinition;
            _namespace = options.Value.Namespace;
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                await _k8s.ListNamespacedCustomObjectWithHttpMessagesAsync(
                    _resourceDefinition.Group, 
                    _resourceDefinition.Version, 
                    _namespace, 
                    _resourceDefinition.Plural);
            }
            catch (HttpOperationException hoex) when (hoex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("No CustomResourceDefinition found for {Plural}, group {Group} and version {Version} on namespace {Namespace}.",
                    _resourceDefinition.Plural,
                    _resourceDefinition.Group,
                    _resourceDefinition.Version,
                    _namespace);

                _logger.LogInformation("Checking again in {ReconciliationInterval} seconds...", _resourceDefinition.ReconciliationCheckInterval);

                return false;
            }

            return true;
        }
    }
}
