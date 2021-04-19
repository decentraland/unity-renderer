using System;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IDragAndDropSceneObjectView
{
    event Action OnDrop;

    void Drop();
}

public class DragAndDropSceneObjectView : MonoBehaviour, IDragAndDropSceneObjectView
{
    public event Action OnDrop;

    [SerializeField] internal EventTrigger dragAndDropEventTrigger;

    private const string VIEW_PATH = "GodMode/DragAndDropSceneObjectView";

    internal static DragAndDropSceneObjectView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<DragAndDropSceneObjectView>();
        view.gameObject.name = "_DragAndDropSceneObjectView";

        return view;
    }

    private void Awake()
    {
        BuilderInWorldUtils.ConfigureEventTrigger(dragAndDropEventTrigger, EventTriggerType.Drop, (eventData) => Drop());
    }
    private void OnDestroy()
    {
        BuilderInWorldUtils.RemoveEventTrigger(dragAndDropEventTrigger, EventTriggerType.Drop);
    }

    public void Drop()
    {
        OnDrop?.Invoke();
    }
}
