using k8s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeController.Exceptions
{
    class UnknownEventTypeException : Exception
    {
        public UnknownEventTypeException(WatchEventType eventType) : base($"Unknown event type received: {eventType}")
        {
            EventType = eventType;
        }

        public WatchEventType EventType { get; }
    }
}
