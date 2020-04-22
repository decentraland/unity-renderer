using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TaskbarHUDView : MonoBehaviour
{
    const string VIEW_PATH = "Taskbar";

    public RectTransform windowContainer;
    public Button chatButton;
    public Button friendsButton;

    public GameObject chatTooltip;

    internal TaskbarHUDController controller;

    internal static TaskbarHUDView Create(TaskbarHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<TaskbarHUDView>();
        view.Initialize(controller);
        return view;
    }

    public void Initialize(TaskbarHUDController controller)
    {
        this.controller = controller;
        chatTooltip.SetActive(false);
        CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
        CommonScriptableObjects.rendererState.OnChange += RendererState_OnChange;
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
    }

    private void RendererState_OnChange(bool current, bool previous)
    {
        if (current == previous)
            return;

        if (current && !controller.alreadyToggledOnForFirstTime)
        {
            chatTooltip.SetActive(true);
        }
    }

    internal void OnAddChatWindow(UnityAction onToggle)
    {
        chatButton.gameObject.SetActive(true);
        chatButton.onClick.AddListener(onToggle);
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void OnToggleForFirstTime()
    {
        //TODO(Brian): Toggle an animator trigger/bool instead of doing this.
        chatTooltip.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Return))
        {
            controller.OnPressReturn();
        }
    }
}
