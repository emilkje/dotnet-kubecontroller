using k8s;

namespace KubeController
{
	public interface IOperationHandler<T> where T : CustomResourceDefinition
	{
		/// <summary>
		/// Called when a new resource is created in the cluster
		/// </summary>
		/// <param name="crd"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task OnAdded(T crd, CancellationToken cancellationToken);

		/// <summary>
		/// Called when a resource is deleted from the cluster
		/// </summary>
		/// <param name="crd"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task OnDeleted(T crd, CancellationToken cancellationToken);

		/// <summary>
		/// Called when a resource is updated in the cluster
		/// </summary>
		/// <param name="crd"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task OnUpdated(T crd, CancellationToken cancellationToken);

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
		Task OnBookmarked(T crd, CancellationToken cancellationToken);

		/// <summary>
		/// Called when some error is detected
		/// </summary>
		/// <param name="crd"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task OnError(T crd, CancellationToken cancellationToken);

		/// <summary>
		/// Called on interval to give the controller an opportunity to reconcile actual state and desired state
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task CheckCurrentState(CancellationToken cancellationToken);
	}
}