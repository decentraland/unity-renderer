using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestSceneIntegrityChecker
{
    private const bool DEBUG_PAUSE_ON_INTEGRITY_FAIL = false;
    public bool enabled = true;

    protected List<Component> startingSceneComponents = new List<Component>();

    public IEnumerator SaveSceneSnapshot()
    {
        if (!enabled)
            yield break;

        //NOTE(Brian): to make it run faster in CI
        if (Application.isBatchMode)
            yield break;

        yield return null;
        startingSceneComponents = GetAllSceneComponents();
    }

    static List<Component> GetAllSceneComponents()
    {
        if (!(Resources.FindObjectsOfTypeAll(typeof(Component)) is Component[] components))
            return new List<Component>();

        List<Component> result = new List<Component>();

        foreach (Component go in components)
        {
            bool isPersistent = EditorUtility.IsPersistent(go.transform.root.gameObject);
            bool isEditableOrVisible = go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave;

            if (!isPersistent && !isEditableOrVisible)
            {
                result.Add(go);
            }
        }

        return result;
    }

    public IEnumerator TestSceneSnapshot()
    {
        if (!enabled)
            yield break;

        if (startingSceneComponents == null)
        {
            Debug.LogError("SceneIntegrityChecker fail. Check called without Saving snapshot?");
            yield break;
        }

        //NOTE(Brian): If any Destroy() calls are pending, this will flush them.
        yield return null;

        List<Component> currentObjects = GetAllSceneComponents();
        List<Component> newObjects = new List<Component>();

        foreach (var o in currentObjects)
        {
            if (o.ToString().Contains("MainCamera"))
                continue;

            if (!startingSceneComponents.Contains(o))
            {
                newObjects.Add(o);
            }
        }

        if (newObjects.Count > 0)
        {
            Debug.LogError("Dangling components detected!. Look your TearDown code, you missed to destroy objects after the tests?.");

            //NOTE(Brian): Can't use asserts here because Unity Editor hangs for some reason.
            foreach (var o in newObjects)
            {
                if (DEBUG_PAUSE_ON_INTEGRITY_FAIL)
                    Debug.LogError($"Component - {o} (id: {o.GetInstanceID()}) (Click to highlight)", o.gameObject);
                else
                    Debug.LogError($"Component - {o}", o.gameObject);
            }

            if (DEBUG_PAUSE_ON_INTEGRITY_FAIL)
            {
                Debug.Break();
                yield return null;
            }
        }
    }
}