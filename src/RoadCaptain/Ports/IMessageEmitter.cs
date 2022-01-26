namespace RoadCaptain.Ports
{
    public interface IMessageEmitter
    {
        void Emit(object message);
    }
}