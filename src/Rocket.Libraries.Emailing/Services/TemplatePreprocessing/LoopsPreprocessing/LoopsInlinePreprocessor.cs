using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rocket.Libraries.Emailing.Models;

namespace Rocket.Libraries.Emailing.Services.TemplatePreprocessing.LoopsPreprocessing
{
    public class LoopsInlinePreprocessor : PreProcessor
    {
        private const string InLineTagPrefix = "<lv-";
        private PreprocessingResult _preprocessingResult = new PreprocessingResult();
        private Stack<object> _valueObjectsStack = new Stack<object>();
        private TagPair nestingStartTags = new TagPair(string.Empty, LoopsPreprocessor.ObjectNestingStartRawTag);
        private TagPair nestingStopTags = new TagPair(string.Empty, LoopsPreprocessor.ObjectNestingStopRawTag);

        public LoopsInlinePreprocessor(object valuesObject, List<string> templateLines)
             : base(valuesObject, templateLines)
        {
            _valueObjectsStack.Push(valuesObject);
        }

        public override PreprocessingResult PreProcess()
        {
            _preprocessingResult = new PreprocessingResult
            {
                Placeholders = new List<TemplatePlaceholder>()
            };
            for (var i = 0; i < TemplateLines.Count; i++)
            {
                var line = NestInIfRequired(TemplateLines[i]);
                line = NestOutIfRequired(line);
                TemplateLines[i] = GetLinePreprocessed(line);
            }
            _preprocessingResult.TemplateLines = TemplateLines;
            return _preprocessingResult;
        }

        private string NestInIfRequired(string line)
        {
            var newObjectDescription = GetEnclosedText(line, nestingStartTags.OpeningTag, nestingStartTags.ClosingTag, 0);
            if (string.IsNullOrEmpty(newObjectDescription))
            {
                return line;
            }
            else
            {
                var bits = newObjectDescription.Split('-');
                var listProperty = GetProperty(string.Empty, bits[0]);
                var listValue = listProperty.GetValue(ValuesObject);
                var enumerator = (listValue as ICollection).GetEnumerator();
                var targetIndex = int.Parse(bits[1]);
                var currentIndex = 0;
                while (enumerator.MoveNext())
                {
                    if (currentIndex == targetIndex)
                    {
                        _valueObjectsStack.Push(enumerator.Current);
                        return string.Empty;
                    }
                }
                throw new Exception("Could not find object to read from in nested list");
            }
        }

        private string NestOutIfRequired(string line)
        {
            var newObjectDescription = GetEnclosedText(line, nestingStopTags.OpeningTag, nestingStopTags.ClosingTag, 0);
            if (string.IsNullOrEmpty(newObjectDescription))
            {
                return line;
            }
            else
            {
                _valueObjectsStack.Pop();
                return string.Empty;
            }
        }

        private string GetLinePreprocessed(string line)
        {
            var inlineTags = GetFirstTagPair(line, InLineTagPrefix);
            var hasTags = inlineTags != null;
            while (hasTags)
            {
                line = BindTag(inlineTags, line);
                inlineTags = GetFirstTagPair(line, InLineTagPrefix);
                hasTags = inlineTags != null;
            }
            return line;
        }

        private string BindTag(TagPair inlineTags, string line)
        {
            var listProperty = GetProperty(InLineTagPrefix, inlineTags.UnPrefixedTag, _valueObjectsStack.First());
            var placeholder = GetEnclosedText(line, inlineTags.OpeningTag, inlineTags.ClosingTag, 0);
            var valuePropertyName = GetValuePropertyName(placeholder);
            return InsertPlaceholders(line, listProperty, inlineTags, valuePropertyName);
        }

        private string GetValuePropertyName(string placeholder)
        {
            var subStringStart = placeholder.IndexOf("{{");
            var subStringStop = placeholder.IndexOf("}}");
            var propertyName = placeholder.Substring(subStringStart + 2, subStringStop - subStringStart - 2);
            return propertyName;
        }

        private string GetTagInnerText(string line, TagPair inlineTags)
        {
            var innerText = GetEnclosedText(line, inlineTags.OpeningTag, inlineTags.ClosingTag, 0);
            return innerText;
        }

        private string[] GetTextBorderingTag(string line, string innerText, TagPair inlineTags)
        {
            var bits = line.Split(innerText);
            bits[0] = bits[0].Replace(inlineTags.OpeningTag, "");
            var indexOfClosingTag = bits[1].IndexOf(inlineTags.ClosingTag);

            bits[1] = bits[1].Substring(indexOfClosingTag + inlineTags.ClosingTag.Length);
            return bits;
        }

        private string InsertPlaceholders(string line, PropertyInfo listProperty, TagPair inlineTags, string valuePropertyName)
        {
            var value = listProperty.GetValue(ValuesObject);
            if (value == null)
            {
                return string.Empty;
            }
            var valueAsList = value as ICollection;
            if (valueAsList == null)
            {
                throw new Exception("Only lists are supported");
            }
            else
            {
                var enumerator = valueAsList.GetEnumerator();
                var originalMiddle = GetTagInnerText(line, inlineTags);
                var outerText = GetTextBorderingTag(line, originalMiddle, inlineTags);
                var newMiddle = string.Empty;
                var counter = 0;
                while (enumerator.MoveNext())
                {
                    var itemValue = enumerator.Current.GetType().GetProperties()
                        .ToList().First(a => a.Name.Equals(valuePropertyName, StringComparison.CurrentCultureIgnoreCase))
                        .GetValue(enumerator.Current);
                    var indexedPlaceholderName = "{{" + valuePropertyName + "-" + counter + "}}";
                    newMiddle += originalMiddle.Replace("{{" + valuePropertyName + "}}", indexedPlaceholderName);
                    var placeHolderValue = string.Empty;
                    if (itemValue != null)
                    {
                        placeHolderValue = itemValue.ToString();
                    }
                    _preprocessingResult.Placeholders.Add(new TemplatePlaceholder
                    {
                        Placeholder = indexedPlaceholderName,
                        Text = placeHolderValue
                    });
                    counter++;
                }
                return $"{outerText[0]}{newMiddle}{outerText[1]}";
            }
        }
    }
}