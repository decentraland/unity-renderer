using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldChatWindowHUDView : MonoBehaviour, IPointerClickHandler
{
    const string VIEW_PATH = "World Chat Window";
    static int ANIM_PROPERTY_SELECTED = Animator.StringToHash("Selected");

    public Button worldFilterButton;
    public Button pmFilterButton;
    public Button closeButton;

    public ChatHUDView chatHudView;

    public CanvasGroup group;
    public WorldChatWindowHUDController controller;

    TabMode tabMode = TabMode.WORLD;
    enum TabMode
    {
        WORLD,
        PRIVATE
    }

    public static WorldChatWindowHUDView Create(UnityAction onPrivateMessages, UnityAction onWorldMessages)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<WorldChatWindowHUDView>();
        view.Initialize(onPrivateMessages, onWorldMessages);
        return view;
    }

    void OnEnable()
    {
        UpdateTabAnimators();
    }

    void UpdateTabAnimators()
    {
        switch (tabMode)
        {
            case TabMode.WORLD:
                pmFilterButton.animator.SetBool(ANIM_PROPERTY_SELECTED, false);
                worldFilterButton.animator.SetBool(ANIM_PROPERTY_SELECTED, true);
                break;
            case TabMode.PRIVATE:
                pmFilterButton.animator.SetBool(ANIM_PROPERTY_SELECTED, true);
                worldFilterButton.animator.SetBool(ANIM_PROPERTY_SELECTED, false);
                break;
        }
    }

    private void Initialize(UnityAction onPrivateMessages, UnityAction onWorldMessages)
    {
        this.closeButton.onClick.AddListener(Toggle);
        this.pmFilterButton.onClick.AddListener(() =>
           {
               onPrivateMessages.Invoke();
               tabMode = TabMode.PRIVATE;
               UpdateTabAnimators();
           });

        this.worldFilterButton.onClick.AddListener(() =>
           {
               onWorldMessages.Invoke();
               tabMode = TabMode.WORLD;
               UpdateTabAnimators();
           });
    }

    public bool isInPreview { get; private set; }

    public void DeactivatePreview()
    {
        group.alpha = 1;
        isInPreview = false;
    }

    public void ActivatePreview()
    {
        group.alpha = 0;
        isInPreview = true;
    }

    public void Toggle()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        else
        {
            gameObject.SetActive(true);
            DeactivatePreview();
            chatHudView.ForceUpdateLayout();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DeactivatePreview();
    }
}
