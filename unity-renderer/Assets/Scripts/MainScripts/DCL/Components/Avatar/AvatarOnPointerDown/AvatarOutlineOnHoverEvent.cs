using AvatarSystem;
using DCL.Helpers;
using DCL.Models;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using System;
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
        private OnPointerEvent.Model model;
        private bool isHovered;

        public bool ShouldBeHoveredWhenMouseIsLocked { get; set; } = true;

        public event Action OnPointerEnterReport;
        public event Action OnPointerExitReport;

        public void Initialize(OnPointerEvent.Model model, IDCLEntity entity, IAvatar avatar)
        {
            this.entity = entity;
            this.avatar = avatar;
            this.model = model;

            CommonScriptableObjects.allUIHidden.OnChange += AllUIHiddenChanged;
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
            if (!enabled) return;
            if (isHovered == state) return;

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

        public bool IsAtHoverDistance(float distance)
        {
            bool isCursorLocked = Utils.IsCursorLocked;
            if (!ShouldBeHoveredWhenMouseIsLocked && isCursorLocked) return false;
            return !isCursorLocked || distance <= model.distance;
        }

        public bool IsVisible() =>
            true;

        public void OnPoolRelease()
        {
            avatar = null;
        }

        public void OnPoolGet() { }

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
    }
}
