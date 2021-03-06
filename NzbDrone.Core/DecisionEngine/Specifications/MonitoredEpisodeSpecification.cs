using System.Linq;
using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class MonitoredEpisodeSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MonitoredEpisodeSpecification(Logger logger)
        {
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Series or Episode is not monitored";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject)
        {
            if (!subject.Series.Monitored)
            {
                _logger.Debug("{0} is present in the DB but not tracked. skipping.", subject.Series);
                return false;
            }

            //return monitored if any of the episodes are monitored
            if (subject.Episodes.Any(episode => episode.Monitored))
            {
                return true;
            }

            _logger.Debug("No episodes are monitored. skipping.");
            return false;
        }
    }
}
