using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : IAvatarsLODController
    {
        private const float LODS_LOCAL_Y_POS = 1.8f;
        private const float LODS_VERTICAL_MOVEMENT = 0.1f;
        private const float LODS_VERTICAL_MOVEMENT_DELAY = 1f;

        private List<IAvatarRenderer> avatarsList = new List<IAvatarRenderer>();
        private float lastLODsVerticalMovementTime = -1;
        private AvatarLODAssetReferences assetReferences;

        public AvatarsLODController()
        {
            KernelConfig.i.EnsureConfigInitialized()
                        .Then(config =>
                        {
                            DataStore.i.avatarsLOD.LODEnabled.Set(config.features.enableAvatarLODs);
                            if (config.features.enableAvatarLODs)
                                assetReferences = Resources.Load<AvatarLODAssetReferences>("AvatarLODs/AvatarLODAssetReferences");
                        });
        }

        public void Update()
        {
            if (!DataStore.i.avatarsLOD.LODEnabled.Get())
                return;

            UpdateAllLODs();

            UpdateLODsVerticalMovementAndBillboard();
        }

        public void RegisterAvatar(IAvatarRenderer newAvatar)
        {
            if (!DataStore.i.avatarsLOD.LODEnabled.Get() || avatarsList.Contains(newAvatar))
                return;

            avatarsList.Add(newAvatar);
        }

        public void RemoveAvatar(IAvatarRenderer targetAvatar)
        {
            if (!DataStore.i.avatarsLOD.LODEnabled.Get() || !avatarsList.Contains(targetAvatar))
                return;

            int listCount = avatarsList.Count;
            for (int i = 0; i < listCount; i++)
            {
                if (avatarsList[i] == targetAvatar)
                {
                    avatarsList.RemoveAt(i);

                    return;
                }
            }
        }

        private void UpdateLODsVerticalMovementAndBillboard()
        {
            int listCount = avatarsList.Count;
            bool applyVerticalMovement = Time.timeSinceLevelLoad - lastLODsVerticalMovementTime > LODS_VERTICAL_MOVEMENT_DELAY;
            GameObject lodGO;
            for (int i = 0; i < listCount; i++)
            {
                lodGO = avatarsList[i].GetLODRenderer().gameObject;
                if (!lodGO.activeSelf)
                    continue;

                if (applyVerticalMovement && Vector3.Distance(lodGO.transform.position, CommonScriptableObjects.playerUnityPosition.Get()) > 2 * DataStore.i.avatarsLOD.LODDistance.Get())
                    lodGO.transform.localPosition = new Vector3(lodGO.transform.localPosition.x, (lodGO.transform.localPosition.y > LODS_LOCAL_Y_POS ? LODS_LOCAL_Y_POS : LODS_LOCAL_Y_POS + LODS_VERTICAL_MOVEMENT), lodGO.transform.localPosition.z);

                Vector3 previousForward = lodGO.transform.forward;
                Vector3 lookAtDir = (lodGO.transform.position - CommonScriptableObjects.cameraPosition).normalized;

                lookAtDir.y = previousForward.y;

                lodGO.transform.forward = lookAtDir;
            }

            if (applyVerticalMovement)
                lastLODsVerticalMovementTime = Time.timeSinceLevelLoad;
        }

        public void Dispose() { }

        private void UpdateAllLODs()
        {
            if (!DataStore.i.avatarsLOD.LODEnabled.Get())
                return;

            SortedList<float, IAvatarRenderer> closeDistanceAvatars = new SortedList<float, IAvatarRenderer>();
            foreach (IAvatarRenderer avatar in avatarsList)
            {
                float distanceToPlayer = Vector3.Distance(CommonScriptableObjects.playerUnityPosition.Get(), avatar.GetTransform().position);
                bool isInLODDistance = distanceToPlayer >= DataStore.i.avatarsLOD.LODDistance.Get();

                if (isInLODDistance)
                    ToggleLOD(avatar, true);
                else
                    closeDistanceAvatars.Add(distanceToPlayer, avatar);
            }

            int closeDistanceAvatarsCount = closeDistanceAvatars.Count;
            IAvatarRenderer currentAvatar;
            for (var i = 0; i < closeDistanceAvatarsCount; i++)
            {
                currentAvatar = closeDistanceAvatars.Values[i];
                bool isLOD = i >= DataStore.i.avatarsLOD.maxNonLODAvatars.Get();

                ToggleLOD(currentAvatar, isLOD);
            }
        }

        private void ToggleLOD(IAvatarRenderer avatarRenderer, bool enabled)
        {
            var lodRenderer = avatarRenderer.GetLODRenderer();
            if (lodRenderer.gameObject.activeSelf == enabled)
                return;

            if (enabled)
            {
                // SpriteAtlas makes the sprite.texture return the whole atlas...
                /*string randomlySelectedSpriteName = Random.Range(0, impostorAtlas.spriteCount).ToString();
                Debug.Log("Setting sprite: " + randomlySelectedSpriteName);
                Texture2D randomTex = impostorAtlas.GetSprite(randomlySelectedSpriteName).texture; // use SharedTexture???
                lodRenderer.material.mainTexture = randomTex;
                lodRenderer.material.SetTexture("_BaseMap", randomTex);*/

                lodRenderer.material.SetTexture("_BaseMap", assetReferences.impostorTextures[Random.Range(0, assetReferences.impostorTextures.Length)]);
                // TODO: To achieve 1 draw call on impostors: optimize having all the textures in a real atlas and randomize discretely the UVs of the instantiated quad, all having the same material and tex: https://docs.unity3d.com/ScriptReference/Mesh-uv.html
            }

            lodRenderer.gameObject.SetActive(enabled);
            avatarRenderer.SetVisibility(!enabled); // TODO: Resolve coping with AvatarModifierArea regarding this toggling (issue #718)
        }
    }
}