namespace Rocket.Libraries.Emailing.Services.Sending.TemplatePreprocessing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Rocket.Libraries.Emailing.Models;

    public abstract class PreProcessor
    {
        private object _valuesObject;

        private List<TemplatePlaceholder> _existingPlaceholders;

        protected List<TemplatePlaceholder> ExistingPlaceholders
        {
            get
            {
                if (_existingPlaceholders == null)
                {
                    _existingPlaceholders = new List<TemplatePlaceholder>();
                }
                return _existingPlaceholders;
            }
            set => _existingPlaceholders = value;
        }

        public PreProcessor(object valuesObject, List<string> templateLines, int nestingLevel, string key, List<TemplatePlaceholder> existingPlaceholders)
        {
            ValuesObject = valuesObject;
            TemplateLines = templateLines;
            CurrentNestingLevel = nestingLevel;
            Key = key;
            ExistingPlaceholders = existingPlaceholders;
        }

        public object ValuesObject
        {
            get => _valuesObject;
            private set => _valuesObject = value;
        }

        public List<string> TemplateLines { get; }

        protected int CurrentNestingLevel { get; }
        protected string Key { get; }

        public abstract PreprocessingResult PreProcess();

        protected string GetEnclosedText(string targetLine, string openingText, string closingText, int startIndex)
        {
            if (string.IsNullOrEmpty(targetLine))
            {
                return string.Empty;
            }

            if (targetLine.Length <= startIndex)
            {
                throw new Exception($"Cannot perform search for text on string of length '{targetLine.Length}' beginning on character '{startIndex}'");
            }

            var openingIndex = targetLine.IndexOf(openingText, startIndex);
            if (openingIndex < 0)
            {
                return string.Empty;
            }

            var closingIndex = targetLine.IndexOf(closingText, openingIndex);
            if (closingIndex <= 0)
            {
                return string.Empty;
            }

            var subStringLength = closingIndex - closingText.Length - openingIndex + 1;
            var subStringStart = openingIndex + openingText.Length;
            if (subStringLength <= 0)
            {
                throw new Exception($"Cannot read string length '{subStringLength}");
            }

            return targetLine.Substring(subStringStart, subStringLength);
        }

        protected TagPair GetFirstTagPair(string line, string prefix)
        {
            var indexOfOpening = line.IndexOf(prefix);
            if (indexOfOpening < 0)
            {
                return null;
            }

            var indexOfClosing = line.IndexOf(">", indexOfOpening);
            if (indexOfClosing < 0)
            {
                return null;
            }

            var valueLength = indexOfClosing - indexOfOpening;
            var rawTag = line.Substring(indexOfOpening + 1, valueLength - 1);
            return new TagPair(prefix.Substring(1), rawTag);
        }

        protected PropertyInfo GetProperty(string tagPrefix, string propertyName, object parentObject = null)
        {
            if (parentObject == null)
            {
                parentObject = ValuesObject;
            }

            var targetProperty = parentObject.GetType().GetProperties()
                .FirstOrDefault(prop => prop.Name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase));
            if (targetProperty == null)
            {
                throw new Exception($"No property matches the placeholder '{tagPrefix}{propertyName}");
            }
            else
            {
                return targetProperty;
            }
        }
    }
}