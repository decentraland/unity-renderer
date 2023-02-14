using DCL.Providers;

namespace DCL
{
    public static class ServiceLocatorDesktopFactory
    {
        public static ServiceLocator CreateDefault()
        {
            var result = ServiceLocatorFactory.CreateDefault();

            // Platform
            result.Register<IMemoryManager>(() => new MemoryManagerDesktop());

            // HUD
            result.Register<IHUDFactory>(() => new HUDDesktopFactory());

            return result;
        }
    }
}
