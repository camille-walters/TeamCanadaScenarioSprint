using System;
using Unity.DigitalTwins.Live.Sdk.Implementations;
using UnityEngine;

namespace Unity.DigitalTwins.Live.Sdk.Samples.Services
{
    public class DebugSignalBus : SignalBus
    {
        const string k_SubscribingLog = "Subscribing to action of type {0}.";
        const string k_UnsubscribingLog = "Unsubscribing action of type {0}.";
        const string k_FiringSignalLog = "Firing signal {0}.";

        public new void Subscribe<T>(Action<T> signalAction) where T : class
        {
            Debug.Log(string.Format(k_SubscribingLog, typeof(T).Name));
            base.Subscribe(signalAction);
        }

        public new void Subscribe<T>(Action signalAction) where T : class
        {
            Debug.Log(string.Format(k_SubscribingLog, typeof(T).Name));
            base.Subscribe<T>(signalAction);
        }

        public new void Unsubscribe<T>(Action<T> signalAction) where T : class
        {
            Debug.Log(string.Format(k_UnsubscribingLog, typeof(T).Name));
            base.Unsubscribe(signalAction);
        }

        public new void Unsubscribe<T>(Action signalAction) where T : class
        {
            Debug.Log(string.Format(k_UnsubscribingLog, typeof(T).Name));
            base.Unsubscribe<T>(signalAction);
        }

        public new void Fire<T>(T signal)
        {
            Debug.Log(string.Format(k_FiringSignalLog, signal.GetType().Name));
            base.Fire(signal);
        }
    }
}
