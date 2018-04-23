using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Microsoft.Extensions.Configuration.Json;
using Rocket.Libraries.Emailing.Services;
using System.Collections.Generic;
using Rocket.Libraries.Emailing.Models;

namespace TesterApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Brave New World!");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json",false);

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
                    .SendEmail("nyingimaina@gmail.com", "Have A Cold", "<b>Bold</b> <u>Underline</u>", "text.htm", placeholders, "attachment name");
                Console.WriteLine("Check your inbox");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }
    }
}
