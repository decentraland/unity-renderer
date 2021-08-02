using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Camera;
using DCL.Components;
using DCL.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWGodCameraShould : IntegrationTestSuite_Legacy
{
    private FreeCameraMovement freeCameraMovement;
    private GameObject gameObject;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        freeCameraMovement = Resources.FindObjectsOfTypeAll<FreeCameraMovement>().FirstOrDefault();
        gameObject = freeCameraMovement.gameObject;
    }

    [UnityTest]
    public IEnumerator FocusOnEntities()
    {
        //Arrange
        freeCameraMovement.gameObject.SetActive(true);
        Vector3 currentPosition = gameObject.transform.position;

        string entityId = "Test";

        BIWEntity newEntity = new BIWEntity();
        newEntity.Init(TestHelpers.CreateSceneEntity(scene, entityId), null);
        newEntity.rootEntity.gameObject.transform.position = Vector3.one * 444;

        TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);


        //Act
        freeCameraMovement.FocusOnEntities(new List<BIWEntity> { newEntity });

        yield return new WaitForSeconds(0.1f);

        //Assert
        Assert.IsTrue(Vector3.Distance(currentPosition, gameObject.transform.position) >= 0.001f);
    }

    private bool screenshotTaken = false;
    [UnityTest]
    public IEnumerator TakeScreenshot()
    {
        //Arrange
        freeCameraMovement.gameObject.SetActive(true);

        //Act
        freeCameraMovement.TakeSceneScreenshot(AssertScreenShot);
        yield return new WaitUntil(() => screenshotTaken);
    }

    private void AssertScreenShot(Texture2D screenshot)
    {
        screenshotTaken = true;

        //Assert
        Assert.IsNotNull(screenshot);
    }

    protected override IEnumerator TearDown()
    {
        freeCameraMovement.gameObject.SetActive(false);
        yield return base.TearDown();
    }
}