using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWEntityAction
{
    public long entityId;

    public object oldValue;
    public object newValue;

    public BIWEntityAction(long entityId) { this.entityId = entityId; }

    public BIWEntityAction(long entityId, object oldValue, object newValue)
    {
        this.entityId = entityId;
        this.oldValue = oldValue;
        this.newValue = newValue;
    }

    public BIWEntityAction(object oldValue, object newValue)
    {
        this.oldValue = oldValue;
        this.newValue = newValue;
    }

    public BIWEntityAction(IDCLEntity entity) { this.entityId = entity.entityId; }

    public BIWEntityAction(IDCLEntity entity, object oldValue, object newValue)
    {
        this.entityId = entity.entityId;
        this.oldValue = oldValue;
        this.newValue = newValue;
    }
}