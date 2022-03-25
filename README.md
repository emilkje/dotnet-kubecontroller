# Kubernetes Controllers for C#


<p align="center">
<img src="./assets/kubernetes-logo.png" /> <br />
</p>

This is a sample C# library for creating lightweight [controllers][controller] for [Kubernetes CRDs][crd], using the [Kubernetes C# client][csharp-client]. It is intended to show you how to get started writing your own controller for CRDs in C#, and it is _not_ suited for production purposes.


## Using this library

Considering a custom resource definition that can be found in [example-crd.yaml][sample-crd], building the C# object is done by inheriting from the `CustomResourceDefinition` base class which mimics the structure of your CRD. In addition you have to register a handler for the events associated with the custom resource

```csharp
public class Example : CustomResourceDefinition
{
    public Example() : base("v95.io", "v1alpha1", "examples", "example")
    {
        Spec = new();
    }

    public ExampleSpec Spec { get; set; }
}
```

```csharp
public class ExampleOperationHandler : IOperationHandler<Example>
{
    // handle drift
    public Task CheckCurrentState(CancellationToken cancellationToken) { ... }

    // handle new resource added to the cluster
    public Task OnAdded(Example crd, CancellationToken cancellationToken) { ... }

    // handle updated resource
    public Task OnUpdated(Example crd, CancellationToken cancellationToken) { ...}

    // handle resource deleted from cluster
    public Task OnDeleted(Example crd, CancellationToken cancellationToken) { ... }
}
```

Use the provided IHostBuilder extension to register your custom resource definition and handler with the runtime:

```csharp
IHost host = Host.CreateDefaultBuilder(args)
    .UseKubeController(kube =>
    {
        kube.Namespace = "default";
        kube.RegisterController<Example, ExampleOperationHandler>();
    })
    .Build();

await host.RunAsync();
```

See the provided [ExampleKubeController sample][sample-dir] for additional setup.

## Getting Started

Prerequisites:

- a Kubernetes cluster
- .NET 6.0
- Visual Studio or VSCode

To run the sample, you first need to deploy the CRD (the sample uses the `default` namespace), then start the console application:

```sh
kubectl apply -f samples/ExampleKubeController/deploy/example-crd.yaml
dotnet run --project samples/ExampleKubeController
```

At this point, you can start operating on `Example` objects in your namespace, and the handler will get executed.

```sh
# try creating a new custom resource to see the handler in action
kubectl apply -f samples/ExampleKubeController/deploy/my-example.yaml 
```

[controller]: https://kubernetes.io/docs/concepts/workloads/controllers/
[crd]: https://kubernetes.io/docs/concepts/extend-kubernetes/api-extension/custom-resources/
[csharp-client]: https://github.com/kubernetes-client/csharp
[sample-crd]: ./samples/ExampleKubeController/deploy/example-crd.yaml
[sample-dir]: ./samples/ExampleKubeController/
