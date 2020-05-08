using UnityEngine;

public class TaskbarHUDController : IHUD
{
    internal TaskbarHUDView view;
    public WorldChatWindowHUDController worldChatWindowHud;
    public FriendsHUDController friendsHud;
    public bool alreadyToggledOnForFirstTime { get; private set; } = false;

    public TaskbarHUDController()
    {
        view = TaskbarHUDView.Create(this);
    }

    public void AddChatWindow(WorldChatWindowHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddChatWindow >>> World Chat Window doesn't exist yet!");
            return;
        }

        if (controller.view.transform.parent == view.windowContainer)
            return;

        controller.view.transform.SetParent(view.windowContainer, false);

        worldChatWindowHud = controller;
        view.OnAddChatWindow(ToggleChatWindow);
    }

    public void AddFriendsWindow(FriendsHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddFriendsWindow >>> Friends window doesn't exist yet!");
            return;
        }

        if (controller.view.transform.parent == view.windowContainer)
            return;

        controller.view.transform.SetParent(view.windowContainer, false);

        friendsHud = controller;
        view.OnAddFriendsWindow(ToggleFriendsWindow);
    }

    private void ToggleChatWindow()
    {
        if (worldChatWindowHud.view.isInPreview)
        {
            worldChatWindowHud.view.DeactivatePreview();
            return;
        }

        worldChatWindowHud.view.Toggle();

        if (worldChatWindowHud.view.gameObject.activeSelf)
            OnToggleOn();
    }

    private void ToggleFriendsWindow()
    {
        friendsHud.view.Toggle();

        if (friendsHud.view.gameObject.activeSelf)
            OnToggleOn();
    }

    public void Dispose()
    {
        if (view != null)
        {
            Object.Destroy(view.gameObject);
        }
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

    public void OnPressReturn()
    {
        if (worldChatWindowHud.OnPressReturn())
        {
            OnToggleOn();
        }
    }

    void OnToggleOn()
    {
        if (alreadyToggledOnForFirstTime)
            return;

        alreadyToggledOnForFirstTime = true;
        view.OnToggleForFirstTime();
    }
}
