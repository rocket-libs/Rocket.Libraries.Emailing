namespace Rocket.Libraries.Emailing.Services.TemplatePreprocessing.LoopsPreprocessing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Rocket.Libraries.Emailing.Models;
    using Rocket.Libraries.Validation.Services;

    public class LoopBlocksPreprocessor : PreProcessor
    {
        private const string BlockTagPrefix = "<lb-";
        private List<NestedBlockDescription> _nestedBlockDescriptions = new List<NestedBlockDescription>();

        public LoopBlocksPreprocessor(object valuesObject, List<string> templateLines, int nestingLevel, string key, List<TemplatePlaceholder> existingPlaceholders)
            : base(valuesObject, templateLines, nestingLevel, key, existingPlaceholders)
        {
        }

        public override PreprocessingResult PreProcess()
        {
            var results = new PreprocessingResult
            {
                TemplateLines = new List<string>(),
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
            var indexOfStarting = line.IndexOf(BlockTagPrefix, StringComparison.InvariantCulture);
            var startOfBlockFound = indexOfStarting >= 0;
            if (startOfBlockFound)
            {
                var tagPair = GetFirstTagPair(line, BlockTagPrefix);
                new DataValidator().EvaluateImmediate(() => tagPair == null, $"Contrary to expectation, could not find a block opening tag on line '{line}'");
                var isValidForCurrentLevel = IsCachedTag(tagPair) == false;
                return isValidForCurrentLevel;
            }
            else
            {
                return false;
            }
        }

        private bool IsCachedTag(TagPair tagPair)
        {
            var cachedInstance = _nestedBlockDescriptions.FirstOrDefault(a => a.ChildTag.RawTag.Equals(tagPair.RawTag, StringComparison.InvariantCulture));
            return cachedInstance != null;
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

        private bool LineContainsBlockTagClose(string line, TagPair tagPair)
        {
            return line.Contains(tagPair.ClosingTag, StringComparison.InvariantCulture);
        }

        private TagPair GetCurrentNestedTagIfExists(TagPair currentNestedTagPair, string line)
        {
            if (currentNestedTagPair == null)
            {
                currentNestedTagPair = GetFirstTagPair(line, BlockTagPrefix);
                return currentNestedTagPair;
            }
            else
            {
                var containsClosingBlockTag = LineContainsBlockTagClose(line, currentNestedTagPair);
                if (containsClosingBlockTag)
                {
                    return null;
                }
                else
                {
                    return currentNestedTagPair;
                }
            }
        }

        private void InjectExistingPlaceholdersIfAvailable(PreprocessingResult preprocessingResult)
        {
            if (ExistingPlaceholders != null && ExistingPlaceholders.Count > 0)
            {
                preprocessingResult.Placeholders.AddRange(ExistingPlaceholders);
            }
        }

        private PreprocessingResult HandleNestingIfRequired(bool nestingFound, PreprocessingResult preprocessingResult, object currentObject)
        {
            preprocessingResult.TotalNewLinesAfterHandlingNesting = 0;
            if (nestingFound)
            {
                var linesBeforeRecursion = preprocessingResult.TemplateLines.Count;
                var nestingLevel = CurrentNestingLevel + 1;
                var key = Guid.NewGuid().ToString();
                preprocessingResult = new LoopsPreprocessor(currentObject, preprocessingResult.TemplateLines, CurrentNestingLevel + 1, key, preprocessingResult.Placeholders)
                    .PreProcess();
                var linesAfterRecursion = preprocessingResult.TemplateLines.Count;
                var totalNewLinesFromRecursion = linesAfterRecursion - linesBeforeRecursion;
                new DataValidator().EvaluateImmediate(() => totalNewLinesFromRecursion < 0, "Decrement of lines after recursion for nested blocks in templates has not been tested.");
                preprocessingResult.TotalNewLinesAfterHandlingNesting = totalNewLinesFromRecursion;
            }

            return preprocessingResult;
        }

        private PreprocessingResult GetProcessingResult(List<string> blockContent, PropertyInfo targetProperty, int index)
        {
            var preprocessingResult = new PreprocessingResult
            {
                TemplateLines = TemplateLines,
                Placeholders = new List<TemplatePlaceholder>(),
            };
            InjectExistingPlaceholdersIfAvailable(preprocessingResult);
            var nestingFound = false;
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
                    $"{nestingStartTag.OpeningTag}{targetProperty.Name}-{listItemIndex}{nestingStartTag.ClosingTag}",
                };

                var currentNestedTagPair = default(TagPair);
                for (var i = 0; i < blockContent.Count; i++)
                {
                    currentNestedTagPair = GetCurrentNestedTagIfExists(currentNestedTagPair, blockContent[i]);
                    var isInNestedBlock = currentNestedTagPair != null;
                    nestingFound = nestingFound || isInNestedBlock;
                    var processedLine = GetLineWithInnerPlaceHoldersReplaced(blockContent[i], innerPlaceholders, i, targetProperty.Name, listEnumerator.Current, preprocessingResult, listItemIndex, isInNestedBlock);
                    newLines.Add(processedLine);
                }

                newLines.Add($"{nestingStopTag.OpeningTag}{targetProperty.Name}-{listItemIndex}{nestingStopTag.ClosingTag}");
                preprocessingResult.TemplateLines.InsertRange(index, newLines);

                preprocessingResult = HandleNestingIfRequired(nestingFound, preprocessingResult, listEnumerator.Current);

                index += newLines.Count + preprocessingResult.TotalNewLinesAfterHandlingNesting;
                listItemIndex++;
            }

            return preprocessingResult;
        }

        private string GetReplacementPlaceholder(string propertyName, string placeholderName, bool isToStringPlaceholder, int listItemIndex, bool isInNestedBlock)
        {
            if (isToStringPlaceholder || isInNestedBlock)
            {
                return "{{" + GetPlaceholderPrefixIfRequired() + placeholderName + "}}";
            }
            else
            {
                return $"{{{{{propertyName}{GetPlaceholderPrefixIfRequired()}-{listItemIndex}-{placeholderName}}}}}";
            }
        }

        private string GetPlaceholderPrefixIfRequired()
        {
            if (string.IsNullOrEmpty(Key))
            {
                return string.Empty;
            }
            else
            {
                return $"{Key}-";
            }
        }

        private string GetLineWithInnerPlaceHoldersReplaced(string blockContentLine, Dictionary<int, List<string>> innerPlaceholders, int index, string propertyName, object currentObject, PreprocessingResult preprocessingResult, int listItemIndex, bool isInNestedBlock)
        {
            if (innerPlaceholders.ContainsKey(index))
            {
                foreach (var placeholderName in innerPlaceholders[index])
                {
                    var originalPlaceHolder = $"{{{{{placeholderName}}}}}";
                    var isToStringPlaceholder = placeholderName.Equals(LoopsPreprocessor.ToStringPlaceholder, StringComparison.CurrentCultureIgnoreCase);
                    var replacementPlaceHolder = GetReplacementPlaceholder(propertyName, placeholderName, isToStringPlaceholder, listItemIndex, isInNestedBlock);

                    var insertPlaceholderNow = isToStringPlaceholder == false;
                    if (insertPlaceholderNow)
                    {
                        var value = GetObjectValue(currentObject, placeholderName);

                        preprocessingResult.Placeholders.Add(new TemplatePlaceholder
                        {
                            Placeholder = replacementPlaceHolder,
                            Text = value,
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

        private void CacheNestedBlockIfRequired(string currentLine, TagPair blockTag)
        {
            var beginningOfNestedBlock = LineContainsStartOfBlock(currentLine);
            if (beginningOfNestedBlock)
            {
                var nestedTag = GetFirstTagPair(currentLine, BlockTagPrefix);
                var nestedBlockDescription = new NestedBlockDescription
                {
                    ChildTag = nestedTag,
                    ParentTag = blockTag,
                };
                _nestedBlockDescriptions.Add(nestedBlockDescription);
            }
        }

        private List<string> GetBlockContent(TagPair blockTags, int index)
        {
            var blockContent = new List<string>();
            var openingTagStart = TemplateLines[index].IndexOf(blockTags.OpeningTag, StringComparison.InvariantCulture);
            var contentStart = openingTagStart + blockTags.OpeningTag.Length;
            var hasAdditionalContentOnLine = TemplateLines[index].Length > contentStart;
            if (hasAdditionalContentOnLine)
            {
                var targetContent = TemplateLines[index].Substring(contentStart);
                CacheNestedBlockIfRequired(targetContent, blockTags);
                blockContent.Add(targetContent);
            }

            TemplateLines.RemoveAt(index);
            while (LineDoesNotHaveClosingTag(index, blockTags.ClosingTag) && NotAtLastLine(index))
            {
                var currentLine = TemplateLines[index];
                CacheNestedBlockIfRequired(currentLine, blockTags);
                blockContent.Add(currentLine);
                TemplateLines.RemoveAt(index);
            }

            blockContent.Add(GetBlockContentFromLastLine(blockTags, index));
            CleanClosingTagOffLastLine(blockTags.ClosingTag, index);
            return blockContent;
        }

        private string GetBlockContentFromLastLine(TagPair blockTags, int index)
        {
            var indexOfClosingTag = TemplateLines[index].IndexOf(blockTags.ClosingTag, StringComparison.InvariantCulture);

            if (indexOfClosingTag < 0)
            {
                throw new Exception($"Tag {blockTags.OpeningTag} is not closed");
            }

            var line = TemplateLines[index].Substring(0, indexOfClosingTag);
            CacheNestedBlockIfRequired(line, blockTags);
            return line;
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