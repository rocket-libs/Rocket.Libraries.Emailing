using System;
using System.Collections.Generic;
using System.Text;
using Rocket.Libraries.Emailing.Models;

namespace Rocket.Libraries.Emailing.Services.TemplatePreprocessing.LoopsPreprocessing
{
    public class LoopsPreprocessor : PreProcessor
    {
        public LoopsPreprocessor(object valuesObject, List<string> templateLines)
            : base(valuesObject, templateLines)
        {
        }

        public override PreprocessingResult PreProcess()
        {
            var result = new LoopBlocksPreprocessor(ValuesObject, TemplateLines).PreProcess();
            return null;
        }
    }
}