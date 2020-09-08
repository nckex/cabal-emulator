using Microsoft.Extensions.Logging;
using HPT.Common;

namespace HPT.Logging
{
    public class CustomLogger<T> : Logger<T>
    {
        public static CustomLogger<T> Instance = Singleton<CustomLogger<T>>.I;

        public CustomLogger() : base(Singleton<CustomLoggerConfigurator>.I.BuildLoggerFactory())
        {

        }
    }
}
