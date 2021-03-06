﻿using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class IndexerIntegrationFixture : IntegrationTest
    {
        [Test]
        public void should_have_built_in_indexer()
        {
            var indexers = Indexers.All();

            indexers.Should().NotBeEmpty();
            indexers.Should().NotContain(c => string.IsNullOrWhiteSpace(c.Name));
        }
    }
}