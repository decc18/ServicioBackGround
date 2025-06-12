using NLog;

namespace ServicioBackground.Logging
{
    public interface INLogLogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message, Exception ex = null);
        void Debug(string message);
    }

    public class NLogLogger : INLogLogger
    {
        private static readonly NLog.ILogger logger = LogManager.GetCurrentClassLogger();

        public void Info(string message)
        {
            logger.Info(message);
        }

        public void Warn(string message)
        {
            logger.Warn(message);
        }

        public void Error(string message, Exception ex)
        {
            logger.Error(ex, message);
        }

        public void Debug(string message)
        {
            logger.Debug(message);
        }
    }
}
