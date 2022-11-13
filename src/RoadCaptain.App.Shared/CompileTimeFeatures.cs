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