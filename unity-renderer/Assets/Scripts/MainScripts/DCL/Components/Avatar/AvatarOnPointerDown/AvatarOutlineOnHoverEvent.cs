using AvatarSystem;
using DCL.Models;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using UnityEngine;

namespace DCL.Components
{
    /// <summary>
    /// Complementary class to detect avatar outlining only,
    /// It does not live on its own and should coexist with `AvatarOnPointerDown`
    /// </summary>
    public class AvatarOutlineOnHoverEvent : MonoBehaviour, IUnlockedCursorInputEvent, IPoolLifecycleHandler
    {
        private IAvatar avatar;

        private bool isHovered;

        public event System.Action OnPointerEnterReport;
        public event System.Action OnPointerExitReport;

        public void Initialize(IDCLEntity entity, IAvatar avatar)
        {
            this.entity = entity;
            this.avatar = avatar;

            CommonScriptableObjects.allUIHidden.OnChange += AllUIHiddenChanged;
        }

        private void AllUIHiddenChanged(bool isAllUIHidden, bool _)
        {
            if (isAllUIHidden)
                ResetAvatarOutlined();
        }

        private void ResetAvatarOutlined()
        {
            DataStore.i.outliner.avatarOutlined.Set((null, -1, -1));
        }

        private void SetAvatarOutlined()
        {
            if (avatar.status == IAvatar.Status.Loaded)
            {
                var renderer = avatar.GetMainRenderer();

                if (renderer != null && !CommonScriptableObjects.allUIHidden.Get())
                    DataStore.i.outliner.avatarOutlined.Set((renderer, renderer.GetComponent<MeshFilter>().sharedMesh.subMeshCount, avatar.extents.y));
            }
        }

        private void OnDestroy()
        {
            CommonScriptableObjects.allUIHidden.OnChange -= AllUIHiddenChanged;
        }

        public Transform GetTransform() =>
            transform;

        public IDCLEntity entity { get; private set; }

        public void SetHoverState(bool state)
        {
            if (isHovered == state)
                return;

            isHovered = state;

            if (isHovered)
            {
                SetAvatarOutlined();
                OnPointerEnterReport?.Invoke();
            }
            else
            {
                ResetAvatarOutlined();
                OnPointerExitReport?.Invoke();
            }
        }

        public bool IsAtHoverDistance(float distance) =>
            true;

        public bool IsVisible() =>
            true;

        public void OnPoolRelease()
        {
            avatar = null;
        }

        public void OnPoolGet() { }
    }
}
