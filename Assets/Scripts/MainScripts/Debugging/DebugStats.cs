using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugStats : MonoBehaviour
{
    public Text debugStats;

    private void Start()
    {
        InvokeRepeating("LazyUpdate", 0, 0.5f);
    }

    private void LazyUpdate()
    {
        int sharedCount = 0;
        int sharedAttachCount = 0;
        int componentCount = 0;
        int entityCount = 0;
        int materialCount = 0;
        int meshesCount = 0;

        foreach (var v in SceneController.i.loadedScenes)
        {
            meshesCount += v.Value.metricsController.GetModel().meshes;
            materialCount += v.Value.metricsController.GetModel().materials;

            sharedCount += v.Value.disposableComponents.Count;

            foreach (var e in v.Value.disposableComponents)
            {
                sharedAttachCount += e.Value.attachedEntities.Count;
            }

            entityCount += v.Value.entities.Count;

            foreach (var e in v.Value.entities)
            {
                componentCount += e.Value.components.Count;
            }
        }

        debugStats.text = "Shared Objects Count: " + sharedCount + "... attached = " + sharedAttachCount;
        debugStats.text += "\nComponent Objects Count: " + componentCount;
        debugStats.text += "\nEntity Objects Count: " + entityCount;
        debugStats.text += "\n";
        debugStats.text += "\nMaterial Count:" + materialCount;
        debugStats.text += "\nMeshes Count:" + meshesCount;
    }
}
