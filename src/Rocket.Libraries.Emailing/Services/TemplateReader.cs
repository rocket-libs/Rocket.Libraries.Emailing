using Rocket.Libraries.Emailing.Models;
using System;
using System.IO;

namespace Rocket.Libraries.Emailing.Services
{
    class TemplateReader
    {
        private readonly EmailingSettings _emailingSettings;

        public TemplateReader(EmailingSettings emailingSettings)
        {
            _emailingSettings = emailingSettings;
        }

        public string GetContentFromTemplate(string template)
        {
            var templatePath = $@"{_emailingSettings.TemplatesDirectory}/{template}";
            ThrowExceptionIfInvalid(templatePath);
            using (var fs = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            {
                using (var stream = new StreamReader(fs))
                {
                    return stream.ReadToEnd();
                }
            }
        }


        private void ThrowExceptionIfInvalid(string templatePath)
        {
            if(string.IsNullOrEmpty(_emailingSettings.TemplatesDirectory))
            {
                throw new Exception("Value for 'TemplatesDirectory' not set in appsettings.json");
            }
            else
            {
                if(!Directory.Exists(_emailingSettings.TemplatesDirectory))
                {
                    throw new Exception($"Templates directory '{_emailingSettings.TemplatesDirectory}' does not exist");
                }
            }
            if (!File.Exists(templatePath))
            {
                throw new Exception($"Could not find the template at path '{templatePath}'");
            }
        }
    }
}
