using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuilderInWorldInititalHUDView : MonoBehaviour
{
    public Button enterEditModeBtn;


    public event Action OnEnterInEditMode;

    [SerializeField] internal ShowHideAnimator showHideAnimator;

    private void Start()
    {
        enterEditModeBtn.onClick.AddListener(() => OnEnterInEditMode?.Invoke());
    }

}
