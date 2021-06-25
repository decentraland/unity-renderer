using System.Collections.Generic;
using DCL.Components;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : MonoBehaviour
    {
        public static AvatarsLODController i { get; private set; }

        private const int MAX_NON_LOD_AVATARS = 20; // this could be in the settings panel
        private const int lodDistance = 16; // this could be in the settings panel
        // TODO: have a little SortedList with as many items as the max non-lod avatars
        private List<AvatarShape> nonLODAvatars = new List<AvatarShape>();

        private List<AvatarShape> avatarsList = new List<AvatarShape>();

        void Awake()
        {
            if (i != null)
            {
                Destroy(gameObject);
                return;
            }

            i = this;

            CommonScriptableObjects.playerUnityPosition.OnChange += OnMainPlayerReposition;
        }

        // This runs on LateUpdate() instead of Update() to be applied AFTER the transform was moved by the transform component
        public void LateUpdate()
        {
            int listCount = avatarsList.Count;

            Transform quadLODTransform;
            for (int i = 0; i < listCount; i++)
            {
                quadLODTransform = avatarsList[i].avatarRenderer.lodQuad.transform;

                Vector3 previousForward = quadLODTransform.forward;
                Vector3 lookAtDir = (quadLODTransform.position - CommonScriptableObjects.cameraPosition).normalized;

                lookAtDir.y = previousForward.y;

                quadLODTransform.forward = lookAtDir;
            }
        }

        void OnMainPlayerReposition(Vector3 newPos, Vector3 previousPos) { UpdateAllLODs(); }

        void UpdateAllLODs()
        {
            foreach (AvatarShape avatar in avatarsList)
            {
                UpdateLOD(avatar);
            }
        }

        void UpdateLOD(AvatarShape avatar)
        {
            bool isInLODDistance = Vector3.Distance(CommonScriptableObjects.playerUnityPosition.Get(), avatar.transform.position) >= lodDistance;

            // If we reached the max non-lod avatars we force this avatar to be a LOD -> TODO: Change this to be based on distance as well
            if (!isInLODDistance && nonLODAvatars.Count == MAX_NON_LOD_AVATARS && !nonLODAvatars.Contains(avatar))
                isInLODDistance = true;

            if (isInLODDistance)
            {
                avatar.avatarRenderer.lodQuad.SetActive(true);
                avatar.avatarRenderer.SetVisibility(false); // TODO: Can this have any issue with the AvatarModifierArea ???

                if (nonLODAvatars.Contains(avatar))
                    nonLODAvatars.Remove(avatar);
            }
            else
            {
                avatar.avatarRenderer.lodQuad.SetActive(false);
                avatar.avatarRenderer.SetVisibility(true);

                if (!nonLODAvatars.Contains(avatar))
                    nonLODAvatars.Add(avatar);
            }
        }

        public void RegisterAvatar(AvatarShape newAvatar)
        {
            if (avatarsList.Contains(newAvatar))
                return;

            avatarsList.Add(newAvatar);

            // TODO: Find a way to get this behaviour but unsubscribing on RemoveAvatar()
            // DecentralandEntity already nullifies this event on cleanup, maybe that's good enough?
            // Worst case scenario we'll have to refactor the DCLEntity.OnTransformChange event to report which entity was
            newAvatar.entity.OnTransformChange += (object newTransformModel) =>
            {
                UpdateLOD(newAvatar);
            };

            UpdateLOD(newAvatar);
        }

        public void RemoveAvatar(AvatarShape targetAvatar)
        {
            if (!avatarsList.Contains(targetAvatar))
                return;

            int listCount = avatarsList.Count;
            for (int i = 0; i < listCount; i++)
            {
                if (avatarsList[i] == targetAvatar)
                {
                    avatarsList.RemoveAt(i);
                    // targetAvatar.entity.OnTransformChange -= OnAvatarTransformChange;

                    // UpdateLODs();

                    return;
                }
            }
        }
    }
}