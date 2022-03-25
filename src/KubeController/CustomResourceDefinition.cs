using k8s;
using k8s.Models;

namespace KubeController
{
	public abstract class CustomResourceDefinition : IMetadata<V1ObjectMeta>
	{
		protected CustomResourceDefinition(string group, string version, string plural, string singular, int reconInterval = 5)
		{
			if(string.IsNullOrWhiteSpace(group)) 
				throw new ArgumentNullException(nameof(group));
			if(string.IsNullOrWhiteSpace(version))	
				throw new ArgumentNullException(nameof(version));
			if(string.IsNullOrWhiteSpace(plural))	
				throw new ArgumentNullException(nameof(plural));
			if(string.IsNullOrWhiteSpace(singular))
				throw new ArgumentNullException(nameof(singular));

			Group = group;
			Version = version;
			Plural = plural;
			Singular = singular;
			ReconciliationCheckInterval = reconInterval;
		}

		public int ReconciliationCheckInterval { get; protected set; }
		public string Group { get; protected set; }
		public string Version { get; protected set; }
		public string Plural { get; protected set; }
		public string Singular { get; protected set; }
		public string StatusAnnotationName { get => string.Format($"{Group}/{Singular}-status"); }

		public string? Status => Metadata.Annotations.ContainsKey(StatusAnnotationName) ? Metadata.Annotations[StatusAnnotationName] : null;
		public string ApiVersion { get; set; }
		public string Kind { get; set; }
		public V1ObjectMeta Metadata { get; set; }
		public string DefinitionDisplayName => $"{Plural}.{Group}/{Version}";
    }
}