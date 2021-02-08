using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderInWorldInititalHUDController : IHUD
{

    public event Action OnEnterEditMode;
    public event Action OnClose;

    internal BuilderInWorldInititalHUDView view;

    public BuilderInWorldInititalHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("BuilderInWorldInitialHUD")).GetComponent<BuilderInWorldInititalHUDView>();

        view.name = "_BuilderInWorldInitialHUD";
        view.gameObject.SetActive(false);

        view.OnEnterInEditMode += () => OnEnterEditMode?.Invoke();
        view.OnEnterInEditMode += () => CloseBuilderInWorldInitialView();
    }

    public void OpenBuilderInWorldInitialView()
    {
        SetVisibility(true);
    }

    public void CloseBuilderInWorldInitialView()
    {
        SetVisibility(false);
        OnClose?.Invoke();
    }

    public void SetVisibility(bool visible)
    {
        if (!view)
            return;

        if (IsVisible() && !visible)
        {

            view.showHideAnimator.Hide();

            AudioScriptableObjects.fadeOut.Play(true);
        }
        else if (!IsVisible() && visible)
        {
            view.gameObject.SetActive(true);
            view.showHideAnimator.Show();
            AudioScriptableObjects.fadeIn.Play(true);
        }
    }

    public void Dispose()
    {
        if (view)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }

    public void ToggleVisibility()
    {
        SetVisibility(!IsVisible());
    }

    public bool IsVisible()
    {
        if (!view)
            return false;

        return view.showHideAnimator.isVisible;
    }

}
