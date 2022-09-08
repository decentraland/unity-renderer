using TMPro;

public class TMPFontOnToggle : UIToggle
{
    public TextMeshProUGUI targetText;

    public TMP_FontAsset onFont;

    public TMP_FontAsset offFont;

    protected override void OnValueChanged(bool isOn) => targetText.font = isOn ? onFont : offFont;
}