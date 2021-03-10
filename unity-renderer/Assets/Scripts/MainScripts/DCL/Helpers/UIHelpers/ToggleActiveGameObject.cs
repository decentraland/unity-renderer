using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleActiveGameObject : MonoBehaviour
{
    [SerializeField] GameObject activeOn = null;
    [SerializeField] GameObject activeOff = null;

    private Toggle targetToggle;

    private void Awake()
    {
        targetToggle = GetComponent<Toggle>();
        targetToggle.onValueChanged.AddListener(UpdateState);
    }

    protected void Start()
    {
        UpdateState(targetToggle.isOn);
    }

    private void UpdateState(bool isOn)
    {
        if (activeOn) activeOn.gameObject.SetActive(isOn);
        if (activeOff) activeOff.gameObject.SetActive(!isOn);
    }
}