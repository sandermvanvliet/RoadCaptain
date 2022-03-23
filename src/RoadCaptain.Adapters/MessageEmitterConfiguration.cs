// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using Microsoft.Extensions.Configuration;

namespace RoadCaptain.Adapters
{
    internal class MessageEmitterConfiguration
    {
        public MessageEmitterConfiguration(IConfiguration configuration)
        {
            configuration.Bind(this);
        }

        /// <summary>
        ///     When enabled this will throttle the rate at which messages are pushed onto the dispatcher queue
        /// </summary>
        public bool ThrottleMessages { get; set; }

        /// <summary>
        ///     The delay to wait when <see cref="MessageThrottleHighWaterMark" /> is reached
        /// </summary>
        public int MessageThrottleDelayMilliseconds { get; set; } = 50;

        /// <summary>
        ///     The amount of messages on the queue after which throttling is performed
        /// </summary>
        public int MessageThrottleHighWaterMark { get; set; } = 25;
    }
}
