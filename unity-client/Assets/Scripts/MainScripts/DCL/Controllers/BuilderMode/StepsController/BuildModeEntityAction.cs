using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeEntityAction 
{
    public DecentralandEntity entity;
    public object oldValue, newValue;


    public BuildModeEntityAction(DecentralandEntity entity)
    {
        this.entity = entity;
    }
    public BuildModeEntityAction(DecentralandEntity entity,object oldValue,object newValue)
    {
        this.entity = entity;
        this.oldValue = oldValue;
        this.newValue = newValue;
    }
}
