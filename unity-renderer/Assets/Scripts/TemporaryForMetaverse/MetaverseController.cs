using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;

public class MetaverseController : MonoBehaviour
{
    [SerializeField] private Button open;
    [SerializeField] private Button close;
    [SerializeField] private Button jumpin;
    [SerializeField] private GameObject container;
    [SerializeField] private Vector2Int coords;

    // Start is called before the first frame update
    void Start()
    {
        container.SetActive(false);
        open.onClick.AddListener(OnOpen);
        close.onClick.AddListener(OnClose);
        jumpin.onClick.AddListener(OnJumpIn);
    }
    private void OnJumpIn()
    {
        WebInterface.SendChatMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.NONE,
            recipient = string.Empty,
            body = $"/goto {coords.x},{coords.y}",
        });
    }

    private void OnOpen() { container.SetActive(true); }

    private void OnClose() { container.SetActive(false); }

}