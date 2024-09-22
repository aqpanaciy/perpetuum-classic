namespace Perpetuum.Log.Loggers
{
    public class CompositeLogger<T> : IPLogger<T> where T : ILogEvent
    {
        private readonly IPLogger<T>[] _loggers;

        public CompositeLogger(params IPLogger<T>[] loggers)
        {
            _loggers = loggers;
        }

        public void Log(T logEvent)
        {
            foreach (var logger in _loggers)
            {
                logger.Log(logEvent);
            }
        }
    }
}