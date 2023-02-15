using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Input;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace DCLServices.MapRendererV2.Tests.Input
{
    public class MapInputControllerShould
    {
        private MapInputController input;
        private ICoordsUtils coordsUtils;
        private List<Vector2Int> enterCalls;
        private List<Vector2Int> exitCalls;
        private List<Vector2Int> clickCalls;

        [SetUp]
        public void SetUp()
        {
            coordsUtils = Substitute.For<ICoordsUtils>();
            input = new MapInputController(coordsUtils);
            enterCalls = new List<Vector2Int>();
            exitCalls = new List<Vector2Int>();
            clickCalls = new List<Vector2Int>();
            input.OnParcelHoverEnter += (x) => enterCalls.Add(x);
            input.OnParcelHoverExit += (x) => exitCalls.Add(x);
            input.OnParcelClicked += (x) => clickCalls.Add(x);
        }

        [Test]
        public void InitializeItsMembers()
        {
            Assert.IsNull(input.CurrentHover);
            Assert.IsNull(input.CurrentPointerDown);
        }

        [Test]
        public void InvokeHoverEnterIfNoPreviousHoverWhenValidPos()
        {
            input.CurrentHover = null;
            coordsUtils.Configure().PositionToCoordsInWorld(Arg.Any<Vector3>()).Returns(Vector2Int.one);

            input.Hover(new Vector2(-15, -15)); //arbitrary worldpos

            Assert.AreEqual(0, exitCalls.Count);
            Assert.AreEqual(1, enterCalls.Count);
            Assert.AreEqual(Vector2Int.one, enterCalls[0]);
        }

        [Test]
        public void NotInvokeHoverEnterIfNoPreviousHoverWhenInvalidPos()
        {
            input.CurrentHover = null;
            coordsUtils.Configure().PositionToCoordsInWorld(Arg.Any<Vector3>()).Returns(_ => null);

            input.Hover(new Vector2(-15, -15)); //arbitrary worldpos

            Assert.AreEqual(0, exitCalls.Count);
            Assert.AreEqual(0, enterCalls.Count);
        }

        [Test]
        public void InvokeHoverEnterAndExitIfNewHoverIsDistinct()
        {
            input.CurrentHover = Vector2Int.left;
            coordsUtils.Configure().PositionToCoordsInWorld(Arg.Any<Vector3>()).Returns(_ => Vector2Int.right);

            input.Hover(new Vector2(-15, -15)); //arbitrary worldpos

            Assert.AreEqual(1, exitCalls.Count);
            Assert.AreEqual(Vector2Int.left, exitCalls[0]);
            Assert.AreEqual(1, enterCalls.Count);
            Assert.AreEqual(Vector2Int.right, enterCalls[0]);
        }

        [Test]
        public void NotInvokeHoverEnterOrExitWhenAlreadyHovering()
        {
            input.CurrentHover = Vector2Int.left;
            coordsUtils.Configure().PositionToCoordsInWorld(Arg.Any<Vector3>()).Returns(_ => Vector2Int.left);

            input.Hover(new Vector2(-15, -15)); //arbitrary worldpos

            Assert.AreEqual(0, exitCalls.Count);
            Assert.AreEqual(0, enterCalls.Count);
        }

        [Test]
        public void InvokeHoverExitIfNewHoverIsInvalid()
        {
            input.CurrentHover = Vector2Int.left;
            coordsUtils.Configure().PositionToCoordsInWorld(Arg.Any<Vector3>()).Returns(_ => null);

            input.Hover(new Vector2(-15, -15)); //arbitrary worldpos

            Assert.AreEqual(1, exitCalls.Count);
            Assert.AreEqual(Vector2Int.left, exitCalls[0]);
            Assert.AreEqual(0, enterCalls.Count);
        }

        [Test]
        public void NotInvokeClickedWhenNoPreviouslyPointerDownAndValidPointerUp()
        {
            input.CurrentPointerDown = null;
            coordsUtils.Configure().PositionToCoordsInWorld(Arg.Any<Vector3>()).Returns(_ => Vector2Int.left);

            input.PointerUp(new Vector2(-15, -15)); //Arbitrary worldpos

            Assert.AreEqual(0, clickCalls.Count);
        }

        [Test]
        public void NotInvokeClickedWhenNoPreviouslyPointerDownAndInvalidPointerUp()
        {
            input.CurrentPointerDown = null;
            coordsUtils.Configure().PositionToCoordsInWorld(Arg.Any<Vector3>()).Returns(_ => null);

            input.PointerUp(new Vector2(-15, -15)); //Arbitrary worldpos

            Assert.AreEqual(0, clickCalls.Count);
        }

        [Test]
        public void NotInvokeClickedWhenPreviouslyPointerDownAndInvalidPointerUp()
        {
            input.CurrentPointerDown = Vector2Int.left;
            coordsUtils.Configure().PositionToCoordsInWorld(Arg.Any<Vector3>()).Returns(_ => null);

            input.PointerUp(new Vector2(-15, -15)); //Arbitrary worldpos

            Assert.AreEqual(0, clickCalls.Count);
            Assert.AreEqual(null, input.CurrentPointerDown);
        }

        [Test]
        public void NotInvokeClickedWhenPreviouslyPointerDownAndDifferentPointerUp()
        {
            input.CurrentPointerDown = Vector2Int.left;
            coordsUtils.Configure().PositionToCoordsInWorld(Arg.Any<Vector3>()).Returns(_ => Vector2Int.right);

            input.PointerUp(new Vector2(-15, -15)); //Arbitrary worldpos

            Assert.AreEqual(0, clickCalls.Count);
            Assert.AreEqual(null, input.CurrentPointerDown);
        }

        [Test]
        public void InvokeClickedWhenPreviouslyPointerDownAndSamePointerUp()
        {
            input.CurrentPointerDown = Vector2Int.left;
            coordsUtils.Configure().PositionToCoordsInWorld(Arg.Any<Vector3>()).Returns(_ => Vector2Int.left);

            input.PointerUp(new Vector2(-15, -15)); //Arbitrary worldpos

            Assert.AreEqual(1, clickCalls.Count);
            Assert.AreEqual(Vector2Int.left, clickCalls[0]);
            Assert.AreEqual(null, input.CurrentPointerDown);
        }

        [Test]
        public void RegisterValidPointerDown()
        {
            input.CurrentPointerDown = null;
            coordsUtils.Configure().PositionToCoordsInWorld(Arg.Any<Vector3>()).Returns(_ => Vector2Int.left);

            input.PointerDown(new Vector2(-15, -15)); //Arbitrary worldpos

            Assert.AreEqual(Vector2Int.left, input.CurrentPointerDown);
        }

        [Test]
        public void NullifyCurrentPointerDownWhenInvalidPointerDown()
        {
            input.CurrentPointerDown = Vector2Int.right;
            coordsUtils.Configure().PositionToCoordsInWorld(Arg.Any<Vector3>()).Returns(_ => null);

            input.PointerDown(new Vector2(-15, -15)); //Arbitrary worldpos

            Assert.AreEqual(null, input.CurrentPointerDown);
        }
    }
}
