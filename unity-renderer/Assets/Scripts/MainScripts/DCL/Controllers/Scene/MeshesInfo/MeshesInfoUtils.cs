using DCL.Helpers;
using UnityEngine;

namespace DCL.Models
{
    public static class MeshesInfoUtils
    {
        public static Bounds BuildMergedBounds(Renderer[] renderers)
        {
            Bounds bounds = new Bounds();

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;

                if (i == 0)
                    bounds = renderers[i].GetSafeBounds();
                else
                    bounds.Encapsulate(renderers[i].GetSafeBounds());
            }

            return bounds;
        }
    }
}