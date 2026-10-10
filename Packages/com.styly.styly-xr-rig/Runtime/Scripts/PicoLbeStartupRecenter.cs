using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Styly.XRRig
{
    /// <summary>Controls automatic PICO LBE recentering during application startup.</summary>
    public static class PicoLbeStartupRecenter
    {
        private const double PreparationTimeoutSeconds = 15;
        // The optional PICO assembly supplies the backend before any scene Awake runs.
        internal static Func<IPicoLbeRecenterBackend> BackendFactory;
        private static bool attempted;

        /// <summary>
        /// Enabled by default for each app session. Set to false on the Unity main thread
        /// in a BeforeSceneLoad callback to prevent Enterprise Service initialization.
        /// Disabling during preparation cancels it on the next tick; re-enabling does
        /// not restart a cancelled or completed attempt.
        /// </summary>
        public static bool Enabled { get; set; } = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetSession()
        {
            Enabled = true;
            BackendFactory = null;
            attempted = false;
        }

        internal static PicoLbeRecenterRequest Begin(double now, Action<string> log)
        {
            if (!Enabled || attempted || BackendFactory == null)
                return null;

            attempted = true;
            return new PicoLbeRecenterRequest(BackendFactory(), now, PreparationTimeoutSeconds, log);
        }
    }

    internal interface IPicoLbeRecenterBackend
    {
        bool IsPicoRuntime { get; }
        bool IsTrackingReady { get; }
        bool Initialize();
        void Bind(Action<bool> completed);
        void QueryLbe(Action<string> completed);
        int Recenter();
    }

    internal sealed class PicoLbeRecenterRequest
    {
        internal enum Phase
        {
            WaitingForRuntime, Binding, QueryLbe, QueryingLbe, WaitingForTracking, ReadyToRecenter,
            Completed, Skipped, Failed, TimedOut, Cancelled
        }

        private readonly IPicoLbeRecenterBackend backend;
        private readonly Action<string> log;
        private readonly double deadline;
        private readonly ConcurrentQueue<Action> callbacks = new ConcurrentQueue<Action>();
        private int queryGeneration;

        internal Phase State { get; private set; } = Phase.WaitingForRuntime;
        internal bool Finished => State >= Phase.Completed;

        internal PicoLbeRecenterRequest(IPicoLbeRecenterBackend backend, double now, double timeout, Action<string> log)
        {
            this.backend = backend;
            this.log = log;
            deadline = now + (double.IsNaN(timeout) || double.IsInfinity(timeout) ? 15 : Math.Max(1, timeout));
        }

        // Called on the Unity main thread. SDK callbacks only enqueue data here.
        internal void Tick(double now)
        {
            if (Finished)
                return;

            if (!PicoLbeStartupRecenter.Enabled)
            {
                Finish(Phase.Cancelled, "Disabled through PicoLbeStartupRecenter.Enabled.");
                return;
            }

            if (now >= deadline)
            {
                Finish(Phase.TimedOut, "Preparation timed out while " + State + ".");
                return;
            }

            try
            {
                while (!Finished && callbacks.TryDequeue(out Action callback))
                    callback();

                switch (State)
                {
                    case Phase.WaitingForRuntime:
                        if (!backend.IsPicoRuntime)
                            return;
                        if (!backend.Initialize())
                        {
                            Finish(Phase.Failed, "Enterprise Service initialization failed.");
                            return;
                        }
                        State = Phase.Binding;
                        backend.Bind(success => callbacks.Enqueue(() => OnBound(success)));
                        break;
                    case Phase.QueryLbe:
                        QueryLbe(false);
                        break;
                    case Phase.WaitingForTracking:
                        if (!backend.IsPicoRuntime || !backend.IsTrackingReady)
                            return;
                        // LBE may have changed while tracking was unavailable.
                        QueryLbe(true);
                        break;
                    case Phase.ReadyToRecenter:
                        if (!backend.IsPicoRuntime || !backend.IsTrackingReady)
                        {
                            State = Phase.WaitingForTracking;
                            return;
                        }
                        int result = backend.Recenter();
                        Finish(result == 0 ? Phase.Completed : Phase.Failed, "Recenter returned " + result + " (0=success, 1=failure).");
                        break;
                }
            }
            catch (Exception exception)
            {
                Finish(Phase.Failed, exception.GetType().Name + ": " + exception.Message);
            }
        }

        private void OnBound(bool success)
        {
            if (State != Phase.Binding)
                return;
            if (success)
                State = Phase.QueryLbe;
            else
                Finish(Phase.Failed, "Enterprise Service binding failed.");
        }

        private void QueryLbe(bool finalCheck)
        {
            State = Phase.QueryingLbe;
            int generation = ++queryGeneration;
            backend.QueryLbe(status => callbacks.Enqueue(() => OnLbeStatus(status, finalCheck, generation)));
        }

        private void OnLbeStatus(string status, bool finalCheck, int generation)
        {
            if (State != Phase.QueryingLbe || generation != queryGeneration)
                return;
            if (status == "1")
                State = finalCheck ? Phase.ReadyToRecenter : Phase.WaitingForTracking;
            else if (status == "0")
                Finish(Phase.Skipped, "LBE mode is disabled.");
            else
                Finish(Phase.Failed, "LBE status is unavailable or unrecognized.");
        }

        internal void Cancel()
        {
            if (!Finished)
                Finish(Phase.Cancelled, "The requesting rig was disabled or destroyed.");
        }

        private void Finish(Phase state, string details)
        {
            State = state;
            while (callbacks.TryDequeue(out _)) { }
            log(state + ": " + details);
        }
    }
}
