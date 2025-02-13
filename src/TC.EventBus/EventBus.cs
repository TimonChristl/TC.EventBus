using System;
using System.Collections.Generic;
using System.Linq;

namespace TC.EventBus
{
    /// <summary>
    /// 
    /// </summary>
    public class EventBus
    {
        private readonly object lockObj = new object();
        private readonly Dictionary<Type, List<Subscription>> subscriptions = new Dictionary<Type, List<Subscription>>();

        /// <summary>
        /// Publish an event of type <typeparamref name="TEvent"/>, thereby invoking the handlers of all subscribers for that event type.
        /// This method can be called concurrently from different threads.
        /// </summary>
        /// <typeparam name="TEvent">Event type</typeparam>
        /// <param name="e">Event object</param>
        public void Publish<TEvent>(TEvent e) 
        {
            Subscription<TEvent>[] subscriptionsToInvoke;

            lock(lockObj)
            {
                var type = typeof(TEvent);

                if(!subscriptions.TryGetValue(type, out var subscriptionsForType))
                    return;

                subscriptionsToInvoke = subscriptionsForType.Cast<Subscription<TEvent>>().ToArray();
            }

            foreach(var subscription in subscriptionsToInvoke)
                subscription.Invoke(e);
        }

        /// <summary>
        /// Subscribe to events of type <typeparamref name="TEvent"/> with event handler <paramref name="handler"/>.
        /// This method can be called concurrently from different threads.
        /// </summary>
        /// <typeparam name="TEvent">Event type</typeparam>
        /// <param name="handler">Event handler</param>
        /// <returns></returns>
        public Subscription<TEvent> Subscribe<TEvent>(EventBusHandler<TEvent> handler) 
        {
            lock(lockObj)
            {
                var type = typeof(TEvent);

                if(!subscriptions.TryGetValue(type, out var subscriptionsForType))
                {
                    subscriptionsForType = new List<Subscription>();
                    subscriptions.Add(type, subscriptionsForType);
                }

                var subscription = new Subscription<TEvent>(handler);
                subscriptionsForType.Add(subscription);
                return subscription;
            }
        }

        /// <summary>
        /// Cancel <paramref name="subscription"/> so that its handler is no longer called for events of type <typeparamref name="TEvent"/>.
        /// If the subscription was already cancelled, nothing will happen.
        /// This method can be called concurrently from different threads.
        /// </summary>
        /// <typeparam name="TEvent">Event type</typeparam>
        /// <param name="subscription">Subscription to cancel</param>
        public void Unsubscribe<TEvent>(Subscription<TEvent> subscription)
        {
            lock(lockObj)
            {
                var type = typeof(TEvent);

                if(!subscriptions.TryGetValue(type, out var subscriptionsForType))
                    return;

                subscriptionsForType.Remove(subscription);
            }
        }

        /// <summary>
        /// Cancel <paramref name="subscription"/> so that its handler is no longer called for events of the type that was used to create the subscription.
        /// If the subscription was already cancalled, nothing will happen.
        /// This method can be called concurrently from different threads.
        /// </summary>
        /// <param name="subscription">Subscription to cancel</param>
        public void Unsubscribe(Subscription subscription)
        {
            lock(lockObj)
            {
                var type = subscription.EventType;

                if(!subscriptions.TryGetValue(type, out var subscriptionsForType))
                    return;

                subscriptionsForType.Remove(subscription);
            }
        }
    }

    /// <summary>
    /// Represents an event handler for the <see cref="EventBus"/>.
    /// </summary>
    /// <typeparam name="TEvent">Event type</typeparam>
    /// <param name="e">Event objet</param>
    public delegate void EventBusHandler<TEvent>(TEvent e);

    /// <summary>
    /// Represents a subscription to events of unknown type.
    /// </summary>
    public abstract class Subscription
    {
        internal abstract Type EventType { get; }
    }

    /// <summary>
    /// Represents a subscription to events of type <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public sealed class Subscription<TEvent> : Subscription
    {
        private readonly EventBusHandler<TEvent> handler;

        internal Subscription(EventBusHandler<TEvent> handler)
        {
            this.handler = handler;
        }

        internal void Invoke(TEvent e)
        {
            handler(e);
        }

        internal override Type EventType => typeof(TEvent);
    }
}
