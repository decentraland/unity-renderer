using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : MonoBehaviour
    {
        public static AvatarsLODController i { get; private set; }

        private const int MAX_NON_LOD_AVATARS = 20; // this could be in the settings panel
        // TODO: have a little SortedList with as many items as the max non-lod avatars
        private List<AvatarRenderer> nonLODAvatars = new List<AvatarRenderer>();

        private List<AvatarRenderer> avatarRenderersList = new List<AvatarRenderer>();

        void Awake()
        {
            if (i != null)
            {
                Destroy(gameObject);
                return;
            }

            i = this;

            CommonScriptableObjects.playerUnityPosition.OnChange += OnMainPlayerReposition;
            // TODO: Also bind to own avatar/entity reposition
        }

        // This runs on LateUpdate() instead of Update() to be applied AFTER the transform was moved by the transform component
        public void LateUpdate()
        {
            int listCount = avatarRenderersList.Count;
            for (int i = 0; i < listCount; i++)
            {
                Vector3 previousForward = avatarRenderersList[i].lodQuad.transform.forward;
                Vector3 lookAtDir = (avatarRenderersList[i].lodQuad.transform.position - CommonScriptableObjects.cameraPosition).normalized;

                lookAtDir.y = previousForward.y;

                avatarRenderersList[i].lodQuad.transform.forward = lookAtDir;
            }
        }

        void OnMainPlayerReposition(Vector3 newPos, Vector3 previousPos) { UpdateAllLODs(); }

        void UpdateAllLODs()
        {
            foreach (AvatarRenderer avatarRenderer in avatarRenderersList)
            {
                UpdateLOD(avatarRenderer);
            }
        }

        void UpdateLOD(AvatarRenderer avatarRenderer)
        {
            int lodDistance = 16; // this could be in the settings panel
            bool isInLODDistance = Vector3.Distance(CommonScriptableObjects.playerUnityPosition.Get(), avatarRenderer.transform.position) >= lodDistance;

            // If we reached the max non-lod avatars we force this avatar to be a LOD -> TODO: Change this to be based on distance as well
            if (!isInLODDistance && nonLODAvatars.Count == MAX_NON_LOD_AVATARS && !nonLODAvatars.Contains(avatarRenderer))
                isInLODDistance = true;

            if (isInLODDistance)
            {
                avatarRenderer.lodQuad.SetActive(true);
                avatarRenderer.SetVisibility(false); // TODO: Can this have any issue with the AvatarModifierArea ???

                if (nonLODAvatars.Contains(avatarRenderer))
                    nonLODAvatars.Remove(avatarRenderer);
            }
            else
            {
                avatarRenderer.lodQuad.SetActive(false);
                avatarRenderer.SetVisibility(true);

                if (!nonLODAvatars.Contains(avatarRenderer))
                    nonLODAvatars.Add(avatarRenderer);
            }
        }

        public void RegisterAvatar(AvatarRenderer newAvatarRenderer)
        {
            if (avatarRenderersList.Contains(newAvatarRenderer))
                return;

            avatarRenderersList.Add(newAvatarRenderer);

            UpdateLOD(newAvatarRenderer);
        }

        public void RemoveAvatar(AvatarRenderer targetAvatarRenderer)
        {
            if (!avatarRenderersList.Contains(targetAvatarRenderer))
                return;

            int listCount = avatarRenderersList.Count;
            for (int i = 0; i < listCount; i++)
            {
                if (avatarRenderersList[i] == targetAvatarRenderer)
                {
                    avatarRenderersList.RemoveAt(i);

                    // UpdateLODs();

                    return;
                }
            }
        }
    }
}