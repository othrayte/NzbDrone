﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Providers
{
    public class InventoryProvider
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly HistoryProvider _historyProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public InventoryProvider(SeriesProvider seriesProvider, EpisodeProvider episodeProvider, HistoryProvider historyProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _historyProvider = historyProvider;
        }

        public InventoryProvider()
        {

        }


        public virtual bool IsMonitored(EpisodeParseResult parseResult)
        {
            var series = _seriesProvider.FindSeries(parseResult.CleanTitle);

            if (series == null)
            {
                Logger.Trace("{0} is not mapped to any series in DB. skipping", parseResult.CleanTitle);
                return false;
            }

            parseResult.Series = series;

            if (!series.Monitored)
            {
                Logger.Debug("{0} is present in the DB but not tracked. skipping.", parseResult.CleanTitle);
                return false;
            }

            var episodes = _episodeProvider.GetEpisodesByParseResult(parseResult, true);

            //return monitored if any of the episodes are monitored
            if (episodes.Any(episode => !episode.Ignored))
            {
                return true;
            }

            Logger.Debug("All episodes are ignored. skipping.");
            return false;
        }

        /// <summary>
        ///   Comprehensive check on whether or not this episode is needed.
        /// </summary>
        /// <param name = "parsedReport">Episode that needs to be checked</param>
        /// <returns>Whether or not the file quality meets the requirements </returns>
        /// <remarks>for multi episode files, all episodes need to meet the requirement
        /// before the report is downloaded</remarks>
        public virtual bool IsQualityNeeded(EpisodeParseResult parsedReport)
        {
            Logger.Trace("Checking if report meets quality requirements. {0}", parsedReport.Quality);
            if (!parsedReport.Series.QualityProfile.Allowed.Contains(parsedReport.Quality.QualityType))
            {
                Logger.Trace("Quality {0} rejected by Series' quality profile", parsedReport.Quality);
                return false;
            }

            var cutoff = parsedReport.Series.QualityProfile.Cutoff;

            foreach (var episode in _episodeProvider.GetEpisodesByParseResult(parsedReport, true))
            {
                //Checking File
                var file = episode.EpisodeFile;
                if (file != null)
                {
                    Logger.Trace("Comparing file quality with report. Existing file is {0} proper:{1}", file.Quality, file.Proper);
                    if (!IsUpgrade(new Quality { QualityType = file.Quality, Proper = file.Proper }, parsedReport.Quality, cutoff))
                        return false;
                }

                //Checking History
                var bestQualityInHistory = _historyProvider.GetBestQualityInHistory(episode.EpisodeId);
                if (bestQualityInHistory != null)
                {
                    Logger.Trace("Comparing history quality with report. History is {0}", bestQualityInHistory);
                    if (!IsUpgrade(bestQualityInHistory, parsedReport.Quality, cutoff))
                        return false;
                }

            }

            Logger.Debug("Episode {0} is needed", parsedReport);
            return true; //If we get to this point and the file has not yet been rejected then accept it
        }


        public static bool IsUpgrade(Quality currentQuality, Quality newQuality, QualityTypes cutOff)
        {
            if (currentQuality.QualityType >= cutOff)
            {
                Logger.Trace("Existing file meets cut-off. skipping.");
                return false;
            }

            if (newQuality > currentQuality)
                return true;

            if (currentQuality > newQuality)
            {
                Logger.Trace("existing item has better quality. skipping");
                return false;
            }

            if (currentQuality == newQuality && !newQuality.Proper)
            {
                Logger.Trace("same quality. not proper skipping");
                return false;
            }

            Logger.Debug("New item has better quality than existing item");
            return true;
        }
    }
}