using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IBIWSearchBarView
{
    TMP_InputField searchInput { get; }
    Button smartItemBtn { get; }
    void SetSmartItemPressStatus(bool isPressed);
    void SetEmptyFilter();
}

public class BIWSearchBarView : MonoBehaviour , IBIWSearchBarView
{
    public TMP_InputField searchInput => searchInputField;
    public Button smartItemBtn => smartItemButton;

    public Color smartItemPressedBtnColor;
    public Color smartItemNormalBtnColor;

    [Header("Prefab References")]
    [SerializeField] internal TMP_InputField searchInputField;
    [SerializeField] internal Button smartItemButton;

    public void SetSmartItemPressStatus(bool isPressed) { smartItemBtn.image.color = isPressed ? smartItemPressedBtnColor : smartItemNormalBtnColor; }

    public void SetEmptyFilter() { searchInputField.text = ""; }
}