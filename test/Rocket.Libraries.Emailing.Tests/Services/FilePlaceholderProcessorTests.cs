using Moq;
using Rocket.Libraries.Emailing.Models;
using Rocket.Libraries.Emailing.Services.Sending;
using System.Collections.Generic;
using Xunit;

namespace Rocket.Libraries.Emailing.Tests.Services
{
    public class FilePlaceholderProcessorTests
    {
        [Fact]
        public void PreprocessFilePlaceholdersIfRequiredWorks()
        {
            const string fileLine = "2";
            const string freeText = "1";
            var content = "{{head}}" + freeText;
            var expectedResult = $"{fileLine}{freeText}";

            var fileplaceholders = new List<FilePlaceholder>
            {
                new FilePlaceholder
                {
                    Placeholder = "{{head}}"
                }
            };

            var mockTemplateReader = new Mock<TemplateReader>();
            mockTemplateReader.Setup(a => a.GetContentFromTemplate(It.IsAny<string>()))
                .Returns(new List<string>
                {
                    fileLine
                });
            var filePlaceholderProcessor = new FilePlaceholderProcessor(mockTemplateReader.Object);
            var result = filePlaceholderProcessor.PreprocessFilePlaceholdersIfRequired(content, fileplaceholders);
            Assert.Equal(expectedResult, result);
        }
    }
}