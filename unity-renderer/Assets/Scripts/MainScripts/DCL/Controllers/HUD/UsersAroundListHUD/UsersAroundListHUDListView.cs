using System;
using System.Collections.Generic;
using DCL.SettingsCommon;
using UnityEngine;
using UnityEngine.UI;

internal class UsersAroundListHUDListView : MonoBehaviour, IUsersAroundListHUDListView
{
    public event Action<string, bool> OnRequestMuteUser;
    public event Action<bool> OnRequestMuteGlobal;
    public event Action OnGoToCrowdPressed;
    public event Action OnOpen;

    [SerializeField] private UsersAroundListHUDListElementView listElementView;
    [SerializeField] private ShowHideAnimator showHideAnimator;
    [SerializeField] internal TMPro.TextMeshProUGUI textPlayersTitle;
    [SerializeField] internal Transform contentPlayers;
    [SerializeField] internal Toggle muteAllToggle;
    [SerializeField] internal UserContextMenu contextMenu;
    [SerializeField] internal UserContextConfirmationDialog confirmationDialog;
    [SerializeField] internal GameObject emptyListGameObject;
    [SerializeField] internal Button gotoCrowdButton;
    [SerializeField] internal SpinBoxPresetted voiceChatSpinBox;

    internal Queue<UsersAroundListHUDListElementView> availableElements;
    internal Dictionary<string, UsersAroundListHUDListElementView> userElementDictionary;

    private string playersTextPattern;
    private bool isGameObjectDestroyed = false;

    private void Awake()
    {
        availableElements = new Queue<UsersAroundListHUDListElementView>();
        userElementDictionary = new Dictionary<string, UsersAroundListHUDListElementView>();

        playersTextPattern = textPlayersTitle.text;
        textPlayersTitle.text = string.Format(playersTextPattern, userElementDictionary?.Count ?? 0);

        muteAllToggle.onValueChanged.AddListener(OnMuteGlobal);
        gotoCrowdButton.onClick.AddListener(() => OnGoToCrowdPressed?.Invoke());
        voiceChatSpinBox.onValueChanged.AddListener(OnVoiceChatSettingChanged);

        listElementView.OnMuteUser += OnMuteUser;
        listElementView.OnShowUserContexMenu += OnUserContextMenu;
        listElementView.OnPoolRelease();
        availableElements.Enqueue(listElementView);

        Settings.i.generalSettings.OnChanged += OnSettingsChanged;
    }

    void OnDestroy()
    {
        isGameObjectDestroyed = true;
        Settings.i.generalSettings.OnChanged -= OnSettingsChanged;
    }

    void IUsersAroundListHUDListView.AddOrUpdatePlayer(Player player)
    {
        if (userElementDictionary.ContainsKey(player.id))
        {
            return;
        }

        var profile = UserProfileController.userProfilesCatalog.Get(player.id);

        if (profile == null)
            return;

        UsersAroundListHUDListElementView view = null;
        if (availableElements.Count > 0)
        {
            view = availableElements.Dequeue();
        }
        else
        {
            view = Instantiate(listElementView, contentPlayers);
            view.OnMuteUser += OnMuteUser;
            view.OnShowUserContexMenu += OnUserContextMenu;
        }

        view.OnPoolGet();
        view.SetUserProfile(profile);
        userElementDictionary.Add(player.id, view);
        OnModifyListCount();
        CheckListEmptyState();
    }

    void IUsersAroundListHUDListView.RemoveUser(string userId)
    {
        if (!userElementDictionary.TryGetValue(userId, out UsersAroundListHUDListElementView elementView))
        {
            return;
        }
        if (!elementView)
        {
            return;
        }

        PoolElementView(elementView);
        userElementDictionary.Remove(userId);
        OnModifyListCount();
        CheckListEmptyState();
    }

    void IUsersAroundListHUDListView.SetUserRecording(string userId, bool isRecording)
    {
        if (!userElementDictionary.TryGetValue(userId, out UsersAroundListHUDListElementView elementView))
        {
            return;
        }
        elementView.SetRecording(isRecording);
    }

    void IUsersAroundListHUDListView.SetUserMuted(string userId, bool isMuted)
    {
        if (!userElementDictionary.TryGetValue(userId, out UsersAroundListHUDListElementView elementView))
        {
            return;
        }
        elementView.SetMuted(isMuted);
    }

    void IUsersAroundListHUDListView.SetUserBlocked(string userId, bool blocked)
    {
        if (!userElementDictionary.TryGetValue(userId, out UsersAroundListHUDListElementView elementView))
        {
            return;
        }
        elementView.SetBlocked(blocked);
    }

    void IUsersAroundListHUDListView.SetVisibility(bool visible)
    {
        if (visible)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            showHideAnimator.Show();
            CheckListEmptyState();
            OnOpen?.Invoke();
        }
        else
        {
            showHideAnimator.Hide();
            contextMenu.Hide();
            confirmationDialog.Hide();
        }
    }

    void IUsersAroundListHUDListView.Dispose()
    {
        userElementDictionary.Clear();
        availableElements.Clear();

        if (!isGameObjectDestroyed)
        {
            Destroy(gameObject);
        }
    }

    void OnMuteUser(string userId, bool mute) { OnRequestMuteUser?.Invoke(userId, mute); }

    void OnMuteGlobal(bool mute) { OnRequestMuteGlobal?.Invoke(mute); }

    void OnUserContextMenu(Vector3 position, string userId)
    {
        contextMenu.transform.position = position;
        contextMenu.Show(userId);
    }

    void PoolElementView(UsersAroundListHUDListElementView element)
    {
        element.OnPoolRelease();
        availableElements.Enqueue(element);
    }

    void OnModifyListCount() { textPlayersTitle.text = string.Format(playersTextPattern, userElementDictionary.Count); }

    void CheckListEmptyState()
    {
        bool isEmpty = userElementDictionary.Count == 0;
        emptyListGameObject.SetActive(isEmpty);
    }

    void OnVoiceChatSettingChanged(int index)
    {
        var newSettings = Settings.i.generalSettings.Data;
        newSettings.voiceChatAllow = (GeneralSettings.VoiceChatAllow)index;
        Settings.i.generalSettings.Apply(newSettings);
    }

    void OnSettingsChanged(GeneralSettings settings) { voiceChatSpinBox.SetValue((int)settings.voiceChatAllow, false); }
}