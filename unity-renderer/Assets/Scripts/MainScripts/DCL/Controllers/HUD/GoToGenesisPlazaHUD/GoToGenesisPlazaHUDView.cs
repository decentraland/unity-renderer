using UnityEngine;
using UnityEngine.UI;

namespace DCL.GoToGenesisPlazaHUD
{
    public class GoToGenesisPlazaHUDView : MonoBehaviour
    {
        public bool isOpen { get; private set; } = false;

        public event System.Action OnClose;
        public event System.Action OnContinueClick;
        public event System.Action OnCancelClick;

        private const string PATH = "GoToGenesisPlazaHUD";
        private const string VIEW_OBJECT_NAME = "_GoToGenesisPlazaHUD";

        [SerializeField] private ShowHideAnimator goToGenesisPlazaAnimator;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button continueButton;

        private void Initialize()
        {
            gameObject.name = VIEW_OBJECT_NAME;

            closeButton.onClick.AddListener(() => { OnCancelClick?.Invoke(); });
            cancelButton.onClick.AddListener(() => { OnCancelClick?.Invoke(); });
            continueButton.onClick.AddListener(() => { OnContinueClick?.Invoke(); });
        }

        public static GoToGenesisPlazaHUDView Create()
        {
            GoToGenesisPlazaHUDView view = Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<GoToGenesisPlazaHUDView>();
            view.Initialize();
            return view;
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
                goToGenesisPlazaAnimator.Show();
            else
                goToGenesisPlazaAnimator.Hide();

            if (!visible && isOpen)
                OnClose?.Invoke();

            isOpen = visible;
        }
    }
}