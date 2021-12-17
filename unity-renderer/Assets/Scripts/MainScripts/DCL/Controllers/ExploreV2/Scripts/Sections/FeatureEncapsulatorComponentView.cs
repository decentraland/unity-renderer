using UnityEngine;

public interface IFeatureEncapsulatorComponentView
{
    /// <summary>
    /// Encapsulates a feature HUD into the section.
    /// </summary>
    /// <param name="featureConfiguratorFlag">Flag used to configurates the feature.</param>
    void EncapsulateFeature(BaseVariable<Transform> featureConfiguratorFlag);
}

public class FeatureEncapsulatorComponentView : BaseComponentView, IFeatureEncapsulatorComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal Transform featureContainer;

    public override void RefreshControl() { }

    public void EncapsulateFeature(BaseVariable<Transform> featureConfiguratorFlag) { featureConfiguratorFlag.Set(featureContainer, true); }
}