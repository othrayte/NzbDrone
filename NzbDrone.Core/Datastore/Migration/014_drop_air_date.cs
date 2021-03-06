﻿using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Tags("")]
    [Migration(14)]
    public class drop_air_date : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            SQLiteAlter.DropColumns("Episodes", new []{ "AirDate" });
        }
    }
}
