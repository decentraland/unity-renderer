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
            rowsInputField.OnInputLostFocus += RowsInputLostFocus;
            
            columsInputField.OnInputChange += ColumnsChanged;
            columsInputField.OnInputLostFocus += ColumnsInputLostFocus;

            backButton.onClick.AddListener(BackPressed);
            nextButton.onClick.AddListener(NextPressed);
            gridView.Configure(gridModel);
        }

        public override void RefreshControl() {  }

        internal void RowsInputLostFocus()
        {
            if (!string.IsNullOrEmpty(rowsInputField.GetValue()))
                return;
            
            rows = 1;
            rowsInputField.SetText(rows.ToString());
            ValueChanged(rowsInputField);
        }   
        
        internal void ColumnsInputLostFocus()
        {
            if (!string.IsNullOrEmpty(columsInputField.GetValue()))
                return;
            
            colums = 1;
            columsInputField.SetText(colums.ToString());
            ValueChanged(columsInputField);
        }

        internal void RowsChanged(string value)
        {
            //We ensure that the minimum size of the row is 1
            if (string.IsNullOrEmpty(value) || value == "0")
            {
                if(rowsInputField.HasFocus())
                    return;
                rows = 1;
                rowsInputField.SetText(rows.ToString());
            }
            else
            {
                rows = Mathf.Abs(Int32.Parse(value));
            }
            ValueChanged(rowsInputField);
        }

        internal void ColumnsChanged(string value)
        {
            //We ensure that the minimum size of the column is 1
            if (string.IsNullOrEmpty(value) || value == "0")
            {
                if(columsInputField.HasFocus())
                    return;
                colums = 1;
                columsInputField.SetText(colums.ToString());
            }
            else
            {
                colums = Mathf.Abs(Int32.Parse(value));
            }
            ValueChanged(columsInputField);
        }

        private void ValueChanged(LimitInputField origin)
        {
            if (rows * colums > MAX_PARCELS)
            {
                ShowError();
                origin.SetError();
            }
            else
            {
                columsInputField.InputAvailable();
                rowsInputField.InputAvailable();
                
                gridModel.constraintCount = rows;
                gridView.SetItems(parcelImagePrefab, rows * colums);
                gridView.Configure(gridModel);
                ShowGrid();
            }
        }

        internal void ShowError()
        {
            nextButton.SetInteractable(false);
            errorGameObject.SetActive(true);
            gridGameObject.SetActive(false);

            parcelText.color = errorTextColor;

            parcelText.text = (rows * colums) + MAX_PARCELS_TEXT;
        }

        internal void ShowGrid()
        {
            nextButton.SetInteractable(true);
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