// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

namespace RoadCaptain.Ports
{
    public interface IVersionChecker
    {
        (Release? official, Release? preRelease) GetLatestRelease();
    }
}
