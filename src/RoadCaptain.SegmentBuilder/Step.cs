using Serilog;

namespace RoadCaptain.SegmentBuilder
{
    internal abstract class Step
    {
        private readonly ILogger _logger;

        protected Step(ILogger logger)
        {
            _logger = logger;
        }

        protected ILogger Logger => _logger;

        public abstract Context Run(Context context);
    }
}