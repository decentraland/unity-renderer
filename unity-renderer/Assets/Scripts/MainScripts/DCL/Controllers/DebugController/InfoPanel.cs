using TMPro;
using UnityEngine;

namespace DCL
{
    public class InfoPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI infoText;

        public void SetVisible(bool active)
        {
            gameObject.SetActive(active);
        }

        public void Setup(string info)
        {
            infoText.text = info;
        }
    }
}