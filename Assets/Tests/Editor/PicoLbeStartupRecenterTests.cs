using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Styly.XRRig;

public class PicoLbeStartupRecenterTests
{
    private FakeBackend backend;
    private List<string> messages;
    private PicoLbeRecenterRequest request;

    [SetUp]
    public void SetUp()
    {
        PicoLbeStartupRecenter.ResetSession();
        backend = new FakeBackend();
        messages = new List<string>();
        PicoLbeStartupRecenter.BackendFactory = () => backend;
        request = new PicoLbeRecenterRequest(backend, 0, 10, messages.Add);
    }

    [TearDown]
    public void TearDown() => PicoLbeStartupRecenter.ResetSession();

    [Test]
    public void EnabledByDefaultAndPreparesWithoutInspectorConfiguration()
    {
        Assert.That(PicoLbeStartupRecenter.Enabled, Is.True);
        Assert.That(PicoLbeStartupRecenter.Begin(0, messages.Add), Is.Not.Null);
        Assert.That(backend.InitializeCalls, Is.Zero);
    }

    [Test]
    public void CodeOptOutDoesNotInitializeOrConsumeTheSessionAttempt()
    {
        PicoLbeStartupRecenter.Enabled = false;
        Assert.That(PicoLbeStartupRecenter.Begin(0, messages.Add), Is.Null);
        Assert.That(messages, Is.Empty);
        Assert.That(backend.InitializeCalls, Is.Zero);
        PicoLbeStartupRecenter.Enabled = true;
        Assert.That(PicoLbeStartupRecenter.Begin(0, messages.Add), Is.Not.Null);
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void CodeOptOutCancelsPreparationAndCannotBeRestarted(int stage)
    {
        request = PicoLbeStartupRecenter.Begin(0, messages.Add);
        if (stage >= 0) ReachStage(stage);
        PicoLbeStartupRecenter.Enabled = false;
        if (backend.Bound != null) backend.Bound(true);
        if (backend.Lbe != null) backend.Lbe("1");
        backend.TrackingReady = true;
        request.Tick(5);
        Assert.That(request.State, Is.EqualTo(PicoLbeRecenterRequest.Phase.Cancelled));
        Assert.That(backend.RecenterCalls, Is.Zero);
        if (stage < 0) Assert.That(backend.InitializeCalls, Is.Zero);
        PicoLbeStartupRecenter.Enabled = true;
        if (backend.Bound != null) backend.Bound(true);
        if (backend.Lbe != null) backend.Lbe("1");
        request.Tick(6);
        Assert.That(PicoLbeStartupRecenter.Begin(6, messages.Add), Is.Null);
        Assert.That(backend.RecenterCalls, Is.Zero);
        Assert.That(messages.Count, Is.EqualTo(1));
    }

    [Test]
    public void MissingSdkSkipsWithoutPlatformCalls()
    {
        PicoLbeStartupRecenter.BackendFactory = null;
        Assert.That(PicoLbeStartupRecenter.Begin(0, messages.Add), Is.Null);
        Assert.That(messages, Is.Empty);
        Assert.That(backend.InitializeCalls, Is.Zero);
    }

    [Test]
    public void MultipleRigsAndRecreatedRigsShareOneAttempt()
    {
        var first = PicoLbeStartupRecenter.Begin(0, messages.Add);
        Assert.That(PicoLbeStartupRecenter.Begin(0, messages.Add), Is.Null);
        first.Cancel();
        Assert.That(PicoLbeStartupRecenter.Begin(1, messages.Add), Is.Null);
        PicoLbeStartupRecenter.Enabled = false;
        PicoLbeStartupRecenter.ResetSession();
        Assert.That(PicoLbeStartupRecenter.Enabled, Is.True);
        PicoLbeStartupRecenter.BackendFactory = () => backend;
        Assert.That(PicoLbeStartupRecenter.Begin(2, messages.Add), Is.Not.Null);
    }

    [Test]
    public void DoesNotInitializeOnANonPicoRuntime()
    {
        backend.PicoRuntime = false;
        request.Tick(0);
        request.Tick(9);
        Assert.That(backend.InitializeCalls, Is.Zero);
        request.Tick(10);
        Assert.That(request.State, Is.EqualTo(PicoLbeRecenterRequest.Phase.TimedOut));
    }

    [Test]
    public void WaitsForBindingLbeAndTrackingBeforeRecenteringOnce()
    {
        WaitForTracking();
        request.Tick(3);
        Assert.That(backend.RecenterCalls, Is.Zero);
        Assert.That(backend.QueryCalls, Is.EqualTo(1));
        backend.TrackingReady = true;
        request.Tick(4);
        Assert.That(backend.QueryCalls, Is.EqualTo(2));
        Assert.That(backend.RecenterCalls, Is.Zero);
        backend.Lbe("1");
        request.Tick(5);
        request.Tick(6);
        request.Tick(100);
        Assert.That(backend.RecenterCalls, Is.EqualTo(1));
        Assert.That(request.State, Is.EqualTo(PicoLbeRecenterRequest.Phase.Completed));
        Assert.That(messages.Count, Is.EqualTo(1));
    }

    [TestCase("0", "Skipped")]
    [TestCase("", "Failed")]
    [TestCase(null, "Failed")]
    [TestCase("unknown", "Failed")]
    public void RequiresAnExplicitLbeEnabledStatus(string status, string expected)
    {
        WaitForLbe();
        backend.Lbe(status);
        request.Tick(2);
        Assert.That(request.State.ToString(), Is.EqualTo(expected));
        Assert.That(backend.RecenterCalls, Is.Zero);
    }

    [Test]
    public void RechecksLbeAfterWaitingForTracking()
    {
        WaitForTracking();
        backend.TrackingReady = true;
        request.Tick(3);
        backend.Lbe("0");
        request.Tick(4);
        Assert.That(request.State, Is.EqualTo(PicoLbeRecenterRequest.Phase.Skipped));
        Assert.That(backend.RecenterCalls, Is.Zero);
    }

    [Test]
    public void TrackingLossDuringFinalQueryRequiresFreshReadinessAndStatus()
    {
        WaitForTracking();
        backend.TrackingReady = true;
        request.Tick(3);
        Action<string> oldQuery = backend.Lbe;
        backend.TrackingReady = false;
        oldQuery("1");
        request.Tick(4);
        Assert.That(backend.RecenterCalls, Is.Zero);
        backend.TrackingReady = true;
        request.Tick(5);
        oldQuery("1");
        request.Tick(6);
        Assert.That(backend.RecenterCalls, Is.Zero, "A stale query cannot authorize another attempt.");
        backend.Lbe("1");
        request.Tick(7);
        Assert.That(backend.RecenterCalls, Is.EqualTo(1));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void TimeoutAndLateCallbacksNeverRecenter(int stage)
    {
        ReachStage(stage);
        request.Tick(10);
        if (backend.Bound != null) backend.Bound(true);
        if (backend.Lbe != null) backend.Lbe("1");
        backend.TrackingReady = true;
        request.Tick(11);
        Assert.That(request.State, Is.EqualTo(PicoLbeRecenterRequest.Phase.TimedOut));
        Assert.That(backend.RecenterCalls, Is.Zero);
        Assert.That(messages.Count, Is.EqualTo(1));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void CancellationAndLateCallbacksNeverRecenter(int stage)
    {
        ReachStage(stage);
        request.Cancel();
        if (backend.Bound != null) backend.Bound(true);
        if (backend.Lbe != null) backend.Lbe("1");
        backend.TrackingReady = true;
        request.Tick(5);
        request.Cancel();
        Assert.That(request.State, Is.EqualTo(PicoLbeRecenterRequest.Phase.Cancelled));
        Assert.That(backend.RecenterCalls, Is.Zero);
        Assert.That(messages.Count, Is.EqualTo(1));
    }

    [Test]
    public void CallbackThreadsDoNotInvokeAnyBackendOrLoggingOperations()
    {
        request.Tick(0);
        Task.Run(() => backend.Bound(true)).GetAwaiter().GetResult();
        Assert.That(backend.QueryCalls, Is.Zero);
        request.Tick(1);
        backend.TrackingReady = true;
        Task.Run(() => backend.Lbe("1")).GetAwaiter().GetResult();
        Assert.That(backend.RecenterCalls, Is.Zero);
        request.Tick(2);
        Task.Run(() => backend.Lbe("1")).GetAwaiter().GetResult();
        Assert.That(messages, Is.Empty);
        request.Tick(3);
        Assert.That(backend.RecenterCalls, Is.EqualTo(1));
    }

    [Test]
    public void FailedInitializationDoesNotBind()
    {
        backend.InitializeResult = false;
        request.Tick(0);
        Assert.That(request.State, Is.EqualTo(PicoLbeRecenterRequest.Phase.Failed));
        Assert.That(backend.Bound, Is.Null);
    }

    [Test]
    public void FailedBindingDoesNotQueryLbe()
    {
        request.Tick(0);
        backend.Bound(false);
        request.Tick(1);
        Assert.That(request.State, Is.EqualTo(PicoLbeRecenterRequest.Phase.Failed));
        Assert.That(backend.QueryCalls, Is.Zero);
    }

    [Test]
    public void FailedRecenterIsLoggedAndNeverRetried()
    {
        WaitForTracking();
        backend.TrackingReady = true;
        backend.RecenterResult = 1;
        request.Tick(3);
        backend.Lbe("1");
        request.Tick(4);
        request.Tick(5);
        Assert.That(request.State, Is.EqualTo(PicoLbeRecenterRequest.Phase.Failed));
        Assert.That(backend.RecenterCalls, Is.EqualTo(1));
        Assert.That(messages[0], Does.Contain("Recenter returned 1"));
    }

    [TestCase("initialize")]
    [TestCase("bind")]
    [TestCase("query")]
    [TestCase("recenter")]
    public void SdkExceptionsFinishTheRequest(string operation)
    {
        backend.ThrowOn = operation;
        backend.TrackingReady = true;
        request.Tick(0);
        if (backend.Bound != null) backend.Bound(true);
        request.Tick(1);
        if (backend.Lbe != null) backend.Lbe("1");
        request.Tick(2);
        if (backend.Lbe != null) backend.Lbe("1");
        request.Tick(3);
        Assert.That(request.State, Is.EqualTo(PicoLbeRecenterRequest.Phase.Failed));
        Assert.That(messages.Count, Is.EqualTo(1));
    }

    private void ReachStage(int stage)
    {
        request.Tick(0);
        if (stage >= 1) { backend.Bound(true); request.Tick(1); }
        if (stage >= 2) { backend.Lbe("1"); request.Tick(2); }
        if (stage >= 3) { backend.TrackingReady = true; request.Tick(3); }
    }

    private void WaitForLbe() => ReachStage(1);
    private void WaitForTracking() => ReachStage(2);

    private sealed class FakeBackend : IPicoLbeRecenterBackend
    {
        internal bool PicoRuntime = true, TrackingReady, InitializeResult = true;
        internal int InitializeCalls, QueryCalls, RecenterCalls, RecenterResult;
        internal string ThrowOn;
        internal Action<bool> Bound;
        internal Action<string> Lbe;
        private readonly int mainThread = Environment.CurrentManagedThreadId;

        public bool IsPicoRuntime => PicoRuntime;
        public bool IsTrackingReady => TrackingReady;
        public bool Initialize() { Check("initialize"); InitializeCalls++; return InitializeResult; }
        public void Bind(Action<bool> completed) { Check("bind"); Bound = completed; }
        public void QueryLbe(Action<string> completed) { Check("query"); QueryCalls++; Lbe = completed; }
        public int Recenter() { Check("recenter"); RecenterCalls++; return RecenterResult; }

        private void Check(string operation)
        {
            Assert.That(Environment.CurrentManagedThreadId, Is.EqualTo(mainThread));
            if (ThrowOn == operation) throw new InvalidOperationException("SDK " + operation + " failed");
        }
    }
}
