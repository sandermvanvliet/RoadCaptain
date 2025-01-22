// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal abstract class BaseStep
    {
        protected int Step { get; }
        private readonly ILogger _logger;

        protected BaseStep(ILogger logger, int step)
        {
            Step = step;
            _logger = logger;
        }

        protected ILogger Logger => _logger;

        public abstract Context Run(Context context);
    }
}
