using Furaha.Services.Logic.Legacy.Models.Reporting;
using Rocket.Libraries.Emailing.Models.Sending;
using Rocket.Libraries.Emailing.Services.Sending;
using System.Collections.Generic;
using System.Threading.Tasks;
using TesterApplication.Models;

namespace TesterApplication.Tests
{
    internal class LoopBlocksTester
    {
        public async Task SendNestedAsync()
        {
            var groupCats = new GroupedUninvoicedLoaded
            {
                Group = "Cat",
                Rows = new List<UninvoicedLoaded>
                {
                    new UninvoicedLoaded
                    {
                        CompanyName = "Lion"
                    },
                    new UninvoicedLoaded
                    {
                        CompanyName = "Tiger"
                    }
                }
            };

            var groupDogs = new GroupedUninvoicedLoaded
            {
                Group = "Dogs",
                Rows = new List<UninvoicedLoaded>
                {
                    new UninvoicedLoaded
                    {
                        CompanyName = "Wolf"
                    },
                    new UninvoicedLoaded
                    {
                        CompanyName = "Hyena"
                    }
                }
            };

            await new EmailBuilder()
                .AddBodyAsTemplate("UninvoicedLoaded.htm")
                .AddSubject("Testing sending of looped of nested")
                .AddRecepient("nyingi@auto-kenya.com")
                .AddPlaceholdersObject(new
                {
                    Items = new List<GroupedUninvoicedLoaded> { groupCats, groupDogs }
                })
                .AddSender("nyingi@rocketdocuments.com", "Nyingi's Rocket Email")
                .BuildAsync();
        }

        public async Task SendUnnestedAsync()
        {
            var telexRequestInformation = new TelexRequestInformation();
            telexRequestInformation.Recepients = new List<Recepient>
                {
                    new Recepient
                    {
                        EmailAddress = "nyingimaina@gmail.com",
                        Name = "Nyingi 1"
                    },
                    new Recepient
                    {
                        EmailAddress = "nyingi@auto-kenya.com",
                        Name = "Nyingi 2"
                    }
                };

            telexRequestInformation.Mbls = new List<Mbl>
            {
                new Mbl
                {
                    HouseBillsInformation = new List<string>
                    {
                        "A","B"
                    },
                    Number = "1234"
                },
                new Mbl
                {
                    HouseBillsInformation = new List<string>
                    {
                        "1","2","3"
                    },
                    Number = "2357"
                }
            };

            await new EmailBuilder()
                .AddBodyAsTemplate("Request-Telex.htm")
                .AddSubject("Testing sending of looped placeholders")
                .AddRecepient("nyingi@auto-kenya.com")
                .AddRecepient("nyingimaina@gmail.com")
                .AddPlaceholdersObject(telexRequestInformation)
                .AddSender("nyingi@rocketdocuments.com", "Nyingi's Rocket Email")
                .AddPlaceholders(new List<TemplatePlaceholder>
                {
                    new TemplatePlaceholder
                    {
                        Placeholder = "{{vessel-name}}",
                        Text = "Rita"
                    },
                    new TemplatePlaceholder
                    {
                        Placeholder = "{{voyage-name}}",
                        Text = "Pushing Boundaries"
                    },
                    new TemplatePlaceholder
                    {
                        Placeholder = "{{sender-name}}",
                        Text = "Jane Cho"
                    },
                    new TemplatePlaceholder
                    {
                        Placeholder = "{{sender-email}}",
                        Text = "nyingimaina@gmail.com"
                    }
                })
                .BuildAsync();
        }
    }
}