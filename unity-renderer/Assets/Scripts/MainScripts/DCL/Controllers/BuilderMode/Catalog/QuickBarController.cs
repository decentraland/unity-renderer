using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickBarController 
{
    public event System.Action<SceneObject> OnSceneObjectSelected;

    List<SceneObject> quickBarShortcutsSceneObjects = new List<SceneObject>() { null, null, null, null, null, null, null, null, null };

    public QuickBarController(QuickBarView view)
    { 
        view.OnQuickBarAdd += SetQuickBarShortcut;
    }

    public void QuickBarObjectSelected(int index)
    {
        if (quickBarShortcutsSceneObjects.Count > index && quickBarShortcutsSceneObjects[index] != null)
            OnSceneObjectSelected?.Invoke(quickBarShortcutsSceneObjects[index]);
    }

    void SetQuickBarShortcut(SceneObject sceneObject, int index)
    {
        quickBarShortcutsSceneObjects[index] = sceneObject;
    }

    int FindEmptyShortcutSlot()
    {
        int index = quickBarShortcutsSceneObjects.Count;
        int cont = 0;
        foreach (SceneObject sceneObjectIteration in quickBarShortcutsSceneObjects)
        {
            if (sceneObjectIteration == null)
            {
                index = cont;
                break;
            }
            cont++;
        }
        return index;
    }
}
