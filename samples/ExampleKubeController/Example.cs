using KubeController;

namespace ExampleKubeController
{
    public class Example : CustomResourceDefinition
    {
        public Example() : base("v95.io", "v1alpha1", "examples", "example")
        {
			Spec = new();
		}

		public ExampleSpec Spec { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj == null)
				return false;

			return ToString().Equals(obj.ToString());
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override string ToString()
		{
			return Spec.ToString();
		}
	}

	public class ExampleSpec
	{
		public string? ExampleType { get; set; }

		public string? ProgrammingLanguage { get; set; }

		public override string ToString()
		{
			return $"{ExampleType}:{ProgrammingLanguage}";
		}
	}
}