// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.App.Web.Adapters.EntityFramework
{
    public class User
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? ZwiftSubject { get; set; }
        public string? ZwiftProfileId { get; set; }
        public IEnumerable<Route> Routes { get; set; }
    }
}
