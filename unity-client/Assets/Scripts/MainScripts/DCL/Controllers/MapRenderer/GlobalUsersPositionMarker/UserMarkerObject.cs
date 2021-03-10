using UnityEngine;
using UnityEngine.UI;

public class UserMarkerObject : MonoBehaviour
{
    [SerializeField] Image colorImage = null;
    [SerializeField] internal Color sameRealmColor;
    [SerializeField] internal Color otherRealmColor;

    public Color color
    {
        set
        {
            if (colorImage)
            {
                colorImage.color = value;
            }
        }
    }
}
