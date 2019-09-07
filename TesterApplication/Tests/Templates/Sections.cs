using Rocket.Libraries.Emailing.Services.Sending;
using System.Threading.Tasks;

namespace TesterApplication.Tests.Templates
{
    internal class Sections
    {
        public async Task SendAsync()
        {
            await new EmailBuilder()
                .AddBodyAsTemplate("request-substitution.htm")
                .AddFilePlaceholder("{{head}}", "sections/head.section")
                .AddRecepient("nyingimaina@gmail.com")
                .AddSender("nyingimaina@rocketdocuments.com", "Nyingi Maina")
                .AddSubject("Hi")
                .BuildAsync();
        }
    }
}