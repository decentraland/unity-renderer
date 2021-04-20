using System;

public interface IDragAndDropSceneObjectController
{
    event Action OnDrop;

    void Initialize(IDragAndDropSceneObjectView dragAndDropSceneObjectView);
    void Dispose();
    void Drop();
}

public class DragAndDropSceneObjectController : IDragAndDropSceneObjectController
{
    public event Action OnDrop;

    private IDragAndDropSceneObjectView dragAndDropSceneObjectView;

    public void Initialize(IDragAndDropSceneObjectView dragAndDropSceneObjectView)
    {
        this.dragAndDropSceneObjectView = dragAndDropSceneObjectView;

        dragAndDropSceneObjectView.OnDrop += Drop;
    }

    public void Dispose()
    {
        dragAndDropSceneObjectView.OnDrop -= Drop;
    }

    public void Drop()
    {
        OnDrop?.Invoke();
    }
}
