// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;

namespace RoadCaptain
{
    public abstract class MonitoringEvents
    {
        public abstract void Debug(string message, params object[] arguments);

        public abstract void Information(string message, params object[] arguments);

        public abstract void Warning(string message, params object[] arguments);
        public abstract void Warning(Exception exception, string message, params object[] arguments);

        public abstract void Error(string message, params object[] arguments);
        public abstract void Error(Exception exception, string messageTemplate, params object[] propertyValues);
    }
}
