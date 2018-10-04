using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rocket.Libraries.Emailing.Models;

namespace Rocket.Libraries.Emailing.Services.TemplatePreprocessing.LoopsPreprocessing
{
    public class LoopBlocksPreprocessor : PreProcessor
    {
        private const string BlockTagPrefix = "<lb-";

        public LoopBlocksPreprocessor(object valuesObject, List<string> templateLines)
            : base(valuesObject, templateLines)
        {
        }

        public override PreprocessingResult PreProcess()
        {
            var results = new PreprocessingResult
            {
                TemplateLines = new List<string>()
            };

            for (var i = 0; i < TemplateLines.Count; i++)
            {
                if (LineContainsStartOfBlock(TemplateLines[i]))
                {
                    results = InjectBlocks(i);
                    i = -1;
                }
            }
            if (results.TemplateLines.Count == 0)
            {
                results.TemplateLines = TemplateLines;
                results.Placeholders = new List<TemplatePlaceholder>();
            }
            return results;
        }

        private bool LineContainsStartOfBlock(string line)
        {
            var indexOfStarting = line.IndexOf(BlockTagPrefix);
            return indexOfStarting >= 0;
        }

        private PreprocessingResult InjectBlocks(int index)
        {
            var blockTags = GetFirstTagPair(TemplateLines[index], BlockTagPrefix);
            if (blockTags == null)
            {
                return null;
            }
            else
            {
                var blockContent = GetBlockContent(blockTags, index);

                return InsertCopiesOfBlock(blockContent, blockTags.RawTag, index);
            }
        }

        private PreprocessingResult InsertCopiesOfBlock(List<string> blockContent, string rawTag, int index)
        {
            if (ValuesObject == null)
            {
                return null;
            }
            else
            {
                var propertyName = GetAssociatedPropertyName(rawTag);
                var targetProperty = GetProperty(BlockTagPrefix, propertyName);
                return InjectCopiesOfBlock(blockContent, targetProperty, index);
            }
        }

        private PreprocessingResult InjectCopiesOfBlock(List<string> blockContent, PropertyInfo targetProperty, int index)
        {
            var copies = GetPropertyElementLength(targetProperty);
            if (copies == 0)
            {
                return null;
            }
            else
            {
                return GetProcessingResult(blockContent, targetProperty, index);
            }
        }

        private PreprocessingResult GetProcessingResult(List<string> blockContent, PropertyInfo targetProperty, int index)
        {
            var preprocessingResult = new PreprocessingResult
            {
                TemplateLines = TemplateLines,
                Placeholders = new List<TemplatePlaceholder>()
            };
            var innerPlaceholders = GetPlaceholdersInBlockContent(blockContent);
            var list = targetProperty.GetValue(ValuesObject) as ICollection;
            var listEnumerator = list.GetEnumerator();
            var listItemIndex = 0;
            var nestingStartTag = new TagPair(string.Empty, LoopsPreprocessor.ObjectNestingStartRawTag);
            var nestingStopTag = new TagPair(string.Empty, LoopsPreprocessor.ObjectNestingStopRawTag);

            while (listEnumerator.MoveNext())
            {
                var newLines = new List<string>
                {
                    $"{nestingStartTag.OpeningTag}{targetProperty.Name}-{listItemIndex}{nestingStartTag.ClosingTag}"
                };
                for (var i = 0; i < blockContent.Count; i++)
                {
                    var processedLine = GetLineWithInnerPlaceHoldersReplaced(blockContent[i], innerPlaceholders, i, targetProperty.Name, listEnumerator.Current, preprocessingResult, listItemIndex);
                    newLines.Add(processedLine);
                }
                newLines.Add($"{nestingStopTag.OpeningTag}{targetProperty.Name}{nestingStopTag.ClosingTag}");
                preprocessingResult.TemplateLines.InsertRange(index, newLines);
                index += newLines.Count;
                listItemIndex++;
            }
            return preprocessingResult;
        }

        private string GetLineWithInnerPlaceHoldersReplaced(string blockContentLine, Dictionary<int, List<string>> innerPlaceholders, int index, string propertyName, object currentObject, PreprocessingResult preprocessingResult, int listItemIndex)
        {
            if (innerPlaceholders.ContainsKey(index))
            {
                foreach (var placeholderName in innerPlaceholders[index])
                {
                    var originalPlaceHolder = $"{{{{{placeholderName}}}}}";
                    var replacementPlaceHolder = $"{{{{{propertyName}-{listItemIndex}-{placeholderName}}}}}";

                    var insertPlaceholderNow = placeholderName != "$";
                    if (insertPlaceholderNow)
                    {
                        var value = GetObjectValue(currentObject, placeholderName);

                        preprocessingResult.Placeholders.Add(new TemplatePlaceholder
                        {
                            Placeholder = replacementPlaceHolder,
                            Text = value
                        });
                    }
                    blockContentLine = blockContentLine.Replace(originalPlaceHolder, replacementPlaceHolder);
                }
                return blockContentLine;
            }
            else
            {
                return blockContentLine;
            }
        }

