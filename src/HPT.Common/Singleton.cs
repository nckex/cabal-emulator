namespace HPT.Common
{
    public sealed class Singleton<T>
        where T : class, new()
    {
        /// <summary>Get an instance from singleton </summary>
        public static readonly T I = new T();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Singleton()
        {
        }

        private Singleton()
        {
        }
    } 
}

