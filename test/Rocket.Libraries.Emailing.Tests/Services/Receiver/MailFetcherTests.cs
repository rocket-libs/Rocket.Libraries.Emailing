using MimeKit;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Rocket.Libraries.Emailing.Services.Receiving.MailboxAdapting;
using Rocket.Libraries.Emailing.Services.Receiving;

namespace Rocket.Libraries.Emailing.Tests.Services.Receiver
{
    public class MailFetcherTests
    {
        [Fact]
        public async Task TerminatesOnCorrectId()
        {
            var terminalId = "terminalId454CGh";
            var preTerminalId = "preTerminalId";
            var postTerminalId = "postTerminalId";

            var messages = (new List<MimeMessage>
            {
                new MimeMessage
                {
                    MessageId = preTerminalId
                },
                new MimeMessage
                {
                    MessageId = terminalId
                },
                new MimeMessage
                {
                    MessageId = postTerminalId
                },
            }).Select(a => new MimeMessage { MessageId = a.MessageId.ToLower() })
            .ToList();

            var mockMailboxWrapper = new Mock<MailBoxWrapper>();

            Action<int> setupMessageReader = index => mockMailboxWrapper
                .Setup(a => a.GetMessageAsync(It.Is<int>(messageIndex => messageIndex == index)))
                .ReturnsAsync(messages[index]);

            setupMessageReader(0);
            setupMessageReader(1);
            setupMessageReader(2);

            mockMailboxWrapper.Setup(a => a.MessageCount)
                .Returns(messages.Count);

            var mailBoxAdapter = new MailBoxAdapter(mockMailboxWrapper.Object);

            var result = await new MailFetcher().GetMessagesAsync(mailBoxAdapter, terminalId);

            Assert.Single(result);
            Assert.Collection(result, a => a.MessageId.Equals(terminalId, StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public async Task EmptyTerminatorReturnsAllData()
        {
            var alphaId = "alphaId";
            var betaId = "betaId";
            var charlieId = "charlieId";

            var messages = (new List<MimeMessage>
            {
                new MimeMessage
                {
                    MessageId = alphaId
                },
                new MimeMessage
                {
                    MessageId = betaId
                },
                new MimeMessage
                {
                    MessageId = charlieId
                },
            }).Select(a => new MimeMessage { MessageId = a.MessageId.ToLower() })
            .ToList();

            var mockMailboxWrapper = new Mock<MailBoxWrapper>();

            Action<int> setupMessageReader = index => mockMailboxWrapper
                .Setup(a => a.GetMessageAsync(It.Is<int>(messageIndex => messageIndex == index)))
                .ReturnsAsync(messages[index]);

            setupMessageReader(0);
            setupMessageReader(1);
            setupMessageReader(2);

            mockMailboxWrapper.Setup(a => a.MessageCount)
                .Returns(messages.Count);

            var mailBoxAdapter = new MailBoxAdapter(mockMailboxWrapper.Object);

            var result = await new MailFetcher().GetMessagesAsync(mailBoxAdapter, string.Empty);

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task NullTerminatorReturnsAllData()
        {
            var alphaId = "alphaId";
            var betaId = "betaId";
            var charlieId = "charlieId";

            var messages = (new List<MimeMessage>
            {
                new MimeMessage
                {
                    MessageId = alphaId
                },
                new MimeMessage
                {
                    MessageId = betaId
                },
                new MimeMessage
                {
                    MessageId = charlieId
                },
            }).Select(a => new MimeMessage { MessageId = a.MessageId.ToLower() })
            .ToList();

            var mockMailboxWrapper = new Mock<MailBoxWrapper>();

            Action<int> setupMessageReader = index => mockMailboxWrapper
                .Setup(a => a.GetMessageAsync(It.Is<int>(messageIndex => messageIndex == index)))
                .ReturnsAsync(messages[index]);

            setupMessageReader(0);
            setupMessageReader(1);
            setupMessageReader(2);

            mockMailboxWrapper.Setup(a => a.MessageCount)
                .Returns(messages.Count);

            var mailBoxAdapter = new MailBoxAdapter(mockMailboxWrapper.Object);

            var result = await new MailFetcher().GetMessagesAsync(mailBoxAdapter, null);

            Assert.Equal(3, result.Count);
        }
    }
}