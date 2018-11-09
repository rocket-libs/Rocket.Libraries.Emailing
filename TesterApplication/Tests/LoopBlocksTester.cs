using Furaha.Services.Logic.Legacy.Models.Reporting;
using Rocket.Libraries.Emailing.Services;
using Rocket.Libraries.Emailing.Services.TemplatePreprocessing.LoopsPreprocessing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TesterApplication.Models;

namespace TesterApplication.Tests
{
    internal class LoopBlocksTester
    {
        public async Task SendNestedAsync()
        {
            var grouped = new GroupedUninvoicedLoaded
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

            await new EmailBuilder()
                .AddBodyAsTemplate("UninvoicedLoaded.htm")
                .AddSubject("Testing sending of looped of nested")
                .AddRecepient("nyingi@auto-kenya.com")
                .AddPlaceholdersObject(new
                {
                    Items = grouped
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
                .AddPlaceholders(new List<Rocket.Libraries.Emailing.Models.TemplatePlaceholder>
                {
                    new Rocket.Libraries.Emailing.Models.TemplatePlaceholder
                    {
                        Placeholder = "{{vessel-name}}",
                        Text = "Rita"
                    },
                    new Rocket.Libraries.Emailing.Models.TemplatePlaceholder
                    {
                        Placeholder = "{{voyage-name}}",
                        Text = "Pushing Boundaries"
                    },
                    new Rocket.Libraries.Emailing.Models.TemplatePlaceholder
                    {
                        Placeholder = "{{sender-name}}",
                        Text = "Jane Cho"
                    },
                    new Rocket.Libraries.Emailing.Models.TemplatePlaceholder
                    {
                        Placeholder = "{{sender-email}}",
                        Text = "nyingimaina@gmail.com"
                    }
                })
                .BuildAsync();
        }
    }
}