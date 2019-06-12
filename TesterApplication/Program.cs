using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using TesterApplication.Tests;
using TesterApplication.Tests.Templates;

namespace TesterApplication
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            /*var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json",false);*/

            Environment.SetEnvironmentVariable("SPARKPOST_APIKEY", "fa0291b031781bd5dff87f1f4c6ebade277af621");
            Environment.SetEnvironmentVariable("SPARKPOST_SENDINGDOMAIN", "mail.rocketdocuments.com");
            BuildWebHost(args);

            try
            {
                new ReceivingTests().FetchingEmailsWorksAsync().GetAwaiter().GetResult();
                new SubstituteAgreedTests().SendAsync().GetAwaiter().GetResult();
                new LegacySupport().PureBCCingBehaviourStillWorks().GetAwaiter().GetResult();
                Console.WriteLine("Succeeded");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Hit Key To Exit");
            Console.ReadKey();
        }

        public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build();
    }
}