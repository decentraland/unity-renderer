using System.Collections.Generic;
using DCL;
using DCL.Components.Interfaces;
using Environment = DCL.Environment;

namespace DCLPlugins.UIRefresherPlugin
{
    public class UIRefresherPlugin : IPlugin
    {
        private readonly UIRefresherController controller;
        
        public UIRefresherPlugin()
        {
            controller = new UIRefresherController(Environment.i.platform.updateEventHandler, 
                CommonScriptableObjects.sceneNumber,
                DataStore.i.HUDs.dirtyShapes);
        }
        
        public void Dispose()
        {
            DataStore.i.HUDs.dirtyShapes.Set(new Dictionary<int, Queue<IUIRefreshable>>());
            controller.Dispose();
        }
    }

}