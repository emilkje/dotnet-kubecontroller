using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Autorest;
using k8s.Models;
using KubeController.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KubeController
{
    public class EventWatcher<TResourceDefinition> where TResourceDefinition : CustomResourceDefinition
    {
        private readonly Kubernetes _kubernetes;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;
        private readonly TResourceDefinition _resourceDefinition;
        private readonly HandlerExecutor _handlerExecutor;
        private readonly string _namespace;
        private readonly List<Task> _eventQueue = new();

        public EventWatcher(
            ILogger<EventWatcher<TResourceDefinition>> logger, 
            Kubernetes kubernetes,
            IServiceScopeFactory serviceScopeFactory, 
            TResourceDefinition resourceDefinition,
            HandlerExecutor handlerExecutor,
            IOptions<KubeControllerOptions> options)
        {
            _logger = logger;
            _kubernetes = kubernetes;
            _serviceScopeFactory = serviceScopeFactory;
            _resourceDefinition = resourceDefinition;
            _handlerExecutor = handlerExecutor;
            _namespace = options.Value.Namespace;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartWatcher(cancellationToken).ConfigureAwait(false);
        }

        private async Task StartWatcher(CancellationToken cancellationToken)
        {
            var listResponse = _kubernetes.ListNamespacedCustomObjectWithHttpMessagesAsync(
                _resourceDefinition.Group, 
                _resourceDefinition.Version, 
                _namespace, 
                _resourceDefinition.Plural, 
                cancellationToken: cancellationToken,
                watch: true
            );

            var responseEnumerator = listResponse
                .WatchAsync<TResourceDefinition, object>(OnError)
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                await foreach (var (eventType, item) in responseEnumerator)
                {
                    // cleanup old tasks
                    _eventQueue.RemoveAll(t => t.IsCompleted);

                    // enqueue new handler task
                    var task = OnEventReceived(eventType, item, cancellationToken);
                    _eventQueue.Add(task);
                }
            }
            catch(TaskCanceledException)
            {
                // Termination signal is detected and we need to
                // allow the handlers to finish to prevent faulty state
                _logger.LogInformation("Cancellation signal received. Waiting for jobs to finish...");
                await Task.WhenAll(_eventQueue);
                _logger.LogDebug("All handlers completed succesfully");
            }
        }

        private async Task OnEventReceived(WatchEventType eventType, TResourceDefinition item, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Controller received event {EventType} {GroupName}/{CustomResource}: {Namespace}/{ResourceName}",
                eventType,
                _resourceDefinition.Group,
                _resourceDefinition.Singular,
                item.Namespace(),
                item.Name());

            try
            {
                await _handlerExecutor.ExecuteHandler(eventType, item, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred on the {EventType} call of {ResourceName} ({CustomResource})", 
                    eventType, 
                    item.Name(), 
                    _resourceDefinition.Singular);
            }
        }

        private void OnError(Exception exception)
        {
            _logger.LogCritical(0, exception, "Resource watcher for {CustomResource} failed with an unrecoverable error", typeof(TResourceDefinition).Name);
        }
    }
}
