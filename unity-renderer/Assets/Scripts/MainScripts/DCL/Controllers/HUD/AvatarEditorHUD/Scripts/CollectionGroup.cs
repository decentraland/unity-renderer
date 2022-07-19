using TMPro;
using UnityEngine;

public class CollectionGroup : MonoBehaviour
{
    public TMP_Text collectionName;
    public Transform itemContainer;
    [SerializeField] private ItemToggle[] items;

    public string collectionId { get; private set; }

    public void Configure(string collectionId, string collectionName)
    {
        this.collectionId = collectionId;
        this.collectionName.text = $"{collectionName} collection";
    }
    public ItemToggle LoadItem(int index, WearableSettings wearableSettings, string collection)
    {
        var item = wearableSettings.Item;
        var newToggle = items[index];

        if (item.IsCollectible())
        {
            newToggle.transform.SetAsFirstSibling();
        }

        newToggle.Initialize(item, false, wearableSettings.Amount);
        newToggle.SetHideOtherWerablesToastStrategy(wearableSettings.HideOtherWearablesToastStrategy);
        newToggle.SetReplaceOtherWearablesToastStrategy(wearableSettings.ReplaceOtherWearablesToastStrategy);

        /*bool active = string.IsNullOrEmpty(currentBodyShape) || item.SupportsBodyShape(currentBodyShape);
        newToggle.gameObject.SetActive(active);*/

        newToggle.collectionId = collection;

        return newToggle;
    }
    public void HideItem(int i)
    {
        var newToggle = items[i];
        newToggle.Hide();
    }
}