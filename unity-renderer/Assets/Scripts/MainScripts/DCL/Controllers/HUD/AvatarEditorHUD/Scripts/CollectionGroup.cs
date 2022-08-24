using TMPro;
using UnityEngine;

public class CollectionGroup : MonoBehaviour
{
    public TMP_Text collectionName;
    public Transform itemContainer;

    public string collectionId { get; private set; }

    public void Configure(string collectionId, string collectionName)
    {
        this.collectionId = collectionId;
        this.collectionName.text = $"{collectionName} collection";
    }
}
