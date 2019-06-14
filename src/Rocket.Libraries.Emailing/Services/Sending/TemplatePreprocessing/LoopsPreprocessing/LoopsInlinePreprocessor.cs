namespace Rocket.Libraries.Emailing.Services.Sending.TemplatePreprocessing.LoopsPreprocessing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Rocket.Libraries.Emailing.Models.Sending;

    public class LoopsInlinePreprocessor : PreProcessor
    {
        private int _nestTraversalCounter = 0;

        private class NestingInformation
        {
            public NestingInformation(int index, object Obj)
            {
                Index = index;
                this.Obj = Obj;
            }

            public int Index { get; }

            public object Obj { get; }
        }

        private object CurrentValuesObject => _nestingStack?.First()?.Obj;

        private const string InLineTagPrefix = "<lv-";

        private PreprocessingResult _preprocessingResult = new PreprocessingResult();

        private Stack<NestingInformation> _nestingStack = new Stack<NestingInformation>();

        private TagPair nestingStartTags = new TagPair(string.Empty, LoopsPreprocessor.ObjectNestingStartRawTag);

        private TagPair nestingStopTags = new TagPair(string.Empty, LoopsPreprocessor.ObjectNestingStopRawTag);

        public LoopsInlinePreprocessor(object valuesObject, List<string> templateLines, int nestingLevel, string key, List<TemplatePlaceholder> existingPlaceholders)
             : base(valuesObject, templateLines, nestingLevel, key, existingPlaceholders)
        {
            _nestingStack.Push(new NestingInformation(0, valuesObject));
        }

        public override PreprocessingResult PreProcess()
        {
            try
            {
                _preprocessingResult = new PreprocessingResult
                {
                    Placeholders = new List<TemplatePlaceholder>(),
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
            catch (Exception e)
            {
                throw;
            }
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
                _nestTraversalCounter++;
                var isCurrentLevel = _nestTraversalCounter == CurrentNestingLevel;
                if (isCurrentLevel)
                {
                    var bits = newObjectDescription.Split('-');
                    var listProperty = GetProperty(string.Empty, bits[0]);
                    var listValue = listProperty.GetValue(CurrentValuesObject);
                    var enumerator = (listValue as ICollection).GetEnumerator();
                    var targetIndex = int.Parse(bits[1]);
                    var currentIndex = 0;
                    while (enumerator.MoveNext())
                    {
                        if (currentIndex == targetIndex)
                        {
                            _nestingStack.Push(new NestingInformation(targetIndex, enumerator.Current));
                            return string.Empty;
                        }

                        currentIndex++;
                    }

                    throw new Exception("Could not find object to read from in nested list");
                }
                else
                {
                    return line;
                }
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
                if (_nestTraversalCounter == CurrentNestingLevel)
                {
                    _nestingStack.Pop();
                    _nestTraversalCounter--;
                    return string.Empty;
                }
                else
                {
                    _nestTraversalCounter--;
                    return line;
                }
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
            var listProperty = GetProperty(InLineTagPrefix, inlineTags.UnPrefixedTag, CurrentValuesObject);
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

        private IEnumerator GetEnumerator(PropertyInfo listProperty)
        {
            var value = listProperty.GetValue(CurrentValuesObject);
            if (value == null)
            {
                return null;
            }

            var valueAsList = value as ICollection;
            if (valueAsList == null)
            {
                throw new Exception("Only lists are supported for looping in templates");
            }
            else
            {
                return valueAsList.GetEnumerator();
            }
        }

        private string GetListObjectValueFromExplicitPropertyName(IEnumerator enumerator, string valuePropertyName)
        {
            var itemValue = enumerator.Current.GetType().GetProperties()
                    .ToList().First(a => a.Name.Equals(valuePropertyName, StringComparison.CurrentCultureIgnoreCase))
                    .GetValue(enumerator.Current);
            if (itemValue == null)
            {
                return string.Empty;
            }
            else
            {
                return itemValue.ToString();
            }
        }

        private string GetListObjectValueFromByToString(IEnumerator enumerator)
        {
            var itemValue = enumerator.Current;

            if (itemValue == null)
            {
                return string.Empty;
            }
            else
            {
                return itemValue.ToString();
            }
        }

        private string GetListObjectValue(IEnumerator enumerator, bool isToStringPlaceholder, string valuePropertyName)
        {
            var placeHolderValue = string.Empty;
            if (isToStringPlaceholder)
            {
                placeHolderValue = GetListObjectValueFromByToString(enumerator);
            }
            else
            {
                placeHolderValue = GetListObjectValueFromExplicitPropertyName(enumerator, valuePropertyName);
            }

            return placeHolderValue;
        }

        private string GetIndexedPlaceholderName(int listIndex, int valueIndex, string valuePropertyName, string ownerObjectName)
        {
            return "{{" + ownerObjectName + "-" + listIndex + "-" + valuePropertyName + "-" + valueIndex + "}}";
        }

        private string InsertPlaceholders(string line, PropertyInfo listProperty, TagPair inlineTags, string valuePropertyName)
        {
            var isToStringPlaceholder = valuePropertyName.Equals(LoopsPreprocessor.ToStringPlaceholder, StringComparison.CurrentCultureIgnoreCase);
            var ownerObjectName = CurrentValuesObject.GetType().Name;
            var nestedListIndex = _nestingStack.First().Index;

            var enumerator = GetEnumerator(listProperty);
            var originalMiddle = GetTagInnerText(line, inlineTags);
            var outerText = GetTextBorderingTag(line, originalMiddle, inlineTags);
            var newMiddle = string.Empty;
            var valueIndex = 0;
            while (enumerator.MoveNext())
            {
                var indexedPlaceholderName = GetIndexedPlaceholderName(nestedListIndex, valueIndex, valuePropertyName, ownerObjectName);
                newMiddle += originalMiddle.Replace("{{" + valuePropertyName + "}}", indexedPlaceholderName);
                var placeHolderValue = GetListObjectValue(enumerator, isToStringPlaceholder, valuePropertyName);
                _preprocessingResult.Placeholders.Add(new TemplatePlaceholder
                {
                    Placeholder = indexedPlaceholderName,
                    Text = placeHolderValue,
                });
                valueIndex++;
            }

            return $"{outerText[0]}{newMiddle}{outerText[1]}";
        }
    }
}