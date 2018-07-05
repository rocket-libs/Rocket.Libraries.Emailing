using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Microsoft.Extensions.Configuration.Json;
using Rocket.Libraries.Emailing.Services;
using System.Collections.Generic;
using Rocket.Libraries.Emailing.Models;
using Microsoft.Extensions.DependencyInjection;
using SparkPostDotNet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;

namespace TesterApplication
{
    class Program
    {
        static void Main(string[] args)
        {

            /*var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json",false);*/


            Environment.SetEnvironmentVariable("SPARKPOST_APIKEY", "fa0291b031781bd5dff87f1f4c6ebade277af621");
            Environment.SetEnvironmentVariable("SPARKPOST_SENDINGDOMAIN", "blah");
            BuildWebHost(args);

            var placeholders = new List<TemplatePlaceholder>
            {
                new TemplatePlaceholder
                {
                    Placeholder = "=text",
                    Text = "The quick brown fox jumps over the lazy dog"
                }
            };

            try
            {
                new Emailer()
                    .SendEmailAsync("nyingimaina@gmail.com", "Have A Cold", "<b>Bold</b> <u>Underline</u>", "text.htm", placeholders, "attachment name")
                    .ConfigureAwait(true);
                Console.WriteLine("Check your inbox");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build();
    }
}
