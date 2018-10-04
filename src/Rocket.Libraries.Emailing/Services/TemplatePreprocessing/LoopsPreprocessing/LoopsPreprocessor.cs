using System;
using System.Collections.Generic;
using System.Text;
using Rocket.Libraries.Emailing.Models;

namespace Rocket.Libraries.Emailing.Services.TemplatePreprocessing.LoopsPreprocessing
{
    public class LoopsPreprocessor : PreProcessor
    {
        public const string ObjectNestingStartRawTag = "nesting-start";
        public const string ObjectNestingStopRawTag = "nesting-stop";

        public LoopsPreprocessor(object valuesObject, List<string> templateLines)
            : base(valuesObject, templateLines)
        {
        }

        public override PreprocessingResult PreProcess()
        {
            var blocksResult = new LoopBlocksPreprocessor(ValuesObject, TemplateLines).PreProcess();
            var inlineResult = new LoopsInlinePreprocessor(ValuesObject, blocksResult.TemplateLines).PreProcess();
            return null;
        }
    }
}