using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INewProjectFlowView
{
    event Action<string, string> OnTittleAndDescriptionSet;
    event Action<int, int> OnSizeSet;
    void ShowNewProjectTitleAndDescrition();
    void Dispose();
}

public class NewProjectFlowView : MonoBehaviour, INewProjectFlowView
{
    public event Action<string, string> OnTittleAndDescriptionSet;
    public event Action<int, int> OnSizeSet;

    [SerializeField] private FirstStep firstStep;
    [SerializeField] private SecondStep secondStep;

    [SerializeField] private ModalComponentView modal;
    [SerializeField] private CarouselComponentView carrousel;

    private int currentStep = 0;

    private void Awake()
    {
        firstStep.OnBackPressed += BackPressed;
        secondStep.OnBackPressed += BackPressed;

        firstStep.OnNextPressed += SetTittleAndDescription;
        secondStep.OnNextPressed += SetSize;
    }

    public void ShowNewProjectTitleAndDescrition() { modal.Show(); }

    public void Dispose()
    {
        firstStep.OnBackPressed += BackPressed;
        secondStep.OnBackPressed += BackPressed;

        firstStep.OnNextPressed -= SetTittleAndDescription;
        secondStep.OnNextPressed -= SetSize;
    }

    private void SetSize(int rows, int colums)
    {
        OnSizeSet?.Invoke(rows, colums);
        NextPressed();
    }

    private void SetTittleAndDescription(string title, string description)
    {
        OnTittleAndDescriptionSet?.Invoke(title, description);
        NextPressed();
    }

    private void NextPressed()
    {
        if (currentStep >= 2)
            return;

        currentStep++;
        carrousel.GoToNextItem();
    }

    private void BackPressed()
    {
        if (currentStep == 0)
            modal.Hide();
        else
        {
            currentStep--;
            carrousel.GoToPreviousItem();
        }
    }
}