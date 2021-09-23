using UnityEngine;

[CreateAssetMenu(fileName = "GPUSkinningThrottlingCurve", menuName = "DCL/GPUSkinning throttling curve")]
public class GPUSkinningThrottlingCurveSO : ScriptableObject
{
    public AnimationCurve curve;
}