using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWRotateGizmosShould
{
    private GameObject mockedScaleGizmosGameObject;
    private GameObject mockedEntityGameObject;
    private BIWScaleGizmos gizmos;
    private BIWGizmosAxis axis;

    [UnitySetUp]
    protected IEnumerator SetUp()
    {
        mockedScaleGizmosGameObject = new GameObject("ScaleGizmos");
        mockedEntityGameObject = new GameObject("EntityScaleGizmos");
        gizmos = mockedScaleGizmosGameObject.AddComponent<BIWScaleGizmos>();
        axis = mockedScaleGizmosGameObject.AddComponent<BIWGizmosAxis>();
        axis.SetGizmo(gizmos);
        gizmos.activeAxis = axis;

        yield break;
    }

    [UnityTearDown]
    protected IEnumerator TearDown()
    {
        GameObject.Destroy(mockedScaleGizmosGameObject);
        yield break;
    }
}