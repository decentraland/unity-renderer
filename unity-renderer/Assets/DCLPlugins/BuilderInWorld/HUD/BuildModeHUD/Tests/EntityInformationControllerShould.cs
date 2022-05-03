using System.Collections;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tests.BuildModeHUDControllers
{
    /// <summary>
    /// TODO: This is using IntegrationTestSuite_Legacy instead of the normal because there is a bug in the NSustitute library
    /// where the IDCLEntity are not mocked correctly, when you try to use Substitute.For<IDCLEntity>() there is a null reference in the variable pointer to an exception in the castle library
    /// After it is fixed, we should go to IntegrationTestSuite 
    /// </summary>
    public class EntityInformationControllerShould : IntegrationTestSuite_Legacy
    {
        private EntityInformationController entityInformationController;

        protected override IEnumerator SetUp()
        {
            entityInformationController = new EntityInformationController();
            entityInformationController.Initialize(Substitute.For<IEntityInformationView>());
            yield return base.SetUp();
        }

        protected override IEnumerator TearDown()
        {
            entityInformationController.Dispose();
            yield return base.TearDown();
        }

        [Test]
        public void PositionChangedCorrectly()
        {
            // Arrange
            Vector3 testPos = new Vector3(5, 7, 0);
            Vector3 returnedPos = Vector3.zero;
            entityInformationController.OnPositionChange += (pos) => { returnedPos = pos; };

            // Act
            entityInformationController.PositionChanged(testPos);

            // Assert
            Assert.AreEqual(testPos, returnedPos, "The position does not match!");
        }

        [Test]
        public void RotationChangedCorrectly()
        {
            // Arrange
            Vector3 testRot = new Vector3(5, 7, 0);
            Vector3 returnedRot = Vector3.zero;
            entityInformationController.OnRotationChange += (rot) => { returnedRot = rot; };

            // Act
            entityInformationController.RotationChanged(testRot);

            // Assert
            Assert.AreEqual(testRot, returnedRot, "The rotation does not match!");
        }

        [Test]
        public void ScaleChangedCorrectly()
        {
            // Arrange
            Vector3 testScale = new Vector3(5, 7, 0);
            Vector3 returnedScale = Vector3.zero;
            entityInformationController.OnScaleChange += (scale) => { returnedScale = scale; };

            // Act
            entityInformationController.ScaleChanged(testScale);

            // Assert
            Assert.AreEqual(testScale, returnedScale, "The scale does not match!");
        }

        [Test]
        public void NameChangedCorrectly()
        {
            // Arrange
            BIWEntity testEntity = new BIWEntity();
            string testText = "Test text";
            BIWEntity returnedEntity = null;
            string returnedText = "";
            entityInformationController.OnNameChange += (entity, name) =>
            {
                returnedEntity = entity;
                returnedText = name;
            };

            // Act
            entityInformationController.NameChanged(testEntity, testText);

            // Assert
            Assert.AreEqual(testEntity, returnedEntity, "The entity does not match!");
            Assert.AreEqual(testText, returnedText, "The text does not match!");
        }

        [Test]
        public void ToggleDetailsInfoCorrectly()
        {
            // Act
            entityInformationController.ToggleDetailsInfo();

            // Assert
            entityInformationController.entityInformationView.Received(1).ToggleDetailsInfo();
        }

        [Test]
        public void ToggleBasicInfoCorrectly()
        {
            // Act
            entityInformationController.ToggleBasicInfo();

            // Assert
            entityInformationController.entityInformationView.Received(1).ToggleBasicInfo();
        }

        [Test]
        public void StartChangingNameCorrectly()
        {
            // Arrange
            entityInformationController.isChangingName = false;

            // Act
            entityInformationController.StartChangingName();

            // Assert
            Assert.IsTrue(entityInformationController.isChangingName, "isChangingName is false!");
        }

        [Test]
        public void EndChangingNameCorrectly()
        {
            // Arrange
            entityInformationController.isChangingName = true;

            // Act
            entityInformationController.EndChangingName();

            // Assert
            Assert.IsFalse(entityInformationController.isChangingName, "isChangingName is true!");
        }

        [Test]
        public void SetEntityCorrectly()
        {
            // Arrange
            BIWEntity testEntity = new BIWEntity();
            IParcelScene testScene2 = Substitute.For<IParcelScene>();

            // Act
            entityInformationController.SetEntity(testEntity, testScene2);

            // Assert
            entityInformationController.entityInformationView.Received(1).SetCurrentEntity(testEntity);
            Assert.AreEqual(testScene2, entityInformationController.parcelScene, "The parcel scene does not match!");
            entityInformationController.entityInformationView.Received(1).SetEntityThumbnailEnable(false);
        }

        [Test]
        public void GetThumbnailCorrectly()
        {
            // Arrange
            CatalogItem testCatalogItem = new CatalogItem();
            testCatalogItem.thumbnailURL = "test url";
            entityInformationController.loadedThumbnailPromise = null;

            // Act
            entityInformationController.GetThumbnail(testCatalogItem);

            // Assert
            Assert.IsNotNull(entityInformationController.loadedThumbnailPromise, "loadedThumbnailPromise is null!");
        }

        [Test]
        public void SetThumbnailCorrectly()
        {
            // Arrange
            Asset_Texture testTexture = new Asset_Texture();
            testTexture.texture = new Texture2D(20, 20);

            // Act
            entityInformationController.SetThumbnail(testTexture);

            // Assert
            entityInformationController.entityInformationView.Received(1).SetEntityThumbnailEnable(true);
            entityInformationController.entityInformationView.Received(1).SetEntityThumbnailTexture(testTexture.texture);
        }

        [Test]
        public void UpdateEntityNameCorrectly()
        {
            // Arrange
            BIWEntity testEntity = new BIWEntity();
            entityInformationController.isChangingName = false;

            // Act
            entityInformationController.UpdateEntityName(testEntity);

            // Assert
            entityInformationController.entityInformationView.Received(1).SetNameIFText(Arg.Any<string>());
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void UpdateLimitsInformationCorrectly(bool isCatalogNull)
        {
            // Arrange
            CatalogItem testCatalogItem = null;
            if (!isCatalogNull)
            {
                testCatalogItem = new CatalogItem();
                testCatalogItem.metrics = new SceneObject.ObjectMetrics
                {
                    entities = 5,
                    bodies = 3,
                    triangles = 10,
                    textures = 2,
                    materials = 2,
                    meshes = 6
                };
            }

            // Act
            entityInformationController.UpdateLimitsInformation(testCatalogItem);

            // Assert
            entityInformationController.entityInformationView.Received(1).SeEntityLimitsText(isCatalogNull ? "" : Arg.Any<string>(), isCatalogNull ? "" : Arg.Any<string>(), isCatalogNull ? "" : Arg.Any<string>());
        }

        [Test]
        public void SetEnableCorrectly()
        {
            // Arrange
            entityInformationController.entityInformationView.SetActive(false);

            // Act
            entityInformationController.Enable();

            // Assert
            entityInformationController.entityInformationView.Received(1).SetActive(true);
        }

        [Test]
        public void SetDisableCorrectly()
        {
            // Arrange
            bool hidden = false;
            entityInformationController.entityInformationView.SetActive(true);

            entityInformationController.OnDisable += () =>
            {
                hidden = true;
            };

            // Act
            entityInformationController.Disable();

            // Assert
            entityInformationController.entityInformationView.Received(1).SetActive(false);
            entityInformationController.entityInformationView.Received(1).SetCurrentEntity(null);
            Assert.IsTrue(hidden);
        }

        [Test]
        public void UpdateInfoCorrectly()
        {
            // Arrange
            BIWEntity testEntity = new BIWEntity();

            ParcelScene scene = TestUtils.CreateTestScene();
            var entity = TestUtils.CreateSceneEntity(scene, 1);
            testEntity.Initialize(entity, null);

            // Act
            entityInformationController.UpdateInfo(testEntity);

            // Assert
            entityInformationController.entityInformationView.Received(1).SetPositionAttribute(Arg.Any<Vector3>());
            entityInformationController.entityInformationView.Received(1).SetRotationAttribute(Arg.Any<Vector3>());
            entityInformationController.entityInformationView.Received(1).SetScaleAttribute(Arg.Any<Vector3>());
        }

        [Test]
        [TestCase(1)]
        [TestCase(5)]
        public void UpdateEntitiesSelectionCorrectly(int numberOfSelectedEntities)
        {
            // Act
            entityInformationController.UpdateEntitiesSelection(numberOfSelectedEntities);

            // Assert
            entityInformationController.entityInformationView.Received(1).UpdateEntitiesSelection(numberOfSelectedEntities);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetTransparencyModeCorrectly(bool isOn)
        {
            // Act
            entityInformationController.SetTransparencyMode(isOn);

            // Assert
            entityInformationController.entityInformationView.Received(1).SetTransparencyMode(Arg.Any<float>(), !isOn);
        }
    }
}