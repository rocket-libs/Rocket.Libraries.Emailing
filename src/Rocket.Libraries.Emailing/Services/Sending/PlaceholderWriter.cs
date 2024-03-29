﻿namespace Rocket.Libraries.Emailing.Services.Sending
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Rocket.Libraries.Emailing.Models.Sending;

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
                    if (placeholder.Text == null)
                    {
                        placeholder.Text = string.Empty;
                    }

                    input = Regex.Replace(input, placeholder.Placeholder, placeholder.Text);
                }
            }

            return input;
        }
    }
}