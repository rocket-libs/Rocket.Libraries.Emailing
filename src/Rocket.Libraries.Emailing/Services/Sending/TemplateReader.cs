namespace Rocket.Libraries.Emailing.Services.Sending
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Rocket.Libraries.Emailing.Models.Sending;

    public class TemplateReader
    {
        private readonly EmailingSettings _emailingSettings;

        [Obsolete("This parameterless constructor only exists to allow mocking during testing. Using this constructor for production code will almost certainly result in failure")]
        public TemplateReader()
        {
        }

        public TemplateReader(EmailingSettings emailingSettings)
        {
            _emailingSettings = emailingSettings;
        }

        public virtual List<string> GetContentFromTemplate(string template)
        {
            var templatePath = $@"{_emailingSettings.TemplatesDirectory}/{template}";
            var result = new List<string>();
            ThrowExceptionIfInvalid(templatePath);
            using (var fs = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            {
                using (var stream = new StreamReader(fs))
                {
                    while (stream.EndOfStream == false)
                    {
                        result.Add(stream.ReadLine());
                    }
                }
            }

            return result;
        }

        private void ThrowExceptionIfInvalid(string templatePath)
        {
            if (string.IsNullOrEmpty(_emailingSettings.TemplatesDirectory))
            {
                throw new Exception("Value for 'TemplatesDirectory' not set in appsettings.json");
            }
            else
            {
                if (!Directory.Exists(_emailingSettings.TemplatesDirectory))
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