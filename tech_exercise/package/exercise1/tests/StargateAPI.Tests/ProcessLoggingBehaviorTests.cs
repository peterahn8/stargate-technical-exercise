using Microsoft.Extensions.Logging.Abstractions;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Logging;

namespace StargateAPI.Tests
{
    /// <summary>
    /// ProcessLoggingBehavior on its own, for outcomes the real handlers can't produce on demand: an
    /// unexpected exception, and a failure to write the log itself (D17).
    /// </summary>
    public class ProcessLoggingBehaviorTests
    {
        private readonly RecordingWriter _writer = new();
        private readonly CreatePerson _request = new() { Name = "John Doe" };

        [Fact]
        [Trait("AC", "SPEC-5")]
        public async Task Handle_UnexpectedException_IsLoggedAsErrorWithDetailsAndRethrown()
        {
            var behavior = Behavior(_writer);

            var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                behavior.Handle(_request, () => throw new InvalidOperationException("Boom"), CancellationToken.None));

            Assert.Equal("Boom", thrown.Message);
            var log = Assert.Single(_writer.Entries);
            Assert.Equal(("CreatePerson", "Failure", "Error", "Boom"), (log.RequestType, log.Outcome, log.Level, log.Message));
            Assert.Contains("System.InvalidOperationException: Boom", log.Exception);
        }

        [Fact]
        [Trait("AC", "SPEC-5")]
        public async Task Handle_LogWriteFails_RequestStillSucceeds()
        {
            var behavior = Behavior(new FailingWriter());
            var expected = new CreatePersonResult { Id = 7 };

            var result = await behavior.Handle(_request, () => Task.FromResult(expected), CancellationToken.None);

            Assert.Same(expected, result);
        }

        [Fact]
        [Trait("AC", "SPEC-5")]
        public async Task Handle_LogWriteFails_OriginalExceptionStillReachesTheCaller()
        {
            var behavior = Behavior(new FailingWriter());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                behavior.Handle(_request, () => throw new InvalidOperationException("Boom"), CancellationToken.None));
        }

        private static ProcessLoggingBehavior<CreatePerson, CreatePersonResult> Behavior(IProcessLogWriter writer) =>
            new(writer, NullLogger<ProcessLoggingBehavior<CreatePerson, CreatePersonResult>>.Instance);

        private sealed class RecordingWriter : IProcessLogWriter
        {
            public List<ProcessLog> Entries { get; } = new();

            public Task WriteAsync(ProcessLog entry)
            {
                Entries.Add(entry);
                return Task.CompletedTask;
            }
        }

        private sealed class FailingWriter : IProcessLogWriter
        {
            public Task WriteAsync(ProcessLog entry) => throw new InvalidOperationException("Database unavailable");
        }
    }
}
