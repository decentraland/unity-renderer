using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IEntityInformationView
{
    DCLBuilderInWorldEntity currentEntity { get; set; }
    AttributeXYZ position { get; }
    AttributeXYZ rotation { get; }
    AttributeXYZ scale { get; }
    SmartItemListView smartItemList { get; }

    event Action OnDisable;
    event Action OnEndChangingName;
    event Action<DCLBuilderInWorldEntity, string> OnNameChange;
    event Action OnStartChangingName;
    event Action<DCLBuilderInWorldEntity> OnUpdateInfo;

    void ChangeEntityName(string newName);
    void Disable();
    void EndChangingName();
    void SeEntityLimitsText(string tris, string mats, string textures);
    void SetActive(bool isActive);
    void SetCurrentEntity(DCLBuilderInWorldEntity entity);
    void SetEntityThumbnailEnable(bool isEnable);
    void SetEntityThumbnailTexture(Texture2D texture);
    void SetNameIFText(string text);
    void SetPositionAttribute(Vector3 newPos);
    void SetRotationAttribute(Vector3 newRotation);
    void SetScaleAttribute(Vector3 newScale);
    void SetSmartItemListViewActive(bool isActive);
    void StartChangingName();
    void ToggleBasicInfo();
    void ToggleDetailsInfo();
    void UpdateEntitiesSelection(int numberOfSelectedEntities);
}

public class EntityInformationView : MonoBehaviour, IEntityInformationView
{
    [Header("Sprites")]
    [SerializeField] internal Sprite openMenuSprite;
    [SerializeField] internal Sprite closeMenuSprite;

    [Header("Prefab references")]
    [SerializeField] internal GameObject individualEntityPanel;
    [SerializeField] internal GameObject multipleEntitiesPanel;
    [SerializeField] internal TextMeshProUGUI multipleEntitiesText;
    [SerializeField] internal TextMeshProUGUI entityLimitsTrisTxt;
    [SerializeField] internal TextMeshProUGUI entityLimitsMaterialsTxt;
    [SerializeField] internal TextMeshProUGUI entityLimitsTextureTxt;
    [SerializeField] internal TMP_InputField nameIF;
    [SerializeField] internal RawImage entitytTumbailImg;
    [SerializeField] internal AttributeXYZ positionAttribute;
    [SerializeField] internal AttributeXYZ rotationAttribute;
    [SerializeField] internal AttributeXYZ scaleAttribute;
    [SerializeField] internal GameObject detailsGO;
    [SerializeField] internal GameObject basicsGO;
    [SerializeField] internal Image detailsToggleBtn;
    [SerializeField] internal Image basicToggleBtn;
    [SerializeField] internal SmartItemListView smartItemListView;
    [SerializeField] internal Button backButton;
    [SerializeField] internal Button detailsToggleButton;
    [SerializeField] internal Button basicInfoTogglekButton;

    public DCLBuilderInWorldEntity currentEntity { get; set; }
    public AttributeXYZ position => positionAttribute;
    public AttributeXYZ rotation => rotationAttribute;
    public AttributeXYZ scale => scaleAttribute;
    public SmartItemListView smartItemList => smartItemListView;

    public event Action<DCLBuilderInWorldEntity, string> OnNameChange;
    public event Action<DCLBuilderInWorldEntity> OnUpdateInfo;
    public event Action OnStartChangingName;
    public event Action OnEndChangingName;
    public event Action OnDisable;

    internal const int FRAMES_BETWEEN_UPDATES = 5;
    private const string VIEW_PATH = "Common/EntityInformationView";

    internal static EntityInformationView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<EntityInformationView>();
        view.gameObject.name = "_EntityInformationView";

        return view;
    }

    private void Awake()
    {
        backButton.onClick.AddListener(Disable);
        detailsToggleButton.onClick.AddListener(ToggleDetailsInfo);
        basicInfoTogglekButton.onClick.AddListener(ToggleBasicInfo);
        nameIF.onEndEdit.AddListener((newName) => ChangeEntityName(newName));
        nameIF.onSelect.AddListener((newName) => StartChangingName());
        nameIF.onDeselect.AddListener((newName) => EndChangingName());
    }

    private void OnDestroy()
    {
        backButton.onClick.RemoveListener(Disable);
        detailsToggleButton.onClick.RemoveListener(ToggleDetailsInfo);
        basicInfoTogglekButton.onClick.RemoveListener(ToggleBasicInfo);
        nameIF.onEndEdit.RemoveListener((newName) => ChangeEntityName(newName));
        nameIF.onSelect.RemoveListener((newName) => StartChangingName());
        nameIF.onDeselect.RemoveListener((newName) => EndChangingName());
    }

    private void LateUpdate()
    {
        if (currentEntity == null)
            return;

        if (Time.frameCount % FRAMES_BETWEEN_UPDATES == 0)
            OnUpdateInfo?.Invoke(currentEntity);
    }

    public void SetCurrentEntity(DCLBuilderInWorldEntity entity) { currentEntity = entity; }

    public void ToggleDetailsInfo()
    {
        detailsGO.SetActive(!detailsGO.activeSelf);
        detailsToggleBtn.sprite = detailsGO.activeSelf ? openMenuSprite : closeMenuSprite;
    }

    public void ToggleBasicInfo()
    {
        basicsGO.SetActive(!basicsGO.activeSelf);
        basicToggleBtn.sprite = basicsGO.activeSelf ? openMenuSprite : closeMenuSprite;
    }

    public void StartChangingName() { OnStartChangingName?.Invoke(); }

    public void EndChangingName() { OnEndChangingName?.Invoke(); }

    public void ChangeEntityName(string newName) { OnNameChange?.Invoke(currentEntity, newName); }

    public void Disable() { OnDisable?.Invoke(); }

    public void SetEntityThumbnailEnable(bool isEnable)
    {
        if (entitytTumbailImg == null)
            return;

        entitytTumbailImg.enabled = isEnable;
    }

    public void SetEntityThumbnailTexture(Texture2D texture)
    {
        if (entitytTumbailImg == null)
            return;

        entitytTumbailImg.texture = texture;
    }

    public void SeEntityLimitsText(string tris, string mats, string textures)
    {
        entityLimitsTrisTxt.text = tris;
        entityLimitsMaterialsTxt.text = mats;
        entityLimitsTextureTxt.text = textures;
    }

    public void SetSmartItemListViewActive(bool isActive) { smartItemListView.gameObject.SetActive(isActive); }

    public void SetNameIFText(string text) { nameIF.SetTextWithoutNotify(text); }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    public void SetPositionAttribute(Vector3 newPos) { positionAttribute.SetValues(newPos); }

    public void SetRotationAttribute(Vector3 newRotation) { rotationAttribute.SetValues(newRotation); }

    public void SetScaleAttribute(Vector3 newScale) { scaleAttribute.SetValues(newScale); }

    public void UpdateEntitiesSelection(int numberOfSelectedEntities)
    {
        if (numberOfSelectedEntities > 1)
        {
            individualEntityPanel.SetActive(false);
            multipleEntitiesPanel.SetActive(true);
            multipleEntitiesText.text = $"{numberOfSelectedEntities} entities selected";
        }
        else
        {
            individualEntityPanel.SetActive(true);
            multipleEntitiesPanel.SetActive(false);
        }
    }
}