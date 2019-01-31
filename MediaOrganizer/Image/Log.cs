using NLog;

namespace Image
{
    public class Log
    {
        public static Logger Instance { get; private set; }

        static Log()
        {
            Instance = LogManager.GetCurrentClassLogger();
        }
    }
}
