namespace RoadCaptain.App.Web
{
    internal class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message)
            : base(message)
        {
        }
    }
}