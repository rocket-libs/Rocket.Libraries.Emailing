﻿using Rocket.Libraries.Emailing.Models.Sending;
using Rocket.Libraries.Emailing.Services.Sending;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Rocket.Libraries.Emailing.Tests.Services
{
    public class EmailBuilderTests
    {
        [Fact(Skip = "No longer in possession of domain below. Anyhow, this is an integration test.")]
        public async Task KitchenSinkTestAsync()
        {
            await new EmailBuilder()
                .AddAttachmentAsTemplate("text.htm", "rescue")
                .AddBodyAsTemplate("body.htm")
                .AddPlaceholders(
                    new List<TemplatePlaceholder>
                    {
                        new TemplatePlaceholder
                        {
                            Placeholder = "=text",
                            Text = "This is content of the attachment"
                        },
                        new TemplatePlaceholder
                        {
                            Placeholder = "=a",
                            Text = "Tintin"
                        },
                        new TemplatePlaceholder
                        {
                            Placeholder = "=b",
                            Text = "funny"
                        },
                        new TemplatePlaceholder
                        {
                            Placeholder = "=c",
                            Text = "Test"
                        }
                    }
                )
                .AddSender("sender@example.com", "Sender Name")
                .AddRecepient("nyingimaina@gmail.com")
                .AddSubject("Kitchen Sink =c")
                .BuildAsync();
        }

        [Fact]
        public async Task VerifyMissingRecipentIsReported()
        {
            var emailBuilder = new EmailBuilder()
                .AddBodyAsText("Body")
                .AddSender("sender@example.com", "Example Sender")
                .AddSubject("Subject");
            var ex = await Assert.ThrowsAsync<Exception>(async () => await emailBuilder.BuildAsync());
            Assert.Contains("Your email message has no recepients", ex.Message);
        }

        [Fact]
        public async Task VerifyMissingSenderIsReported()
        {
            var emailBuilder = new EmailBuilder()
                .AddBodyAsText("Body")
                .AddSubject("Subject");
            var ex = await Assert.ThrowsAsync<Exception>(async () => await emailBuilder.BuildAsync());
            Assert.Contains("Sender Email", ex.Message);
        }

        [Fact]
        public async Task VerifyMissingBodyIsReported()
        {
            var emailBuilder = new EmailBuilder()
                .AddRecepient("recepient@example.com")
                .AddSubject("Subject");
            var ex = await Assert.ThrowsAsync<Exception>(async () => await emailBuilder.BuildAsync());
            Assert.Contains("Body", ex.Message);
        }

        [Fact]
        public async Task VerifyMissingSubjectIsReported()
        {
            var emailBuilder = new EmailBuilder()
                .AddBodyAsText("Body")
                .AddRecepient("recepient@example.com");
            var ex = await Assert.ThrowsAsync<Exception>(async () => await emailBuilder.BuildAsync());
            Assert.Contains("Subject", ex.Message);
        }

        [Fact(Skip = "No longer in possession of domain below. Anyhow, this is an integration test.")]
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