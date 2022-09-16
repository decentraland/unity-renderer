using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ECSInteractionHoverTooltipMonoBehavior : MonoBehaviour, ITooltip
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TextMeshProUGUI _inputText;
    [SerializeField] private GameObject _iconGameObject;
    [SerializeField] private GameObject _inputTextGameObject;

    public void SetText(string text)
    {
        const int MAX_CHARACTERS = 25;
        _text.text = (text.Length <= MAX_CHARACTERS) ? text : text.Substring(0, MAX_CHARACTERS);
    }

    public void SetInputText(string text)
    {
        _inputText.text = text;
        _inputTextGameObject.SetActive(true);
        _iconGameObject.SetActive(false);
    }

    public void SetInputIcon(Sprite sprite)
    {
        _icon.sprite = sprite;
        _inputTextGameObject.SetActive(false);
        _iconGameObject.SetActive(true);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}