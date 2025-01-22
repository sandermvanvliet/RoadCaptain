// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.App.Shared
{
    internal class CompileTimeFeatures : IApplicationFeatures
    {
        public bool IsPreRelease
        {
            get
            {
#if PRE_RELEASE
                return true;
#else
                return false;
#endif
            }
        }
    }
}
