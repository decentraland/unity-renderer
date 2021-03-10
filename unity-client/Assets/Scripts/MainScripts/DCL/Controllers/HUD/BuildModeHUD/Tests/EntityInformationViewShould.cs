using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class EntityInformationViewShould
    {
        private EntityInformationView entityInformationView;

        [SetUp]
        public void SetUp()
        {
            entityInformationView = EntityInformationView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(entityInformationView.gameObject);
        }

        [Test]
        public void SetCurrentEntityCorrectly()
        {
            // Arrange
            DCLBuilderInWorldEntity newEntity = new GameObject("_DCLBuilderInWorldEntity").AddComponent<DCLBuilderInWorldEntity>();
            newEntity.entityUniqueId = "testId";
            entityInformationView.currentEntity = null;

            // Act
            entityInformationView.SetCurrentEntity(newEntity);

            // Assert
            Assert.AreEqual(newEntity.entityUniqueId, entityInformationView.currentEntity.entityUniqueId, "The entity id does not match!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ToggleDetailsInfoCorrectly(bool isActive)
        {
            // Arrange
            entityInformationView.detailsGO.SetActive(!isActive);
            entityInformationView.detailsToggleBtn.sprite = null;

            // Act
            entityInformationView.ToggleDetailsInfo();

            // Assert
            Assert.AreEqual(isActive, entityInformationView.detailsGO.activeSelf, "The details game object activation property does not match!");
            Assert.AreEqual(isActive ? entityInformationView.openMenuSprite : entityInformationView.closeMenuSprite, entityInformationView.detailsToggleBtn.sprite, "The details sprite does not match!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ToggleBasicInfoCorrectly(bool isActive)
        {
            // Arrange
            entityInformationView.basicsGO.SetActive(!isActive);
            entityInformationView.basicToggleBtn.sprite = null;

            // Act
            entityInformationView.ToggleBasicInfo();

            // Assert
            Assert.AreEqual(isActive, entityInformationView.basicsGO.activeSelf, "The basics game object activation property does not match!");
            Assert.AreEqual(isActive ? entityInformationView.openMenuSprite : entityInformationView.closeMenuSprite, entityInformationView.basicToggleBtn.sprite, "The basics sprite does not match!");
        }

        [Test]
        public void StartChangingNameCorrectly()
        {
            // Arrange
            bool changingNameStarted = false;
            entityInformationView.OnStartChangingName += () => changingNameStarted = true;

            // Act
            entityInformationView.StartChangingName();

            // Assert
            Assert.IsTrue(changingNameStarted, "changingNameStarted is false!");
        }

        [Test]
        public void EndChangingNameCorrectly()
        {
            // Arrange
            bool changingNameEndeed = false;
            entityInformationView.OnEndChangingName += () => changingNameEndeed = true;

            // Act
            entityInformationView.EndChangingName();

            // Assert
            Assert.IsTrue(changingNameEndeed, "changingNameEndeed is false!");
        }

        [Test]
        public void ChangeEntityNameCorrectly()
        {
            // Arrange
            string newEntityName = "Test name";
            DCLBuilderInWorldEntity testEntity = new GameObject("_DCLBuilderInWorldEntity").AddComponent<DCLBuilderInWorldEntity>();
            testEntity.entityUniqueId = "testId";
            entityInformationView.currentEntity = testEntity;

            DCLBuilderInWorldEntity entity = null;
            string entityName = "";
            entityInformationView.OnNameChange += (changedEntity, name) =>
            {
                entity = changedEntity;
                entityName = name;
            };

            // Act
            entityInformationView.ChangeEntityName(newEntityName);

            // Assert
            Assert.AreEqual(entityInformationView.currentEntity, entity, "The current entity does not mach!");
            Assert.AreEqual(newEntityName, entityName, "The entity name does not match!");
        }

        [Test]
        public void DisableCorrectly()
        {
            // Arrange
            bool isDisabled = false;
            entityInformationView.OnDisable += () => isDisabled = true;

            // Act
            entityInformationView.Disable();

            // Assert
            Assert.IsTrue(isDisabled, "isDisabled is false!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetEntityThumbnailEnableCorrectly(bool isEnable)
        {
            if (entityInformationView.entitytTumbailImg == null)
                return;

            // Arrange
            entityInformationView.entitytTumbailImg.enabled = !isEnable;

            // Act
            entityInformationView.SetEntityThumbnailEnable(isEnable);

            // Assert
            Assert.AreEqual(isEnable, entityInformationView.entitytTumbailImg.enabled, "Entity Thumbnail enable property does not match!");
        }

        [Test]
        public void SetEntityThumbnailTextureCorrectly()
        {
            if (entityInformationView.entitytTumbailImg == null)
                return;

            // Arrange
            Texture2D newTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            entityInformationView.entitytTumbailImg.texture = null;

            // Act
            entityInformationView.SetEntityThumbnailTexture(newTexture);

            // Assert
            Assert.AreEqual(newTexture, entityInformationView.entitytTumbailImg.texture, "Entity Thumbnail texture does not match!");
        }

        [Test]
        public void SetTitleTextCorrectly()
        {
            // Arrange
            string newText = "Test text";
            entityInformationView.titleTxt.text = "";

            // Act
            entityInformationView.SeTitleText(newText);

            // Assert
            Assert.AreEqual(newText, entityInformationView.titleTxt.text, "The title text does not match!");
        }

        [Test]
        public void SeEntityLimitsLeftTextCorrectly()
        {
            // Arrange
            string newText = "Test text";
            entityInformationView.entityLimitsLeftTxt.text = "";

            // Act
            entityInformationView.SeEntityLimitsLeftText(newText);

            // Assert
            Assert.AreEqual(newText, entityInformationView.entityLimitsLeftTxt.text, "The left limits text does not match!");
        }

        [Test]
        public void SetEntityLimitsRightTextTextCorrectly()
        {
            // Arrange
            string newText = "Test text";
            entityInformationView.entityLimitsRightTxt.text = "";

            // Act
            entityInformationView.SeEntityLimitsRightText(newText);

            // Assert
            Assert.AreEqual(newText, entityInformationView.entityLimitsRightTxt.text, "The right limits text does not match!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetSmartItemListViewActiveCorrectly(bool isActive)
        {
            // Arrange
            entityInformationView.smartItemListView.gameObject.SetActive(!isActive);

            // Act
            entityInformationView.SetSmartItemListViewActive(isActive);

            // Assert
            Assert.AreEqual(isActive, entityInformationView.smartItemListView.gameObject.activeSelf, "The smartItemList active property does not match!");
        }

        [Test]
        public void SetNameIFTextCorrectly()
        {
            // Arrange
            string newText = "Test text";
            entityInformationView.nameIF.text = "";

            // Act
            entityInformationView.SetNameIFText(newText);

            // Assert
            Assert.AreEqual(newText, entityInformationView.nameIF.text, "The nameIF text does not match!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Arrange
            entityInformationView.gameObject.SetActive(!isActive);

            // Act
            entityInformationView.SetActive(isActive);

            // Assert
            Assert.AreEqual(isActive, entityInformationView.gameObject.activeSelf, "The active property does not match!");
        }
    }
}
