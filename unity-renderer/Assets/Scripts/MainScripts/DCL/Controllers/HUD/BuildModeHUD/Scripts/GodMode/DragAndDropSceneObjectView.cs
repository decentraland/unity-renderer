using System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IDragAndDropSceneObjectView
{
    Canvas GetGeneralCanvas();
    event Action OnDrop;

    void Drop();
}

public class DragAndDropSceneObjectView : MonoBehaviour, IDragAndDropSceneObjectView
{
    public Canvas generalCanvas;
    public event Action OnDrop;

    [SerializeField] internal EventTrigger dragAndDropEventTrigger;

    private const string VIEW_PATH = "GodMode/DragAndDropSceneObjectView";

    internal static DragAndDropSceneObjectView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<DragAndDropSceneObjectView>();
        view.gameObject.name = "_DragAndDropSceneObjectView";

        return view;
    }

    private void Awake() { BIWUtils.ConfigureEventTrigger(dragAndDropEventTrigger, EventTriggerType.Drop, (eventData) => Drop()); }

    private void OnDestroy() { BIWUtils.RemoveEventTrigger(dragAndDropEventTrigger, EventTriggerType.Drop); }

    public Canvas GetGeneralCanvas() { return generalCanvas; }

    public void Drop() { OnDrop?.Invoke(); }
}