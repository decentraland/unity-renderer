using UnityEngine;
using UnityEngine.UI;

public class SwitchToggle : Toggle
{
    private static readonly int animatorOnBoolean = Animator.StringToHash("isOn");

    [SerializeField] Animator switchAnimator;

    protected override void Awake()
    {
        base.Awake();
        OnValueChanged(this.isOn);
        this.onValueChanged.AddListener(OnValueChanged);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        OnValueChanged(this.isOn);
    }

    private void OnValueChanged(bool value)
    {
        switchAnimator.SetBool(animatorOnBoolean, value);
    }
}
