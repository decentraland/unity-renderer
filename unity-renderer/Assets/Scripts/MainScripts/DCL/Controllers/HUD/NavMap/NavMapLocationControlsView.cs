using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class NavMapLocationControlsView : MonoBehaviour
    {
        [field: SerializeField] internal Button homeButton;
        [field: SerializeField] internal Button centerToPlayerButton;

        [Space]
        [SerializeField] private GameObject homeButtonRoot;
        [SerializeField] private GameObject centerToPlayerButtonRoot;

        public void Hide()
        {
            homeButtonRoot.SetActive(false);
            centerToPlayerButtonRoot.SetActive(false);
        }
    }
}
