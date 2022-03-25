using k8s;
using KubeController;

namespace ExampleKubeController
{
    public class ExampleOperationHandler : IOperationHandler<Example>
    {
        private readonly ILogger<ExampleOperationHandler> _logger;
        private readonly Kubernetes _kubernetes;

        public ExampleOperationHandler(ILogger<ExampleOperationHandler> logger, Kubernetes kubernetes)
        {
            _logger = logger;
            _kubernetes = kubernetes;
        }

        /// <summary>
        /// Use this oportunity to read desired state and reconsile the actual state
        /// </summary>
        /// <param name="k8s"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task CheckCurrentState(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
		/// Called when a new resource is created in the cluster
		/// </summary>
		/// <param name="crd"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
        public Task OnAdded(Example crd, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OnAdded, {@CRD}", crd);
            return Task.CompletedTask;
        }

        /// <summary>
		/// To mitigate the impact of short history window, the Kubernetes API provides a watch event named BOOKMARK. 
		/// It is a special kind of event to mark that all changes up to a given resourceVersion the 
		/// client is requesting have already been sent. The document representing the BOOKMARK event is 
		/// of the type requested by the request, but only includes a .metadata.resourceVersion field.
		/// </summary>
		/// <see cref="https://kubernetes.io/docs/reference/using-api/api-concepts/#watch-bookmarks"/>
		/// <param name="crd"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
        public Task OnBookmarked(Example crd, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OnBookmarked, {@CRD}", crd);
            return Task.CompletedTask;
        }

        /// <summary>
		/// Called when a resource is deleted from the cluster
		/// </summary>
		/// <param name="crd"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
        public Task OnDeleted(Example crd, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OnDeleted, {@CRD}", crd);
            return Task.CompletedTask;
        }

        /// <summary>
		/// Called when some error is detected
		/// </summary>
		/// <param name="crd"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
        public Task OnError(Example crd, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OnError, {@CRD}", crd);
            return Task.CompletedTask;
        }

        /// <summary>
		/// Called when a resource is updated in the cluster
		/// </summary>
		/// <param name="crd"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
        public Task OnUpdated(Example crd, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OnUpdated, {@CRD}", crd);
            return Task.CompletedTask;
        }
    }
}