using System.Collections;
using DCL.Camera;
using NUnit.Framework;
using Tests;
using UnityEngine;

public class BIWFreeCameraShould : IntegrationTestSuite
{
    private FreeCameraMovement freeCameraMovement;
    private GameObject mockedGameObject;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        mockedGameObject = new GameObject("MockedGameObject");
        freeCameraMovement = mockedGameObject.AddComponent<FreeCameraMovement>();
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void StartDectectingMovement()
    {
        //Arrange
        freeCameraMovement.StopDetectingMovement();

        //Act
        freeCameraMovement.StartDetectingMovement();

        //Assert
        Assert.IsTrue(freeCameraMovement.isDetectingMovement);
        Assert.IsFalse(freeCameraMovement.HasBeenMovement());
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void StopDectectingMovement()
    {
        //Arrange
        freeCameraMovement.StartDetectingMovement();

        //Act
        freeCameraMovement.StopDetectingMovement();

        //Assert
        Assert.IsFalse(freeCameraMovement.isDetectingMovement);
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void InputMouseDown()
    {
        //Act
        freeCameraMovement.OnInputMouseDown(1, Vector3.zero);

        //Assert
        Assert.IsTrue(freeCameraMovement.isMouseRightClickDown);
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void HandleCameraMovementPositiveInput()
    {
        //Arrange
        freeCameraMovement.isAdvancingForward = true;
        freeCameraMovement.isAdvancingRight = true;
        freeCameraMovement.isAdvancingUp = true;
        freeCameraMovement.isMouseRightClickDown = true;

        //Act
        freeCameraMovement.HandleCameraMovementInput();

        //Assert
        Assert.Greater(freeCameraMovement.direction.magnitude, Vector3.zero.magnitude);
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void HandleCameraMovementNegativeInput()
    {
        //Arrange
        freeCameraMovement.isAdvancingBackward = true;
        freeCameraMovement.isAdvancingLeft = true;
        freeCameraMovement.isAdvancingDown = true;
        freeCameraMovement.isMouseRightClickDown = true;

        //Act
        freeCameraMovement.HandleCameraMovementInput();

        //Assert
        Assert.Greater(freeCameraMovement.direction.magnitude, Vector3.zero.magnitude);
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void CameraCanMove()
    {
        //Arrange
        freeCameraMovement.SetCameraCanMove(false);

        //Act
        freeCameraMovement.SetCameraCanMove(true);

        //Assert
        Assert.IsTrue(freeCameraMovement.isCameraAbleToMove);
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void MouseScroll()
    {
        //Act
        freeCameraMovement.MouseWheel(5f);

        //Assert
        Assert.IsNotNull(freeCameraMovement.smoothScrollCoroutine);
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void MouseDragRaw()
    {
        //Arrange
        float currentYaw = freeCameraMovement.yaw;
        float currentPitch = freeCameraMovement.pitch;
        freeCameraMovement.isPanCameraActive = false;
        freeCameraMovement.isMouseRightClickDown = true;

        //Act
        freeCameraMovement.MouseDragRaw(1, Vector3.zero, 5f, 5f);

        //Assert
        Assert.Greater(freeCameraMovement.yaw, currentYaw);
        Assert.Less(freeCameraMovement.pitch, currentPitch);
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void MouseDrag()
    {
        //Arrange
        float currentPanAxisX = freeCameraMovement.panAxisX;
        float currentPanAxisY = freeCameraMovement.panAxisY;
        freeCameraMovement.isPanCameraActive = true;

        //Act
        freeCameraMovement.MouseDrag(1, Vector3.zero, -5f, -5f);

        //Assert
        Assert.Greater(freeCameraMovement.panAxisX, currentPanAxisX);
        Assert.Greater(freeCameraMovement.panAxisY, currentPanAxisY);
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void GetGameObjectRotation()
    {
        //Act
        mockedGameObject.transform.rotation = Quaternion.identity;

        //Assert
        Assert.AreEqual(mockedGameObject.transform.rotation.eulerAngles, freeCameraMovement.OnGetRotation());
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void SetResetConfiguration()
    {
        //Act
        freeCameraMovement.SetResetConfiguration(Vector3.one, mockedGameObject.transform);

        //Assert
        Assert.AreEqual(freeCameraMovement.originalCameraPosition, Vector3.one);
        Assert.AreEqual(freeCameraMovement.originalCameraPointToLookAt, mockedGameObject.transform.position);
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void ResetCameraPosition()
    {
        //Arrange
        freeCameraMovement.SetResetConfiguration(Vector3.one, mockedGameObject.transform);

        //Act
        freeCameraMovement.ResetCameraPosition();

        //Assert
        Assert.AreEqual(freeCameraMovement.originalCameraPosition, mockedGameObject.transform.position);
        Assert.AreEqual(freeCameraMovement.yaw, mockedGameObject.transform.eulerAngles.y);
        Assert.AreEqual(freeCameraMovement.pitch, mockedGameObject.transform.eulerAngles.x);
    }

    [Test]
    [Explicit("BIW is deprecated")]
    public void InputMouseUp()
    {
        //Act
        freeCameraMovement.OnInputMouseUp(1, Vector3.zero);

        //Assert
        Assert.IsFalse(freeCameraMovement.isMouseRightClickDown);
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(mockedGameObject);
        yield return base.TearDown();
    }
}