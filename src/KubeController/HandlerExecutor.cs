using k8s;
using KubeController.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeController
{
    public class HandlerExecutor
    {
        private readonly ILogger<HandlerExecutor> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public HandlerExecutor(ILogger<HandlerExecutor> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task ExecuteHandler<TResourceDefinition>(
            WatchEventType eventType, 
            TResourceDefinition item, 
            CancellationToken cancellationToken) 
            where TResourceDefinition : CustomResourceDefinition
        {
            try
            {
                // resolve event handler in its own scope making it possible
                // to handle concurrent events within they own service scope.
                using var scope = _serviceScopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IOperationHandler<TResourceDefinition>>();

                await ExecuteHandlerAction(handler, eventType, item, cancellationToken);
            }
            // task is gracefully cancelled by the stopping token
            catch (TaskCanceledException) { }
            catch (UnknownEventTypeException ex)
            {
                _logger.LogWarning(ex, "Skipping event {EventType}", ex.EventType);
            }
        }

        private Task ExecuteHandlerAction<TResourceDefinition>(
            IOperationHandler<TResourceDefinition> handler, 
            WatchEventType eventType, 
            TResourceDefinition item,
            CancellationToken cancellationToken) 
            where TResourceDefinition : CustomResourceDefinition => eventType switch
        {
            WatchEventType.Added => handler.OnAdded(item, cancellationToken),
            WatchEventType.Modified => handler.OnUpdated(item, cancellationToken),
            WatchEventType.Deleted => handler.OnDeleted(item, cancellationToken),
            WatchEventType.Bookmark => handler.OnBookmarked(item, cancellationToken),
            WatchEventType.Error => handler.OnError(item, cancellationToken),
            _ => throw new UnknownEventTypeException(eventType)
        };
    }
}
