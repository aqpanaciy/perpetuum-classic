namespace Perpetuum.Log.Loggers
{
    public class NullLogger<T> : IPLogger<T> where T:ILogEvent
    {
        public void Log(T logEvent)
        {
        }
    }
}