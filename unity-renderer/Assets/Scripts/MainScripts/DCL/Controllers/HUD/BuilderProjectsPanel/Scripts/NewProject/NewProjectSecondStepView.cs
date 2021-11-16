using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DCL.Builder
{
    public class NewProjectSecondStepView : BaseComponentView
    {
        private const int MAX_PARCELS = 32;

        private const string MAX_PARCELS_TEXT = " parcels";

        public event Action OnBackPressed;
        public event Action<int, int> OnNextPressed;

        [Header("Design variables")]
        [SerializeField] internal Color normalTextColor;
        [SerializeField] internal Color errorTextColor;
        [SerializeField] private GridContainerComponentModel gridModel;
        [SerializeField] private BaseComponentView parcelImagePrefab;

        [Header("References")]
        [SerializeField] private LimitInputField rowsInputField;
        [SerializeField] private LimitInputField columsInputField;
        [SerializeField] internal TextMeshProUGUI parcelText;
        [SerializeField] internal GameObject errorGameObject;
        [SerializeField] internal GameObject gridGameObject;
        [SerializeField] private GridContainerComponentView gridView;

        [SerializeField] internal ButtonComponentView nextButton;
        [SerializeField] private ButtonComponentView backButton;

        internal int rows = 2;
        internal int colums = 2;

        public override void Start()
        {
            base.Start();

            rowsInputField.OnInputChange += RowsChanged;
            columsInputField.OnInputChange += ColumnsChanged;

            backButton.onClick.AddListener(BackPressed);
            nextButton.onClick.AddListener(NextPressed);
            gridView.Configure(gridModel);
        }

        public override void RefreshControl() {  }

        internal void RowsChanged(string value)
        {
            //We ensure that the minimum size of the row is 1
            if (string.IsNullOrEmpty(value) || value == "0")
                rows = 1;
            else
                rows = Mathf.Abs(Int32.Parse(value));
            ValueChanged();
        }

        internal void ColumnsChanged(string value)
        {
            //We ensure that the minimum size of the column is 1
            if (string.IsNullOrEmpty(value) || value == "0")
                colums = 1;
            else
                colums = Mathf.Abs(Int32.Parse(value));
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
                gridModel.constraintCount = rows;
                gridView.SetItems(parcelImagePrefab, rows * colums);
                gridView.Configure(gridModel);
                ShowGrid();
            }
        }

        internal void ShowError()
        {
            errorGameObject.SetActive(true);
            gridGameObject.SetActive(false);

            parcelText.color = errorTextColor;

            parcelText.text = (rows * colums) + MAX_PARCELS_TEXT;
        }

        internal void ShowGrid()
        {
            errorGameObject.SetActive(false);
            gridGameObject.SetActive(true);

            parcelText.color = normalTextColor;

            parcelText.text = $"{(rows * colums)} parcels = {(rows * DCL.Configuration.ParcelSettings.PARCEL_SIZE)}x{colums * DCL.Configuration.ParcelSettings.PARCEL_SIZE}m";
        }

        internal void NextPressed()
        {
            if (!nextButton.IsInteractable())
                return;

            OnNextPressed?.Invoke(rows, colums);
        }

        internal void BackPressed() { OnBackPressed?.Invoke(); }
    }
}