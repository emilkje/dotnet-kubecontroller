using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeController
{
    public static class DependencyInjectionExtensions
    {
        public static IHostBuilder UseKubeController(this IHostBuilder hostBuilder, Action<KubeControllerBuilder> configureController) 
        {

            var controllerBuilder = new KubeControllerBuilder(hostBuilder);
            configureController(controllerBuilder);

            controllerBuilder.Build();

            return hostBuilder;
        }
    }

    public class KubeControllerBuilder
    {
        private readonly IHostBuilder _hostBuilder;

        public KubeControllerBuilder(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
        }

        public void ConfigureServices(Action<HostBuilderContext, IServiceCollection> configure)
        {
            _hostBuilder.ConfigureServices(configure);
        }

        public string Namespace { get; set; } = "default";

        public void RegisterController<TCustomResourceDefinition, THandler>()
            where TCustomResourceDefinition : CustomResourceDefinition
            where THandler : class, IOperationHandler<TCustomResourceDefinition>
        {
            _hostBuilder.ConfigureServices((_, s) =>
            {
                s.Configure<KubeControllerOptions>(o => o.Namespace = Namespace);
                s.AddTransient<TCustomResourceDefinition>();
                s.AddScoped<IOperationHandler<TCustomResourceDefinition>, THandler>();
                s.AddTransient<CustomResourceDefnitionAvailability<TCustomResourceDefinition>>();
                s.AddSingleton<EventWatcher<TCustomResourceDefinition>>();
                s.AddSingleton<ReconciliationLoop<TCustomResourceDefinition>>();
                s.AddHostedService<Controller<TCustomResourceDefinition>>();
            });
        }

        public void Build()
        {
            _hostBuilder.ConfigureServices((_, s) =>
            {
                s.Configure<KubeControllerOptions>(o => o.Namespace = Namespace);
                s.AddSingleton<HandlerExecutor>();

                // fall back to in-cluster or kubeconfig file depending on 
                // the where the controller is running
                var config = KubernetesClientConfiguration.IsInCluster()
                    ? KubernetesClientConfiguration.InClusterConfig()
                    : KubernetesClientConfiguration.BuildConfigFromConfigFile();

                s.AddSingleton(new Kubernetes(config));
            });
        }
    }
}
