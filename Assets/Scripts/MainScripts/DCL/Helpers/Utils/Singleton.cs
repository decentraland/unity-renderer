namespace DCL
{
    public class Singleton<T> where T : class, new()
    {
        private static T instance = null;

        public static T i
        {
            get
            {
                if (instance == null)
                    instance = new T();

                return instance;
            }
        }
    }
}
