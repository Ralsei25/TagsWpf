using System.IO;
using TagsWpf.Services;
using Xunit;

namespace TagsWpf.Tests
{
    public class TagsCounterTest
    {
        private TagsCounter _tagsCounter;
        public TagsCounterTest()
        {
            _tagsCounter = new TagsCounter();
        }
        [Fact]
        public void Test1()
        {
            // Arrange
            string content = File.ReadAllText("YandexHtmlPage.html");
            string tag = "a";
            int expected = 95;

            // Act
            int actual = _tagsCounter.CountTags(content, tag);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}