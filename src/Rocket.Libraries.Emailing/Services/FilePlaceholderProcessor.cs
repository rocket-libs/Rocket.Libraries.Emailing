using Rocket.Libraries.Emailing.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rocket.Libraries.Emailing.Services
{
    internal class FilePlaceholderProcessor
    {
        private PlaceholderWriter _placeholderWriter = new PlaceholderWriter();
        public string PreprocessFilePlaceholdersIfRequired(string content, List<FilePlaceholder> filePlaceholders)
        {
            if(filePlaceholders == null || filePlaceholders.Count == 0)
            {
                return content;
            }
            else
            {
                foreach (var item in filePlaceholders)
                {
                    content = InjectAPlaceholder(content, item);
                }
                return content;
            }
        }

        private string InjectAPlaceholder(string content, FilePlaceholder filePlaceholder)
        {
            ThrowExceptionIfFileMissing(filePlaceholder.File);
            var templatePlaceholder = new TemplatePlaceholder
            {
                Placeholder = filePlaceholder.Placeholder,
                Text = string.Empty
            };
            using (var fs = new FileStream(filePlaceholder.File, FileMode.Open, FileAccess.Read))
            {
                using (var stream = new StreamReader(fs))
                {
                    while (stream.EndOfStream == false)
                    {
                        templatePlaceholder.Text += stream.ReadLine();
                    }
                }
            }
            return _placeholderWriter.GetWithPlaceholdersReplaced(content, new List<TemplatePlaceholder> { templatePlaceholder });
        }

        private void ThrowExceptionIfFileMissing(string file)
        {
            if(!File.Exists(file))
            {
                throw new Exception($"File '{file}' specified as a placeholder datasource does not exist");
            }
        }
    }
}
