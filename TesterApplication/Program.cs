using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using TesterApplication.Tests;

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
                new LoopBlocksTester().SendLoopedBodyTestAsync().GetAwaiter().GetResult();
                Console.WriteLine("Succeeded");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
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