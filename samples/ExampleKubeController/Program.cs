using ExampleKubeController;
using KubeController;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog((host,logging) =>
    {
        logging.WriteTo.Console();
    })
    .UseKubeController(kube =>
    {
        kube.Namespace = "default";
        kube.RegisterController<Example, ExampleOperationHandler>();
        kube.ConfigureServices((_, services)=>
        {
            services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(60));
            services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);
        });
    })
    .Build();

await host.RunAsync();
