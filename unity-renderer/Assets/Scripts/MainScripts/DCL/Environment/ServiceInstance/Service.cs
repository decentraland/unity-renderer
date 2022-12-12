namespace DCL
{
    /// <summary>
    /// Caches an instance of `T` upon the first retrieval
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Service<T> where T: class, IService
    {
        private T @ref;

        public T Ref => @ref ??= Environment.i.serviceLocator.Get<T>();

        public static implicit operator T(Service<T> service) =>
            service.Ref;
    }
}
