using UnityEngine;

namespace DCL.Backpack
{
    public class TestBackpack : MonoBehaviour
    {
        [SerializeField] private AvatarSlotsView slotsView;

        // Start is called before the first frame update
        void Start()
        {
            var avatarSlotsHUDController = new AvatarSlotsHUDController(slotsView);
        }

        // Update is called once per frame
        void Update() { }
    }
}
