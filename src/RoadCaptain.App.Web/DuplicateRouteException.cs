// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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
