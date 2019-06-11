using Rocket.Libraries.Emailing.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Rocket.Libraries.Emailing.Tests.Services
{
    public class EmailBuilderTests
    {
        [Fact]
        public async Task KitchenSinkTestAsync()
        {
            await new EmailBuilder()
                .AddAttachmentAsTemplate("text.htm", "rescue")
                .AddBodyAsTemplate("body.htm")
                .AddPlaceholders(
                    new List<Models.TemplatePlaceholder>
                    {
                        new Models.TemplatePlaceholder
                        {
                            Placeholder = "=text",
                            Text = "This is content of the attachment"
                        },
                        new Models.TemplatePlaceholder
                        {
                            Placeholder = "=a",
                            Text = "Tintin"
                        },
                        new Models.TemplatePlaceholder
                        {
                            Placeholder = "=b",
                            Text = "funny"
                        },
                        new Models.TemplatePlaceholder
                        {
                            Placeholder = "=c",
                            Text = "Test"
                        }
                    }
                )
                .AddRecepient("nyingimaina@gmail.com")
                .AddSubject("Kitchen Sink =c")
                .BuildAsync();
        }

        [Fact]
        public async Task VerifyMissingRecipentIsReported()
        {
            var emailBuilder = new EmailBuilder()
                .AddBodyAsText("Body")
                .AddSubject("Subject");
            var ex = await Assert.ThrowsAsync<Exception>(async () => await emailBuilder.BuildAsync());
            Assert.Contains("Recipient", ex.Message);
        }

        [Fact]
        public async Task VerifyMissingBodyIsReported()
        {
            var emailBuilder = new EmailBuilder()
                .AddRecepient("Body")
                .AddSubject("Subject");
            var ex = await Assert.ThrowsAsync<Exception>(async () => await emailBuilder.BuildAsync());
            Assert.Contains("Body", ex.Message);
        }

        [Fact]
        public async Task VerifyMissingSubjectIsReported()
        {
            var emailBuilder = new EmailBuilder()
                .AddBodyAsText("Body")
                .AddRecepient("Subject");
            var ex = await Assert.ThrowsAsync<Exception>(async () => await emailBuilder.BuildAsync());
            Assert.Contains("Subject", ex.Message);
        }

        [Fact]
        public async Task TestCCsAreSentCorrectly()
        {
            await new EmailBuilder()
                .AddBodyAsText("This is just a test")
                .AddCCRecepient("nyingimaina@gmail.com")
                .SetPrimaryRecepient("nyingi.maina@outlook.com")
                .AddSender("nyingimaina@rocket-documents.com", "Nyingi")
                .AddSubject("CC Test")
                .BuildAsync();
        }
    }
}