using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickBarController 
{
    public event System.Action<SceneObject> OnSceneObjectSelected;

    SceneObject[] quickBarShortcutsSceneObjects = new SceneObject[amountOQuickBarfSlots];

    const int amountOQuickBarfSlots = 9;

    public QuickBarController(QuickBarView view)
    { 
        view.OnQuickBarAdd += SetQuickBarShortcut;
    }

    public void QuickBarObjectSelected(int index)
    {
        if (quickBarShortcutsSceneObjects.Length > index && quickBarShortcutsSceneObjects[index] != null)
            OnSceneObjectSelected?.Invoke(quickBarShortcutsSceneObjects[index]);
    }

    void SetQuickBarShortcut(SceneObject sceneObject, int index)
    {
        quickBarShortcutsSceneObjects[index] = sceneObject;
    }

    int FindEmptyShortcutSlot()
    {
        for (int i = 0; i < quickBarShortcutsSceneObjects.Length; i++)
        {
            if (quickBarShortcutsSceneObjects[i] == null)
                return i;
        }
        return -1;
    }
}
