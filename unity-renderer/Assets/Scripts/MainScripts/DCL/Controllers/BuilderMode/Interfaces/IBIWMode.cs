public interface IBIWMode
{
    void EntityDeselected(BIWEntity entityDeselected);
    void OnDeselectedEntities();
    void SelectedEntity(BIWEntity selectedEntity);
    void SetDuplicationOffset(float offset);
}