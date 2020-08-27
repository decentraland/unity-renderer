using UnityEngine;
using System;
using DCL.Helpers;
using DCL.Interface;

public class ExploreHUDController : IHUD
{
    internal ExploreHUDView view;
    internal InputAction_Trigger toggleExploreTrigger;

    ExploreMiniMapDataController miniMapDataController;
    FriendTrackerController friendsController;

    public event Action OnToggleTriggered;

    public ExploreHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ExploreHUD")).GetComponent<ExploreHUDView>();
        view.name = "_ExploreHUD";
        view.popup.gameObject.SetActive(false);

        toggleExploreTrigger = Resources.Load<InputAction_Trigger>("ToggleExploreHud");
        toggleExploreTrigger.OnTriggered += OnToggleActionTriggered;

        view.closeButton.onPointerDown += () =>
        {
            if (view.IsVisible())
            {
                toggleExploreTrigger.RaiseOnTriggered();
            }
        };

        view.gotoMagicButton.OnGotoMagicPressed += GoToMagic;
        view.togglePopupButton.onPointerDown += () => toggleExploreTrigger.RaiseOnTriggered();
        BaseSceneCellView.OnJumpIn += OnJumpIn;
    }

    public void Initialize(IFriendsController friendsController)
    {
        this.friendsController = new FriendTrackerController(friendsController, view.friendColors);
        miniMapDataController = new ExploreMiniMapDataController();

        view.Initialize(miniMapDataController, this.friendsController);
    }

    public void SetVisibility(bool visible)
    {
        if (view == null)
        {
            return;
        }

        if (visible && !view.IsActive())
        {
            Utils.UnlockCursor();
            view.RefreshData();
        }

        view.SetVisibility(visible);
    }

    public void Dispose()
    {
        miniMapDataController?.Dispose();
        friendsController?.Dispose();

        toggleExploreTrigger.OnTriggered -= OnToggleActionTriggered;
        BaseSceneCellView.OnJumpIn -= OnJumpIn;

        if (view != null)
        {
            view.gotoMagicButton.OnGotoMagicPressed -= GoToMagic;
            GameObject.Destroy(view.gameObject);
        }
    }

    void OnToggleActionTriggered(DCLAction_Trigger action)
    {
        if (view)
        {
            SetVisibility(!view.IsVisible());
            OnToggleTriggered?.Invoke();
        }
    }

    void OnJumpIn(Vector2Int coords, string serverName, string layerName)
    {
        if (view.IsVisible())
        {
            toggleExploreTrigger.RaiseOnTriggered();
        }

        if (string.IsNullOrEmpty(serverName) || string.IsNullOrEmpty(layerName))
        {
            WebInterface.GoTo(coords.x, coords.y);
        }
        else
        {
            WebInterface.JumpIn(coords.x, coords.y, serverName, layerName);
        }
    }

    void GoToMagic()
    {
        if (view.IsVisible())
        {
            toggleExploreTrigger.RaiseOnTriggered();
        }

        WebInterface.GoToMagic();
    }
}
