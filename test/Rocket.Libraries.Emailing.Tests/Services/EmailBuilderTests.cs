using Rocket.Libraries.Emailing.Services;
using System;
using System.Collections.Generic;
using System.Text;
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
                .AddAttachment("text.htm", "rescue")
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
    }
}
