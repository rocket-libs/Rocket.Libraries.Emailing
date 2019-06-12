namespace Rocket.Libraries.Emailing.Services.Sending
{
    using System.Collections.Generic;
    using System.Text;
    using Rocket.Libraries.Emailing.Models;
    using Rocket.Libraries.Emailing.Models.Sending;

    public class FilePlaceholderProcessor
    {
        private readonly TemplateReader _templateReader;

        private PlaceholderWriter _placeholderWriter = new PlaceholderWriter();

        public FilePlaceholderProcessor(TemplateReader templateReader)
        {
            _templateReader = templateReader;
        }

        public string PreprocessFilePlaceholdersIfRequired(string content, List<FilePlaceholder> filePlaceholders)
        {
            if (filePlaceholders == null || filePlaceholders.Count == 0)
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
            var templatePlaceholder = new TemplatePlaceholder
            {
                Placeholder = filePlaceholder.Placeholder,
                Text = string.Empty
            };
            var fileLines = _templateReader.GetContentFromTemplate(filePlaceholder.File);
            var stringBuilder = new StringBuilder();
            foreach (var line in fileLines)
            {
                stringBuilder.AppendLine(line);
            }
            templatePlaceholder.Text = stringBuilder.ToString();
            return _placeholderWriter.GetWithPlaceholdersReplaced(content, new List<TemplatePlaceholder> { templatePlaceholder });
        }
    }
}