using UnityEngine;

public class ScreenshotVisiblePersonView : MonoBehaviour
{
    [field: SerializeField] public ProfileCardComponentView ProfileCard { get; private set; }
    [field: SerializeField] public NFTIconComponentView WearableTemplate { get; private set; }

    [field: SerializeField] public Transform WearablesListContainer { get; private set; }
}
