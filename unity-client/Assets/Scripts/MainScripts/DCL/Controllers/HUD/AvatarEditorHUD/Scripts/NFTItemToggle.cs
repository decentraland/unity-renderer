using UnityEngine;

public class NFTItemToggle : ItemToggle
{
    [SerializeField] internal NFTItemInfo nftItemInfo;
    [SerializeField] private PointerOverDetector infoOver;

    private PointerOverDetector.Enter infoEnterDelegate; 
    private PointerOverDetector.Exit infoExitDelegate; 

    protected override void Awake()
    {
        base.Awake();

        nftItemInfo.SetActive(false);

        infoEnterDelegate = (x) => nftItemInfo.SetActive(true);
        infoOver.OnEnter += infoEnterDelegate;
        infoExitDelegate = (x) => nftItemInfo.SetActive(false);
        infoOver.OnExit += infoExitDelegate;
    }

    public override void Initialize(WearableItem w, bool isSelected, int amount)
    {
        base.Initialize(w, isSelected, amount);
        nftItemInfo.SetModel(NFTItemInfo.Model.FromWearableItem(wearableItem));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        nftItemInfo.CleanUp();
    }
}