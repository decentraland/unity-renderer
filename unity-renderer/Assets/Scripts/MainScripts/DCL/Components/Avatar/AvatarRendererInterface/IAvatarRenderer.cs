using UnityEngine;

namespace DCL
{
    public interface IAvatarRenderer
    {
        void CleanupAvatar();
        void UpdateExpressions(string id, long timestamp);
        void SetVisibility(bool newVisibility);
        MeshRenderer GetLODRenderer();
        Mesh GetLODMesh();
        Transform GetTransform();
    }
}