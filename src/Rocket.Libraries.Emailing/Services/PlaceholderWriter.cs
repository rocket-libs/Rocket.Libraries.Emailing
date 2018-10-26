namespace Rocket.Libraries.Emailing.Services
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Rocket.Libraries.Emailing.Models;

    internal class PlaceholderWriter
    {
        public string GetWithPlaceholdersReplaced(string input, List<TemplatePlaceholder> placeholders)
        {
            if (placeholders == null || string.IsNullOrEmpty(input))
            {
                return input;
            }
            else
            {
                foreach (var placeholder in placeholders)
                {
                    input = Regex.Replace(input, placeholder.Placeholder, placeholder.Text);
                }
            }

            return input;
        }
    }
}