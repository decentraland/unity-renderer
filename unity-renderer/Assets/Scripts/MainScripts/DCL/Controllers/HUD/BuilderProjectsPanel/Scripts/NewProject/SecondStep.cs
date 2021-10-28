using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SecondStep : BaseComponentView
{
    private const int MAX_PARCELS = 32;
    private const int PARCEL_METERS_SIZE = 16;

    private const string MAX_PARCELS_TEXT = " parcels";

    public event Action OnBackPressed;
    public event Action<int, int> OnNextPressed;

    [SerializeField] private Color normalTextColor;
    [SerializeField] private Color errorTextColor;

    [SerializeField] private LimitInputField rowsInputField;
    [SerializeField] private LimitInputField columsInputField;
    [SerializeField] private TextMeshProUGUI parcelText;
    [SerializeField] private GameObject errorGameObject;
    [SerializeField] private GameObject gridGameObject;

    [SerializeField] private ButtonComponentView nextButton;
    [SerializeField] private ButtonComponentView backButton;

    private int rows = 2;
    private int colums = 2;

    public override void PostInitialization()
    {
        rowsInputField.OnInputChange += RowsChanged;
        columsInputField.OnInputChange += ColumnsChanged;

        backButton.OnFullyInitialized += () => backButton.onClick.AddListener(BackPressed);
        nextButton.OnFullyInitialized += () => nextButton.onClick.AddListener(NextPressed);
    }

    public override void Dispose()
    {
        base.Dispose();
        rowsInputField.OnInputChange -= RowsChanged;
        columsInputField.OnInputChange -= ColumnsChanged;
    }

    public override void RefreshControl() {  }

    private void RowsChanged(string value)
    {
        rows = Int32.Parse(value);
        ValueChanged();
    }

    private void ColumnsChanged(string value)
    {
        colums = Int32.Parse(value);
        ValueChanged();
    }

    private void ValueChanged()
    {
        if (rows * colums > MAX_PARCELS)
        {
            ShowError();
        }
        else
        {
            ShowGrid();
        }
    }

    private void ShowError()
    {
        errorGameObject.SetActive(true);
        gridGameObject.SetActive(false);

        parcelText.color = errorTextColor;

        parcelText.text = (rows * colums) + MAX_PARCELS_TEXT;
    }

    private void ShowGrid()
    {
        errorGameObject.SetActive(false);
        gridGameObject.SetActive(true);

        parcelText.color = normalTextColor;

        parcelText.text = $"{(rows * colums)} parcels = {(rows * PARCEL_METERS_SIZE)}x{colums * PARCEL_METERS_SIZE}m";
    }

    private void NextPressed()
    {
        if (!nextButton.IsInteractable())
            return;

        // string title= titleInputField.GetValue();
        // string description = descriptionInputField.GetValue();
        //
        // OnNextPressed?.Invoke(title,description);
    }

    private void BackPressed() { OnBackPressed?.Invoke(); }
}