using DCL.Controllers;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Context related to all the systems involved in the execution of decentraland scenes.
    /// </summary>
    public class HUDContext : System.IDisposable
    {
        public readonly IHUDFactory factory = null;
        public readonly IHUDController controller = null;
        public HUDContext(IHUDFactory hudFactory, IHUDController hudController)
        {
            this.factory = hudFactory;
            this.controller = hudController;
        }

        public void Dispose()
        {
            controller?.Dispose();
        }
    }
}