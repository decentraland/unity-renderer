using System;
using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class UserElementView : MonoBehaviour, ISearchable, ISortable<UserElementView>, IPointerEnterHandler, IPointerExitHandler, IDisposable
{
    enum KeywordIndex
    {
        USER_ID = 0,
        USER_NAME
    }
        
    public event Action<string> OnAddPressed; 
    public event Action<string> OnRemovePressed; 
    
    [SerializeField] internal bool alwaysAsHighlighted;
    [SerializeField] internal RawImage userThumbnail;
    [SerializeField] internal TextMeshProUGUI textUserName;
    [SerializeField] internal Button addButton;
    [SerializeField] internal Button removeButton;
    [SerializeField] internal GameObject buttonsContainer;
    [SerializeField] internal Image highLight;
    [SerializeField] internal Image blocked;

    string[] ISearchable.keywords => searchKeywords;
    private readonly string[] searchKeywords = new string[] { null, null };

    private string userId;
    private bool isDestroyed = false;
    private UserProfile profile;
    private AssetPromise_Texture thumbnailPromise = null;

    private void Awake()
    {
        addButton.onClick.AddListener(()=> OnAddPressed?.Invoke(userId));
        removeButton.onClick.AddListener(()=> OnRemovePressed?.Invoke(userId));
        SetAlwaysHighlighted(alwaysAsHighlighted);
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        if (profile != null)
        {
            profile.OnFaceSnapshotReadyEvent -= SetThumbnail;
        }
    }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            ClearThumbnail();
            Destroy(gameObject);
        }
    }

    public void SetUserProfile(UserProfile userProfile)
    {
        if (profile != null)
        {
            profile.OnFaceSnapshotReadyEvent -= SetThumbnail;
        }

        profile = userProfile;

        SetUserId(profile.userId);
        SetUserName(profile.userName);

        if (profile.faceSnapshot != null)
        {
            SetThumbnail(profile.faceSnapshot);
        }
        else
        {
            profile.OnFaceSnapshotReadyEvent += SetThumbnail;
        }
    }

    public void SetUserProfile(UserProfileModel profileModel)
    {
        profile = null;
        
        var prevThumbnailPromise = thumbnailPromise;
        if (profileModel.snapshots?.face256 != null)
        {
            thumbnailPromise = new AssetPromise_Texture(profileModel.snapshots.face256);
            thumbnailPromise.OnSuccessEvent += (asset => SetThumbnail(asset.texture));
            thumbnailPromise.OnFailEvent += (asset => ClearThumbnail());
            AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
        }
        AssetPromiseKeeper_Texture.i.Forget(prevThumbnailPromise);
        
        SetUserId(profileModel.userId);
        SetUserName(profileModel.name);
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
    }

    public Transform GetParent()
    {
        return transform.parent;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetOrder(int order)
    {
        transform.SetSiblingIndex(order);
    }

    public void SetUserId(string userId)
    {
        this.userId = userId;
        searchKeywords[(int)KeywordIndex.USER_ID] = userId;
    }
    
    public void SetUserName(string userName)
    {
        textUserName.text = userName;
        searchKeywords[(int)KeywordIndex.USER_NAME] = userName;
    }

    public void SetThumbnail(Texture thumbnail)
    {
        userThumbnail.texture = thumbnail;
    }

    public void SetBlocked(bool isBlocked)
    {
        blocked.gameObject.SetActive(isBlocked);
    }

    public void SetIsAdded(bool isAdded)
    {
        addButton.gameObject.SetActive(!isAdded);
        removeButton.gameObject.SetActive(isAdded);
    }

    public void SetAlwaysHighlighted(bool highlighted)
    {
        alwaysAsHighlighted = highlighted;
        if (highlighted)
        {
            SetHighlighted();
        }
        else
        {
            SetNormal();
        }
    }

    public void ClearThumbnail()
    {
        if (profile != null)
        {
            profile.OnFaceSnapshotReadyEvent -= SetThumbnail;
        }
        if (thumbnailPromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            thumbnailPromise = null;
        }
        userThumbnail.texture = null;
    }

    private void SetHighlighted()
    {
        highLight.gameObject.SetActive(true);
        buttonsContainer.SetActive(true);
    }

    private void SetNormal()
    {
        highLight.gameObject.SetActive(false);
        buttonsContainer.SetActive(false);
    }

    int ISortable<UserElementView>.Compare(string sortType, bool isDescendingOrder, UserElementView other)
    {
        //NOTE: for now we only sort by name
        return String.CompareOrdinal(searchKeywords[(int)KeywordIndex.USER_NAME], 
            other.searchKeywords[(int)KeywordIndex.USER_NAME]);
    }
    
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (alwaysAsHighlighted)
            return;

        SetHighlighted();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (alwaysAsHighlighted)
            return;

        SetNormal();
    }
}
