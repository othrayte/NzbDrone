using NLog;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IQualityUpgradableSpecification
    {
        bool IsUpgradable(QualityProfile profile, QualityModel currentQuality, QualityModel newQuality = null);
    }

    public class QualityUpgradableSpecification : IQualityUpgradableSpecification
    {
        private readonly Logger _logger;

        public QualityUpgradableSpecification(Logger logger)
        {
            _logger = logger;
        }

        public bool IsUpgradable(QualityProfile profile, QualityModel currentQuality, QualityModel newQuality = null)
        {
            if (newQuality != null)
            {
                if (currentQuality >= newQuality)
                {
                    _logger.Trace("existing item has better or equal quality. skipping");
                    return false;
                }

                if (currentQuality.Quality == newQuality.Quality && newQuality.Proper)
                {
                    _logger.Trace("Upgrading existing item to proper.");
                    return true;
                }
            }

            if (currentQuality.Quality >= profile.Cutoff)
            {
                _logger.Trace("Existing item meets cut-off. skipping.");
                return false;
            }

            return true;
        }
    }
}
