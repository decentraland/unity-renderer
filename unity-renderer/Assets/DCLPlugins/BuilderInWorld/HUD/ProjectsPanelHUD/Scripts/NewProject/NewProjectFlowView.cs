using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using UnityEngine;

public interface INewProjectFlowView
{
    /// <summary>
    /// Called when the view has hide
    /// </summary>
    event Action OnViewHide;
    
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
    
    /// <summary>
    /// This will return true if the new project windows is active
    /// </summary>
    /// <returns></returns>
    bool IsActive();
    
    void Dispose();
}

public class NewProjectFlowView : MonoBehaviour, INewProjectFlowView
{
    public event Action OnViewHide;
    public event Action<string, string> OnTittleAndDescriptionSet;
    public event Action<int, int> OnSizeSet;

    [SerializeField] private NewProjectFirstStepView newProjectFirstStepView;
    [SerializeField] private NewProjectSecondStepView newProjectSecondStepView;

    [SerializeField] private ModalComponentView modal;
    [SerializeField] private CarouselComponentView carrousel;

    internal int currentStep = 0;

    private void Awake()
    {
        name = "_BuilderNewProjectFlowView";
        newProjectFirstStepView.OnBackPressed += BackPressed;
        newProjectSecondStepView.OnBackPressed += BackPressed;

        newProjectFirstStepView.OnNextPressed += SetTittleAndDescription;
        newProjectSecondStepView.OnNextPressed += SetSize;

        modal.OnCloseAction += () => OnViewHide?.Invoke();
    }

    private void OnDestroy()
    {
        Dispose();
    }

    public void ShowNewProjectTitleAndDescrition()
    {
        gameObject.SetActive(true);
        newProjectFirstStepView.ResetInputs();
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
    
    public bool IsActive()
    {
        return modal.isVisible;
    }

    public void Dispose()
    {
        newProjectFirstStepView.OnBackPressed += BackPressed;
        newProjectSecondStepView.OnBackPressed += BackPressed;

        newProjectFirstStepView.OnNextPressed -= SetTittleAndDescription;
        newProjectSecondStepView.OnNextPressed -= SetSize;
    }

    internal void SetSize(int rows, int colums)
    {
        NextPressed();
        OnSizeSet?.Invoke(rows, colums);
    }

    internal void SetTittleAndDescription(string title, string description)
    {
        NextPressed();
        OnTittleAndDescriptionSet?.Invoke(title, description);
    }

    internal void NextPressed()
    {
        if (currentStep >= 2)
            return;

        currentStep++;
        carrousel.GoToNextItem();
    }

    internal void BackPressed()
    {
        if (currentStep == 0)
        {
            Hide();
            DataStore.i.builderInWorld.areShortcutsBlocked.Set(false);
        }
        else
        {
            currentStep--;
            carrousel.GoToPreviousItem();
        }
    }
}