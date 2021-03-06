﻿using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadClient
    {
        bool DownloadNzb(RemoteEpisode remoteEpisode);
        IEnumerable<QueueItem> GetQueue();
    }

}
