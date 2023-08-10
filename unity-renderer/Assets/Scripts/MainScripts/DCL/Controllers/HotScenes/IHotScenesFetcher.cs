using Cysharp.Threading.Tasks;
using DCL;
using System.Collections.Generic;

namespace MainScripts.DCL.Controllers.HotScenes
{
    public interface IHotScenesFetcher : IService
    {
        public enum UpdateMode
        {
            FOREGROUND,
            BACKGROUND,
            IMMEDIATELY_ONCE,
        }

        IReadOnlyAsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>> ScenesInfo { get; }

        public void SetUpdateMode(UpdateMode mode);
    }
}
