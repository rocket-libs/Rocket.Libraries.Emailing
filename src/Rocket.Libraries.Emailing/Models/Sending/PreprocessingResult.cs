namespace Rocket.Libraries.Emailing.Models.Sending
{
    using System.Collections.Generic;

    public class PreprocessingResult
    {
        public List<string> TemplateLines { get; set; } = new List<string>();

        public List<TemplatePlaceholder> Placeholders { get; set; } = new List<TemplatePlaceholder>();

        public int TotalNewLinesAfterHandlingNesting { get; set; }
    }
}