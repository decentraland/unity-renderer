public interface IBIWCompleteAction
{
    public enum ActionType
    {
        MOVE = 0,
        ROTATE = 1,
        SCALE = 2,
        CREATE = 3,
        DELETE = 4,
        CHANGE_FLOOR = 5
    }

    delegate void OnApplyValueDelegate(long entityId, object value, ActionType actionType, bool isUndo);
    event OnApplyValueDelegate OnApplyValue;
    void Undo();
    void Redo();
    bool IsDone();
}