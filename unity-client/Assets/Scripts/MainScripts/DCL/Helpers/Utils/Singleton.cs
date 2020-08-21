namespace DCL
{
    public class Singleton<T> where T : class, new()
    {
        public static readonly T i = new T();

    }
}
