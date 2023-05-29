using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using DCL.Helpers;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.TestTools;

namespace DCL.DummyTest.Test
{
    public class DummyTestClass01
    {

        [SetUp]
        public void SetUp()
        {
        }

        // [Category("EditModeCI")]
        [Test]
        public void DummyTest01()
        {
            //Arrange

            //Act

            //Assert
            Assert.IsTrue(true);
        }
    }
}
