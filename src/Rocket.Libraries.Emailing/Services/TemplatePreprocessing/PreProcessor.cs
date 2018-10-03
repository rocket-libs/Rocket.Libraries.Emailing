using Rocket.Libraries.Emailing.Models;
using System;
using System.Collections.Generic;

namespace Rocket.Libraries.Emailing.Services.TemplatePreprocessing
{
    public abstract class PreProcessor
    {
        public PreProcessor(object valuesObject, List<string> templateLines)
        {
            ValuesObject = valuesObject;
            TemplateLines = templateLines;
        }

        public object ValuesObject { get; }
        public List<string> TemplateLines { get; }

        public abstract PreprocessingResult PreProcess();

        protected string GetEnclosedText(string targetLine, string openingText, string closingText, int startIndex)
        {
            if (targetLine.Length <= startIndex)
            {
                throw new Exception($"Cannot perform search for text on string of length '{targetLine.Length}' beginning on character '{startIndex}'");
            }
            var openingIndex = targetLine.IndexOf(openingText, startIndex);
            if (openingIndex <= 0)
            {
                return string.Empty;
            }
            var closingIndex = targetLine.IndexOf(closingText, openingIndex);
            if (closingIndex <= 0)
            {
                return string.Empty;
            }
            var subStringLength = closingIndex - closingText.Length - openingIndex;
            var subStringStart = openingIndex + openingText.Length;
            if (subStringLength <= 0)
            {
                throw new Exception($"Cannot read string length '{subStringLength}");
            }
            return targetLine.Substring(subStringStart, subStringLength);
        }

        protected TagPair GetFirstTagPair(string line, string startWith)
        {
            const string charsToTrimFromStart = "<";
            var indexOfOpening = line.IndexOf(startWith);
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
            var substringStart = charsToTrimFromStart.Length;
            var substringLength = indexOfClosing - charsToTrimFromStart.Length;
            var rawTag = line.Substring(substringStart, substringLength);
            return new TagPair
            {
                RawTag = rawTag
            };
        }
    }
}