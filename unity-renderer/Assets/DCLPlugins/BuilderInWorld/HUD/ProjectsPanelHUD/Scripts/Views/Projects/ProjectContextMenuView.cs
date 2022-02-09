using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    public interface IProjectContextMenuView
    {
        /// <summary>
        /// Event released if the duplicate button has been pressed
        /// </summary>
        event Action<IProjectCardView> OnDuplicatePressed;

        /// <summary>
        /// Event released if the delete button has been pressed
        /// </summary>
        event Action<IProjectCardView> OnDeletePressed;

        /// <summary>
        /// Event released if the publish button has been pressed
        /// </summary>
        event Action<IProjectCardView> OnPublishPressed;

        /// <summary>
        /// Shows the context menu at the right of the view 
        /// </summary>
        /// <param name="view"></param>
        void ShowOnCard(IProjectCardView view);

        /// <summary>
        /// Hides the view
        /// </summary>
        void Hide();
    }

    public class ProjectContextMenuView : BaseComponentView, IProjectContextMenuView
    {
        public event Action<IProjectCardView> OnDuplicatePressed;
        public event Action<IProjectCardView> OnDeletePressed;
        public event Action<IProjectCardView> OnPublishPressed;

        [Header("Buttons")]
        [SerializeField] internal Button publishButton;
        [SerializeField] internal Button duplicateButton;
        [SerializeField] internal Button deleteButton;

        // Note: this buttons will be implemented in the future
        [SerializeField] internal Button settingsButton;
        [SerializeField] internal Button downloadButton;
        [SerializeField] internal Button shareButton;

        private IProjectCardView lastViewShowed;

        public override void Awake()
        {
            base.Awake();

            duplicateButton.onClick.AddListener( () =>
            {
                Hide();
                OnDuplicatePressed?.Invoke(lastViewShowed);
            });

            deleteButton.onClick.AddListener( () =>
            {
                Hide();
                OnDeletePressed?.Invoke(lastViewShowed);
            });

            publishButton.onClick.AddListener( () =>
            {
                Hide();
                OnPublishPressed?.Invoke(lastViewShowed);
            });
        }

        public override void Dispose()
        {
            base.Dispose();

            duplicateButton.onClick.RemoveAllListeners();
            deleteButton.onClick.RemoveAllListeners();
            publishButton.onClick.RemoveAllListeners();
        }

        public override void RefreshControl() { }

        public void ShowOnCard(IProjectCardView view)
        {
            lastViewShowed = view;

            transform.position = view.contextMenuButtonPosition;
            gameObject.SetActive(true);
        }

        public void Hide() { gameObject.SetActive(false); }

        private void Update() { HideIfClickedOutside(); }

        private void HideIfClickedOutside()
        {
            if (Input.GetMouseButtonDown(0) &&
                !RectTransformUtility.RectangleContainsScreenPoint((RectTransform) transform, Input.mousePosition))
            {
                Hide();
            }
        }
    }
}