        private string GetObjectValue(object currentObject, string placeholderName)
        {
            var valueProperty = currentObject.GetType().GetProperties()
                .ToList()
                .FirstOrDefault(a => a.Name.Equals(placeholderName, StringComparison.CurrentCultureIgnoreCase));

            var containsProperty = currentObject != null && valueProperty != null;
            if (containsProperty)
            {
                var value = valueProperty.GetValue(currentObject);
                if (value == null)
                {
                    return string.Empty;
                }
                else
                {
                    return value.ToString();
                }
            }
            else
            {
                return string.Empty;
            }
        }

        private Dictionary<int, List<string>> GetPlaceholdersInBlockContent(List<string> blockContent)
        {
            var finalResult = new Dictionary<int, List<string>>();
            for (int i = 0; i < blockContent.Count; i++)
            {
                var placeholders = new List<string>();

                var bits = blockContent[i].Split("{{");
                if (bits.Length > 1)
                {
                    foreach (var item in bits)
                    {
                        var smallerBit = item.Split("}}");
                        if (smallerBit.Length > 1)
                        {
                            placeholders.Add(smallerBit[0]);
                        }
                    }
                }
                if (placeholders.Count > 0)
                {
                    finalResult.Add(i, placeholders);
                }
            }
            return finalResult;
        }

        private List<string> GetBlockContent(TagPair blockTags, int index)
        {
            var blockContent = new List<string>();
            var openingTagStart = TemplateLines[index].IndexOf(blockTags.OpeningTag);
            var contentStart = openingTagStart + blockTags.OpeningTag.Length;
            var hasAdditionalContentOnLine = TemplateLines[index].Length > contentStart;
            if (hasAdditionalContentOnLine)
            {
                blockContent.Add(TemplateLines[index].Substring(contentStart));
            }
            TemplateLines.RemoveAt(index);
            while (LineDoesNotHaveClosingTag(index, blockTags.ClosingTag) && NotAtLastLine(index))
            {
                blockContent.Add(TemplateLines[index]);
                TemplateLines.RemoveAt(index);
            }
            blockContent.Add(GetBlockContentFromLastLine(blockTags, index));
            CleanClosingTagOffLastLine(blockTags.ClosingTag, index);
            return blockContent;
        }

        private string GetBlockContentFromLastLine(TagPair blockTags, int index)
        {
            var indexOfClosingTag = TemplateLines[index].IndexOf(blockTags.ClosingTag);

            if (indexOfClosingTag < 0)
            {
                throw new Exception($"Tag {blockTags.OpeningTag} is not closed");
            }
            return TemplateLines[index].Substring(0, indexOfClosingTag);
        }

        private void CleanClosingTagOffLastLine(string closingTag, int index)
        {
            var indexOfClosingTag = TemplateLines[index].IndexOf(closingTag);
            var subStringStart = indexOfClosingTag + closingTag.Length + 1;
            var hasAdditionalContentAfterClosingTag = TemplateLines[index].Length > subStringStart;
            if (hasAdditionalContentAfterClosingTag)
            {
                TemplateLines[index] = TemplateLines[index].Substring(subStringStart);
            }
            else
            {
                TemplateLines.RemoveAt(index);
            }
        }

        private bool NotAtLastLine(int index)
        {
            return index < TemplateLines.Count;
        }

        private bool LineDoesNotHaveClosingTag(int index, string closingTag)
        {
            return TemplateLines[index].IndexOf(closingTag) < 0;
        }

        private string GetAssociatedPropertyName(string rawTag)
        {
            return rawTag.Substring(BlockTagPrefix.Length - 1);
        }

        private int GetPropertyElementLength(PropertyInfo targetProperty)
        {
            var value = targetProperty.GetValue(ValuesObject);
            if (value == null)
            {
                return 0;
            }
            else
            {
                var asCollection = value as ICollection;
                if (asCollection == null)
                {
                    throw new Exception($"Template only supports System.Collections.Generic.List. Property of type '{targetProperty.PropertyType}' could not be processed");
                }
                else
                {
                    return asCollection.Count;
                }
            }
        }
    }
}