namespace Rocket.Libraries.Emailing.Services.TemplatePreprocessing.LoopsPreprocessing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rocket.Libraries.Emailing.Models;

    public class LoopsPreprocessor : PreProcessor
    {
        public const string ObjectNestingStartRawTag = "nesting-start";
        public const string ObjectNestingStopRawTag = "nesting-stop";
        public const string ToStringPlaceholder = "%s";

        public LoopsPreprocessor(object valuesObject, List<string> templateLines)
            : base(valuesObject, templateLines)
        {
        }

        public override PreprocessingResult PreProcess()
        {
            var blocksResult = new LoopBlocksPreprocessor(ValuesObject, TemplateLines).PreProcess();
            var inlineResult = new LoopsInlinePreprocessor(ValuesObject, blocksResult.TemplateLines).PreProcess();
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