using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;

public interface INewProjectFlowView
{
    /// <summary>
    /// This will set the title and description of the new project. Order param: Title - Description
    /// </summary>
    event Action<string, string> OnTittleAndDescriptionSet;

    /// <summary>
    /// Set the size of the new project. Order param: Rows - Columns
    /// </summary>
    event Action<int, int> OnSizeSet;

    /// <summary>
    /// This will show the first step of the new project flow
    /// </summary>
    void ShowNewProjectTitleAndDescrition();

    /// <summary>
    /// This will reset the view to the initial values
    /// </summary>
    void Reset();

    /// <summary>
    /// Hide the view
    /// </summary>
    void Hide();
    
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
        name = "_BuilderNewProjectFlowView";
        firstStep.OnBackPressed += BackPressed;
        secondStep.OnBackPressed += BackPressed;

        firstStep.OnNextPressed += SetTittleAndDescription;
        secondStep.OnNextPressed += SetSize;
    }

    public void ShowNewProjectTitleAndDescrition()
    {
        gameObject.SetActive(true);
        modal.Show();
    }
    
    public void Reset()
    {
        currentStep = 0;
        carrousel.ResetCarousel();
    }
    
    public void Hide()
    {
        modal.Hide();
    }

    public void Dispose()
    {
        firstStep.OnBackPressed += BackPressed;
        secondStep.OnBackPressed += BackPressed;

        firstStep.OnNextPressed -= SetTittleAndDescription;
        secondStep.OnNextPressed -= SetSize;
    }

    private void SetSize(int rows, int colums)
    {
        NextPressed();
        OnSizeSet?.Invoke(rows, colums);
    }

    private void SetTittleAndDescription(string title, string description)
    {
        NextPressed();
        OnTittleAndDescriptionSet?.Invoke(title, description);
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
            Hide();
        else
        {
            currentStep--;
            carrousel.GoToPreviousItem();
        }
    }
}