using System;
using UnityEngine;

namespace DCL.Models {
  [Serializable]
  public class DecentralandEntity {
    public GameObject gameObject;
    public string entityId;

    public delegate void EntityComponentEventDelegate(DCL.Components.UpdateableComponent componentUpdated);
    public EntityComponentEventDelegate OnComponentUpdated;
  }
}
