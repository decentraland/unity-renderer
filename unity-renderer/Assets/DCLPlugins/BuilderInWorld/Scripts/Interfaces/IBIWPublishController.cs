using System;
using DCL;
using DCL.Builder;
using UnityEngine;

public interface IBIWPublishController : IBIWController
{
    /// <summary>
    /// This will return is there are any unpublished changes
    /// </summary>
    /// <returns></returns>
    bool HasUnpublishChanges();
}