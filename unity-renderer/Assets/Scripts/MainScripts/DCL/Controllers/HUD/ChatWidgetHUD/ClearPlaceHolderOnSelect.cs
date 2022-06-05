using TMPro;
using UnityEngine;

public class ClearPlaceHolderOnSelect : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    private void Awake()
    {
        inputField.onSelect.AddListener(OnSelected);
        inputField.onDeselect.AddListener(OnDeselected);
    }

    private void OnDestroy()
    {
        if (inputField == null) return;
        inputField.onSelect.RemoveListener(OnSelected);
        inputField.onDeselect.RemoveListener(OnDeselected);
    }

    private void OnSelected(string arg0)
    {
        inputField.placeholder.gameObject.SetActive(false);
    }

    private void OnDeselected(string arg0)
    {
        inputField.placeholder.gameObject.SetActive(true);
    }
}