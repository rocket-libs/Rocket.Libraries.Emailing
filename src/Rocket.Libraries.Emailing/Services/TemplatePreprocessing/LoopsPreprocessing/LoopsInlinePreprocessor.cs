using System;
using System.Collections.Generic;
using System.Text;
using Rocket.Libraries.Emailing.Models;

namespace Rocket.Libraries.Emailing.Services.TemplatePreprocessing.LoopsPreprocessing
{
    public class LoopsInlinePreprocessor : PreProcessor
    {
        private const string InLineTag = "<lv-";

        public LoopsInlinePreprocessor(object valuesObject, List<string> templateLines)
            : base(valuesObject, templateLines)
        {
        }

        public override PreprocessingResult PreProcess()
        {
            for (var i = 0; i < TemplateLines.Count; i++)
            {
                var inlineTags = GetFirstTagPair(TemplateLines[i], InLineTag);
                var hasTags = inlineTags != null;
                //if (has)
            }
            return null;
        }
    }
}