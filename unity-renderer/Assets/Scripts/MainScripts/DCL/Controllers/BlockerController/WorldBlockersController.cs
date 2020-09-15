using UnityEngine;

namespace DCL.Controllers
{
    public class WorldBlockersController
    {
        public bool enabled = true;

        Transform blockersParent;
        
        readonly ISceneHandler sceneHandler;
        readonly IBlockerHandler blockerHandler;
        readonly DCLCharacterPosition characterPosition;

        public WorldBlockersController(ISceneHandler sceneHandler, IBlockerHandler blockerHandler, DCLCharacterPosition characterPosition)
        {
            this.blockerHandler = blockerHandler;
            this.sceneHandler = sceneHandler;
            this.characterPosition = characterPosition;

            blockersParent = new GameObject("WorldBlockers").transform;
            blockersParent.position = Vector3.zero;

            characterPosition.OnPrecisionAdjust += OnWorldReposition;
        }

        public void SetupWorldBlockers()
        {
            if(!enabled) return;

            blockerHandler.SetupGlobalBlockers(sceneHandler.GetAllLoadedScenesCoords(), 100, blockersParent);
        }

        public void SetEnabled(bool targetValue)
        {
            enabled = targetValue;

            if(!enabled)
                blockerHandler.CleanBlockers();
        }

        void OnWorldReposition(DCLCharacterPosition charPos)
        {
            var newPosition = charPos.WorldToUnityPosition(Vector3.zero); // Blockers parent original position
            blockersParent.position = newPosition;
        }

        public void Dispose()
        {
            characterPosition.OnPrecisionAdjust -= OnWorldReposition;

            Object.Destroy(blockersParent.gameObject);
        }
    }
}