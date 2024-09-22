namespace Perpetuum.Log
{
    public interface IPLogger { }

    public interface IPLogger<in T> : IPLogger where T:ILogEvent
    {
        void Log(T logEvent);
    }
}