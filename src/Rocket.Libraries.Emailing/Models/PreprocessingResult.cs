using System.Collections.Generic;

namespace Rocket.Libraries.Emailing.Models
{
    public class PreprocessingResult
    {
        public List<string> TemplateLines { get; set; } = new List<string>();

        public List<TemplatePlaceholder> Placeholders { get; set; } = new List<TemplatePlaceholder>();
    }
}