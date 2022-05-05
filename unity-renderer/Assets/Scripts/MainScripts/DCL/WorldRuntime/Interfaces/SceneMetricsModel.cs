using System;
using UnityEngine;

namespace DCL
{
    [System.Serializable]
    public class SceneMetricsModel: IEquatable<SceneMetricsModel>
    {
        public int meshes;
        public int bodies;
        public int materials;
        public int textures;
        public int triangles;
        public int entities;
        public float sceneHeight;

        public long textureMemoryScore;
        public long meshMemoryScore;
        public long audioClipMemoryScore;
        public long animationClipMemoryScore;

        public long textureMemoryProfiler;
        public long meshMemoryProfiler;
        public long audioClipMemoryProfiler;
        public long animationClipMemoryProfiler;

        public long totalMemoryScore =>
            textureMemoryScore + meshMemoryScore + audioClipMemoryScore + animationClipMemoryScore;

        public long totalMemoryProfiler =>
            textureMemoryProfiler + meshMemoryProfiler + audioClipMemoryProfiler + animationClipMemoryProfiler;

        public SceneMetricsModel Clone() { return (SceneMetricsModel) MemberwiseClone(); }

        public bool Equals(SceneMetricsModel obj)
        {
            if (obj == null)
                return false;

            return obj.meshes == meshes && obj.bodies == bodies && obj.materials == materials &&
                   obj.textures == textures && obj.triangles == triangles && obj.entities == entities && Mathf.Abs(obj.sceneHeight - sceneHeight) < 0.001f;
        }

        public static bool operator >(SceneMetricsModel lhs, SceneMetricsModel rhs)
        {
            int lhsValue = lhs.meshes + lhs.bodies + lhs.materials + lhs.textures + lhs.triangles + lhs.entities;
            int rhsValue = rhs.meshes + rhs.bodies + rhs.materials + rhs.textures + rhs.triangles + rhs.entities;

            return lhsValue > rhsValue;
        }

        public static bool operator <(SceneMetricsModel lhs, SceneMetricsModel rhs)
        {
            int lhsValue = lhs.meshes + lhs.bodies + lhs.materials + lhs.textures + lhs.triangles + lhs.entities;
            int rhsValue = rhs.meshes + rhs.bodies + rhs.materials + rhs.textures + rhs.triangles + rhs.entities;

            return lhsValue < rhsValue;
        }

        public static SceneMetricsModel operator -(SceneMetricsModel lhs, SceneMetricsModel rhs)
        {
            SceneMetricsModel result = lhs.Clone();
            result.bodies -= rhs.bodies;
            result.entities -= rhs.entities;
            result.materials -= rhs.materials;
            result.textures -= rhs.textures;
            result.meshes -= rhs.meshes;
            result.triangles -= rhs.triangles;
            result.meshMemoryScore -= rhs.meshMemoryScore;
            result.textureMemoryScore -= rhs.textureMemoryScore;
            result.animationClipMemoryScore -= rhs.animationClipMemoryScore;
            result.audioClipMemoryScore -= rhs.audioClipMemoryScore;
            result.meshMemoryProfiler -= rhs.meshMemoryProfiler;
            result.textureMemoryProfiler -= rhs.textureMemoryProfiler;
            result.animationClipMemoryProfiler -= rhs.animationClipMemoryProfiler;
            result.audioClipMemoryProfiler -= rhs.audioClipMemoryProfiler;
            return result;
        }

        public static SceneMetricsModel operator +(SceneMetricsModel lhs, SceneMetricsModel rhs)
        {
            SceneMetricsModel result = lhs.Clone();
            result.bodies += rhs.bodies;
            result.entities += rhs.entities;
            result.materials += rhs.materials;
            result.textures += rhs.textures;
            result.meshes += rhs.meshes;
            result.triangles += rhs.triangles;
            result.meshMemoryScore += rhs.meshMemoryScore;
            result.textureMemoryScore += rhs.textureMemoryScore;
            result.animationClipMemoryScore += rhs.animationClipMemoryScore;
            result.audioClipMemoryScore += rhs.audioClipMemoryScore;
            result.meshMemoryProfiler += rhs.meshMemoryProfiler;
            result.textureMemoryProfiler += rhs.textureMemoryProfiler;
            result.animationClipMemoryProfiler += rhs.animationClipMemoryProfiler;
            result.audioClipMemoryProfiler += rhs.audioClipMemoryProfiler;
            return result;
        }

        public override string ToString()
        {
            string result = "";

            result += $"Textures: {textures}";
            result += $"- Triangles: {triangles}";
            result += $"- Meshes: {meshes}";
            result += $"- Materials: {materials}";
            result += $"- Entities: {entities}";
            result += $"- Bodies: {bodies}";
            result += $"- Scene Height: {sceneHeight}";
            result += $"- Mesh Memory: {meshMemoryScore}";
            result += $"- Texture Memory: {textureMemoryScore}";
            result += $"- Audio Memory: {audioClipMemoryScore}";
            result += $"- Animation Memory: {animationClipMemoryScore}";

            return result;
        }
    }
}