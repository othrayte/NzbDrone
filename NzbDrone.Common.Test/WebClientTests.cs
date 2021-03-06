﻿
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class WebClientTests : TestBase<HttpProvider>
    {
        [Test]
        public void DownloadString_should_be_able_to_dowload_text_file()
        {
            var jquery = Subject.DownloadString("http://www.google.com/robots.txt");

            jquery.Should().NotBeBlank();
            jquery.Should().Contain("Sitemap");
        }

        [TestCase("")]
        [TestCase("http://")]
        [TestCase(null)]
        [ExpectedException]
        public void DownloadString_should_throw_on_error(string url)
        {
            var jquery = Subject.DownloadString(url);
        }
    
    
        [Test]
        public void should_get_headers()
        {
            Subject.GetHeader("http://www.google.com").Should().NotBeEmpty();
        }
    }

}
