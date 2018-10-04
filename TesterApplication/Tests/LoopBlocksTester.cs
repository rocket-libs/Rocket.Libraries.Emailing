using Rocket.Libraries.Emailing.Services.TemplatePreprocessing.LoopsPreprocessing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TesterApplication.Models;

namespace TesterApplication.Tests
{
    internal class LoopBlocksTester
    {
        public void FillOutRequest()
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

            var templateLines = File.ReadAllLines("./Templates/Request-Telex.htm");
            new LoopsPreprocessor(telexRequestInformation, templateLines.ToList())
                .PreProcess();
        }
    }
}