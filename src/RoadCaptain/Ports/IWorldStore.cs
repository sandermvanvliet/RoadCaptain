namespace RoadCaptain.Ports
{
    public interface IWorldStore
    {
        World[] LoadWorlds();
        World LoadWorldById(string id);
    }
}