using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleSpriteSwap : MonoBehaviour
{
    public Image targetImage;
    public Sprite spriteOn;
    public Sprite spriteOff;
    private Toggle targetToggle;

    private void Awake()
    {
        targetToggle = GetComponent<Toggle>();
    }

    protected void Start()
    {
        SetSprite(targetToggle.isOn);
    }

    private void OnEnable()
    {
        targetToggle.onValueChanged.AddListener(SetSprite);
    }

    private void OnDisable()
    {
        targetToggle.onValueChanged.RemoveListener(SetSprite);
    }

    private void SetSprite(bool newEnabled)
    {
        if (targetImage != null)
            targetImage.sprite = newEnabled ? spriteOn : spriteOff;
    }
}