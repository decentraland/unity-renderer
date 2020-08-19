using UnityEngine;
using DCL.Models;
using DCL.Controllers;
using System.Collections.Generic;

public class AvatarReference : Attachable
{         

    public static AvatarReference i { get; private set; }        

    protected override void Setup()
    {
        // Singleton setup
        if (i != null)
        {
            Destroy(gameObject);
            return;
        }

        i = this;                
    }

}
