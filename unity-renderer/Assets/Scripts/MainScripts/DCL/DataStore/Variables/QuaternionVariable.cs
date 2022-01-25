using UnityEngine;

[CreateAssetMenu(fileName = "QuaternionVariable", menuName = "Variables/QuaternionVariable")]
public class QuaternionVariable : BaseVariableAsset<Quaternion>
{
    // Kinerius: euler angles comparison proved to be more reliable than comparing quaternion itself
    public override bool Equals(Quaternion other) { return value.eulerAngles == other.eulerAngles; }
}