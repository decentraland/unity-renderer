using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderInWorldEntityAction 
{
    public string entityId;

    public object oldValue;
    public object newValue;


    public BuilderInWorldEntityAction(string entityId)
    {
        this.entityId = entityId;
    }

    public BuilderInWorldEntityAction(string entityId, object oldValue, object newValue)
    {
        this.entityId = entityId;
        this.oldValue = oldValue;
        this.newValue = newValue;
    }

    public BuilderInWorldEntityAction(object oldValue, object newValue)
    {
        this.oldValue = oldValue;
        this.newValue = newValue;
    }

    public BuilderInWorldEntityAction(DecentralandEntity entity)
    {
        this.entityId = entity.entityId;
    }

    public BuilderInWorldEntityAction(DecentralandEntity entity,object oldValue,object newValue)
    {
        this.entityId = entity.entityId;
        this.oldValue = oldValue;
        this.newValue = newValue;
    }
}
