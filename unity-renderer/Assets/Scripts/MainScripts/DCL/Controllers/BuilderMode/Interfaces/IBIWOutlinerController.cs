using System.Collections.Generic;

public interface IBIWOutlinerController : IBIWController
{
    void OutlineEntity(BIWEntity entity);
    void CancelEntityOutline(BIWEntity entityToQuitOutline);
    void OutlineEntities(List<BIWEntity> entitiesToEdit);
    void CheckOutline();
    void CancelUnselectedOutlines();
    void CancelAllOutlines();
    void SetOutlineCheckActive(bool isActive);
}