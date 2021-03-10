using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDControllers
{
    public class EntityInformationControllerShould
    {
        private EntityInformationController entityInformationController;

        [SetUp]
        public void SetUp()
        {
            entityInformationController = new EntityInformationController();
            entityInformationController.Initialize(Substitute.For<IEntityInformationView>());
        }

        [TearDown]
        public void TearDown()
        {
            entityInformationController.Dispose();
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
            DCLBuilderInWorldEntity testEntity = new GameObject("_DCLBuilderInWorldEntity").AddComponent<DCLBuilderInWorldEntity>();
            string testText = "Test text";
            DCLBuilderInWorldEntity returnedEntity = null;
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
            DCLBuilderInWorldEntity testEntity = new GameObject("_DCLBuilderInWorldEntity").AddComponent<DCLBuilderInWorldEntity>();
            ParcelScene testScene = new GameObject("_ParcelScene").AddComponent<ParcelScene>();

            // Act
            entityInformationController.SetEntity(testEntity, testScene);

            // Assert
            entityInformationController.entityInformationView.Received(1).SetCurrentEntity(testEntity);
            Assert.AreEqual(testScene, entityInformationController.parcelScene, "The parcel scene does not match!");
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
            DCLBuilderInWorldEntity testEntity = new GameObject("_DCLBuilderInWorldEntity").AddComponent<DCLBuilderInWorldEntity>();
            entityInformationController.isChangingName = false;

            // Act
            entityInformationController.UpdateEntityName(testEntity);

            // Assert
            entityInformationController.entityInformationView.Received(1).SeTitleText(Arg.Any<string>());
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
            entityInformationController.entityInformationView.Received(1).SeEntityLimitsLeftText(isCatalogNull ? "" : Arg.Any<string>());
            entityInformationController.entityInformationView.Received(1).SeEntityLimitsRightText(isCatalogNull ? "" : Arg.Any<string>());
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
            entityInformationController.entityInformationView.SetActive(true);

            // Act
            entityInformationController.Disable();

            // Assert
            entityInformationController.entityInformationView.Received(1).SetCurrentEntity(null);
        }

        [Test]
        public void UpdateInfoCorrectly()
        {
            // Arrange
            DCLBuilderInWorldEntity testEntity = new GameObject("_DCLBuilderInWorldEntity").AddComponent<DCLBuilderInWorldEntity>();

            // Act
            entityInformationController.UpdateInfo(testEntity);

            // Assert
            entityInformationController.entityInformationView.Received(1).SetPositionAttribute(Arg.Any<Vector3>());
            entityInformationController.entityInformationView.Received(1).SetRotationAttribute(Arg.Any<Vector3>());
            entityInformationController.entityInformationView.Received(1).SetScaleAttribute(Arg.Any<Vector3>());
        }
    }
}
