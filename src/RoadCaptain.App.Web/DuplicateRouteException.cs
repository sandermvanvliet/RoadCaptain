namespace RoadCaptain.App.Web
{
    public class DuplicateRouteException : Exception
    {
        public DuplicateRouteException() 
            : base("Another route exists that is exactly the same")
        {
        }
    }
}