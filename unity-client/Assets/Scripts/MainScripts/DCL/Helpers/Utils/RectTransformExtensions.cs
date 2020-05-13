using UnityEngine;

public static class RectTransformExtensions
{
    public static int CountCornersVisibleFrom(this RectTransform rectTransform, RectTransform viewport)
    {
        Vector3[] viewCorners = new Vector3[4];
        viewport.GetWorldCorners(viewCorners);

        Vector2 size = new Vector2(viewport.rect.size.x * viewport.lossyScale.x, viewport.rect.size.y * viewport.lossyScale.y);
        Rect screenBounds = new Rect(viewCorners[0], size); // Screen space bounds (assumes camera renders across the entire screen)

        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);

        int visibleCorners = 0;

        for (var i = 0; i < viewCorners.Length; i++)
        {
            if (i != viewCorners.Length - 1)
                Debug.DrawLine(viewCorners[i], viewCorners[i + 1], Color.blue, 1.0f);
        }

        for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
        {
            if (screenBounds.Contains(objectCorners[i])) // If the corner is inside the screen
            {
                visibleCorners++;
            }
        }

        for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
        {
            if (i != objectCorners.Length - 1)
                Debug.DrawLine(objectCorners[i], objectCorners[i + 1], visibleCorners > 0 ? Color.green : Color.red, 1.0f);
        }

        return visibleCorners;
    }
}
