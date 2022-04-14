namespace RoadCaptain.Ports
{
    public interface IVersionChecker
    {
        Release GetLatestRelease();
    }
}