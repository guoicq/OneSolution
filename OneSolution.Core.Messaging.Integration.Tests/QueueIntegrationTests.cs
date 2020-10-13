using System;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Threading;

namespace OneSolution.Core.Messaging.Integration.Tests
{
    public class QueueIntegrationTests : IClassFixture<QueueFixture>
    {
        private readonly QueueFixture queueFixture;
        private readonly int delaySeconds = 2;
        
        public QueueIntegrationTests(QueueFixture queueFixture)
        {
            this.queueFixture = queueFixture;
        }

        [Fact]
        public async Task Should_send_and_receive_queue_message()
        {
            var message = new TestMessage { Id = Guid.NewGuid().ToString() };
            queueFixture.Message = message;
            queueFixture.TokenSource = new CancellationTokenSource();
            await queueFixture.Sender.SendMessagesAsync(message);


            try { 
                await Task.Delay(delaySeconds * 1000, queueFixture.TokenSource.Token);
            }
            catch (TaskCanceledException) { }
            queueFixture.ReceivedMsg.Should().NotBeNull();
            queueFixture.ReceivedMsg.Id.Should().Be(message.Id);
        }


    }



}
