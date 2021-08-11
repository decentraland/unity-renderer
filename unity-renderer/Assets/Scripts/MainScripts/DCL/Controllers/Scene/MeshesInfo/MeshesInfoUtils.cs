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

        /// <summary>
        /// This get the renderer bounds with a check to ensure the renderer is at a safe position.
        /// If the renderer is too far away from 0,0,0, wasm target ensures a crash.
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns>The bounds value if the value is correct, or a mocked bounds object with clamped values if its too far away.</returns>
        public static Bounds GetSafeBounds( this Renderer renderer )
        {
            // World extents are of 4800 world mts, so this limit far exceeds the world size.
            const float POSITION_OVERFLOW_LIMIT = 10000;
            const float POSITION_OVERFLOW_LIMIT_SQR = POSITION_OVERFLOW_LIMIT * POSITION_OVERFLOW_LIMIT;

            if ( renderer.transform.position.sqrMagnitude > POSITION_OVERFLOW_LIMIT_SQR )
                return new Bounds( Vector3.one * POSITION_OVERFLOW_LIMIT, Vector3.one * 0.1f );

            return renderer.bounds;
        }
    }
}