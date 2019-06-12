using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;
using Rocket.Libraries.Emailing.Services.Receiving;
using System.Threading.Tasks;
using Moq;
using Rocket.Libraries.Emailing.Models.Receiving;
using MailKit.Net.Imap;
using MailKit;

namespace Rocket.Libraries.Emailing.Tests.Services.Receiver
{
    public class ImapReceiverTests
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

            var mockInboxManager = new Mock<InboxManager>();
            var mockImapClientProvider = new Mock<ImapClientProvider>();

            Action<int> setupMessageReader = index => mockInboxManager
                .Setup(a => a.GetMessageAsync(It.Is<int>(messageIndex => messageIndex == index)))
                .ReturnsAsync(messages[index]);

            setupMessageReader(0);
            setupMessageReader(1);
            setupMessageReader(2);

            mockInboxManager.Setup(a => a.GetMessageCount())
                .Returns(messages.Count);

            var imapReceiver = new ImapReceiver
            {
                InboxManager = mockInboxManager.Object,
                ImapClientProvider = mockImapClientProvider.Object
            };

            var result = await imapReceiver.GetMailAsync(null, terminalId);

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

            var mockInboxManager = new Mock<InboxManager>();
            var mockImapClientProvider = new Mock<ImapClientProvider>();

            Action<int> setupMessageReader = index => mockInboxManager
                .Setup(a => a.GetMessageAsync(It.Is<int>(messageIndex => messageIndex == index)))
                .ReturnsAsync(messages[index]);

            setupMessageReader(0);
            setupMessageReader(1);
            setupMessageReader(2);

            mockInboxManager.Setup(a => a.GetMessageCount())
                .Returns(messages.Count);

            var imapReceiver = new ImapReceiver
            {
                InboxManager = mockInboxManager.Object,
                ImapClientProvider = mockImapClientProvider.Object
            };

            var result = await imapReceiver.GetMailAsync(null, string.Empty);

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

            var mockInboxManager = new Mock<InboxManager>();
            var mockImapClientProvider = new Mock<ImapClientProvider>();

            Action<int> setupMessageReader = index => mockInboxManager
                .Setup(a => a.GetMessageAsync(It.Is<int>(messageIndex => messageIndex == index)))
                .ReturnsAsync(messages[index]);

            setupMessageReader(0);
            setupMessageReader(1);
            setupMessageReader(2);

            mockInboxManager.Setup(a => a.GetMessageCount())
                .Returns(messages.Count);

            var imapReceiver = new ImapReceiver
            {
                InboxManager = mockInboxManager.Object,
                ImapClientProvider = mockImapClientProvider.Object
            };

            var result = await imapReceiver.GetMailAsync(null, null);

            Assert.Equal(3, result.Count);
        }
    }
}