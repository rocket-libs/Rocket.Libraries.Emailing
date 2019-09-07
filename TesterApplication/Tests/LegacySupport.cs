using Rocket.Libraries.Emailing.Services.Sending;
using System.Threading.Tasks;

namespace TesterApplication.Tests
{
    internal class LegacySupport
    {
        public async Task PureBCCingBehaviourStillWorks()
        {
            await new EmailBuilder()
                .AddBodyAsText("This is just a test")
                .AddRecepient("nyingimaina@gmail.com")
                .AddRecepient("nyingi.maina@outlook.com")
                .AddSender("tester@rocketdocuments.com", "Nyingi")
                .AddSubject("Pure BCC Test")
                .BuildAsync();
        }
    }
}