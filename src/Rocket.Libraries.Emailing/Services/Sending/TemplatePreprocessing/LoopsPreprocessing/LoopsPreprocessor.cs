namespace Rocket.Libraries.Emailing.Services.Sending.TemplatePreprocessing.LoopsPreprocessing
{
    using System.Collections.Generic;
    using Rocket.Libraries.Emailing.Models;

    public class LoopsPreprocessor : PreProcessor
    {
        public const string ObjectNestingStartRawTag = "nesting-start";
        public const string ObjectNestingStopRawTag = "nesting-stop";
        public const string ToStringPlaceholder = "%s";

        public LoopsPreprocessor(object valuesObject, List<string> templateLines, int nestingLevel, string key, List<TemplatePlaceholder> existingPlaceholders)
            : base(valuesObject, templateLines, nestingLevel, key, existingPlaceholders)
        {
        }

        public override PreprocessingResult PreProcess()
        {
            var blocksResult = new LoopBlocksPreprocessor(ValuesObject, TemplateLines, CurrentNestingLevel, Key, ExistingPlaceholders).PreProcess();
            var inlineResult = new LoopsInlinePreprocessor(ValuesObject, blocksResult.TemplateLines, CurrentNestingLevel, Key, ExistingPlaceholders).PreProcess();
            var finalResult = GetFinalResult(blocksResult, inlineResult);
            return finalResult;
        }

        private PreprocessingResult GetFinalResult(PreprocessingResult blocksResult, PreprocessingResult inlineResult)
        {
            var finalResult = new PreprocessingResult
            {
                Placeholders = inlineResult.Placeholders,
                TemplateLines = inlineResult.TemplateLines,
            };
            finalResult.Placeholders.AddRange(blocksResult.Placeholders);
            return finalResult;
        }
    }
}