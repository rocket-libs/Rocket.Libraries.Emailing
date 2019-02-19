using Rocket.Libraries.Emailing.Services;
using System.Threading.Tasks;

namespace TesterApplication.Tests
{
    public class CCTesting
    {
        public async Task TestCCsAreSentCorrectly()
        {
            await new EmailBuilder()
                .AddBodyAsText("This is just a test")
                .AddCCRecepient("nyingimaina@gmail.com")
                .SetPrimaryRecepient("nyingi.maina@outlook.com")
                .AddSender("tester@rocketdocuments.com", "Nyingi")
                .AddSubject("CC Test")
                .BuildAsync();
        }
    }
}