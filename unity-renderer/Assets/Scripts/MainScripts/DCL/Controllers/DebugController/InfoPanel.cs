using TMPro;
using UnityEngine;

namespace DCL
{
    public class InfoPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI networkText;
        [SerializeField] private TextMeshProUGUI realmText;

        public void SetVisible(bool active)
        {
            gameObject.SetActive(active);
        }

        public void Setup(string network, string realm)
        {
            networkText.text = network;
            realmText.text = realm;
        }
    }
}