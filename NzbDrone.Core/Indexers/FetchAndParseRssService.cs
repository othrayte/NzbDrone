﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Common.TPL;

namespace NzbDrone.Core.Indexers
{
    public interface IFetchAndParseRss
    {
        List<ReportInfo> Fetch();
    }

    public class FetchAndParseRssService : IFetchAndParseRss
    {
        private readonly IIndexerService _indexerService;
        private readonly IFetchFeedFromIndexers _feedFetcher;
        private readonly Logger _logger;

        public FetchAndParseRssService(IIndexerService indexerService, IFetchFeedFromIndexers feedFetcher, Logger logger)
        {
            _indexerService = indexerService;
            _feedFetcher = feedFetcher;
            _logger = logger;
        }

        public List<ReportInfo> Fetch()
        {
            var result = new List<ReportInfo>();

            var indexers = _indexerService.GetAvailableIndexers().ToList();

            if (!indexers.Any())
            {
                _logger.Warn("No available indexers. check your configuration.");
                return result;
            }

            _logger.Debug("Available indexers {0}", indexers.Count);


            var taskList = new List<Task>();
            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

            foreach (var indexer in indexers)
            {
                var indexerLocal = indexer;

                var task = taskFactory.StartNew(() =>
                     {
                         var indexerFeed = _feedFetcher.FetchRss(indexerLocal);

                         lock (result)
                         {
                             result.AddRange(indexerFeed);
                         }
                     }).LogExceptions();

                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());

            _logger.Debug("Found {0} reports", result.Count);

            return result;
        }
    }
}