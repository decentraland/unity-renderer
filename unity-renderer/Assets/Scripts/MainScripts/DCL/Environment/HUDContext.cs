using DCL.Controllers;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Context related to all the systems involved in the execution of decentraland scenes.
    /// </summary>
    [System.Obsolete("This is kept for retrocompatibilty and will be removed in the future. Use Environment.i.serviceLocator instead.")]
    public class HUDContext
    {
        private ServiceLocator serviceLocator;
        public IHUDFactory factory => serviceLocator.Get<IHUDFactory>();
        public IHUDController controller => serviceLocator.Get<IHUDController>();

        public HUDContext (ServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }
    }
}