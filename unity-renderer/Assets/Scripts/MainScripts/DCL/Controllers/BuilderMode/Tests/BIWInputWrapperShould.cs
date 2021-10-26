using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using NUnit.Framework;
using Tests;
using UnityEngine;

public class BIWInputWrapperShould : IntegrationTestSuite
{
    private BIWInputWrapper inputWrapper;
    private IContext context;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        inputWrapper = new BIWInputWrapper();
        context = BIWTestUtils.CreateContextWithGenericMocks(
            inputWrapper);

        inputWrapper.Initialize(context);
    }

    [Test]
    public void StopInput()
    {
        //Act
        inputWrapper.StopInput();

        //Assert
        Assert.IsFalse(inputWrapper.canInputBeMade);
    }

    [Test]
    public void ResumeInput()
    {
        //Act
        inputWrapper.ResumeInput();

        //Assert
        Assert.IsTrue(inputWrapper.canInputBeMade);
    }

    [Test]
    public void MouseWheelInput()
    {
        //Arrange
        BIWInputWrapper.OnMouseWheel += MouseWheel;

        //Act
        inputWrapper.OnMouseWheelInput(5f);
    }

    [Test]
    public void MouseUpOnUI()
    {
        //Arrange
        BIWInputWrapper.OnMouseClickOnUI += ActionCalled;
        inputWrapper.currentClickIsOnUi = true;

        //Act
        inputWrapper.MouseUp(1, Vector3.one);
    }

    [Test]
    public void MouseOnUIUpInvoke()
    {
        //Arrange
        BIWInputWrapper.OnMouseUpOnUI += ActionCalled;

        //Act
        inputWrapper.MouseUpOnGUI(1, Vector3.one);
    }

    [Test]
    public void MouseOnUpInvoke()
    {
        //Arrange
        BIWInputWrapper.OnMouseUp += ActionCalled;
        BIWInputWrapper.OnMouseClick += ActionCalled;

        //Act
        inputWrapper.MouseUpInvoke(1, Vector3.one);
    }

    [Test]
    public void MouseOnDownInvoke()
    {
        //Arrange
        BIWInputWrapper.OnMouseDown += ActionCalled;

        //Act
        inputWrapper.MouseDown(1, Vector3.one);
    }

    [Test]
    public void MouseDragInvoke()
    {
        //Arrange
        BIWInputWrapper.OnMouseDrag += OnMouseDragAssert;

        //Act
        inputWrapper.MouseDrag(1, Vector3.one, 5, 5);
    }

    [Test]
    public void MouseDragRawInvoke()
    {
        //Arrange
        BIWInputWrapper.OnMouseDragRaw += OnMouseDragAssert;

        //Act
        inputWrapper.MouseRawDrag(1, Vector3.one, 5, 5);
    }

    private void OnMouseDragAssert(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        //Assert
        Assert.AreEqual(1, buttonId);
        Assert.AreEqual(Vector3.one, mousePosition);
        Assert.AreEqual(5, axisX);
        Assert.AreEqual(5, axisY);
    }

    private void ActionCalled(int buttonid, Vector3 value)
    {
        //Assert
        Assert.AreEqual(1, buttonid);
        Assert.AreEqual(Vector3.one, value);
    }

    private void MouseWheel(float value)
    {
        //Assert
        Assert.AreEqual(5f, value);
    }

    protected override IEnumerator TearDown()
    {
        BIWInputWrapper.OnMouseClickOnUI -= ActionCalled;
        BIWInputWrapper.OnMouseWheel -= MouseWheel;
        BIWInputWrapper.OnMouseUpOnUI -= ActionCalled;
        BIWInputWrapper.OnMouseUp -= ActionCalled;
        BIWInputWrapper.OnMouseClick -= ActionCalled;
        BIWInputWrapper.OnMouseDown -= ActionCalled;
        BIWInputWrapper.OnMouseDrag -= OnMouseDragAssert;
        BIWInputWrapper.OnMouseDragRaw -= OnMouseDragAssert;

        inputWrapper.Dispose();
        context.Dispose();
        return base.TearDown();
    }
}