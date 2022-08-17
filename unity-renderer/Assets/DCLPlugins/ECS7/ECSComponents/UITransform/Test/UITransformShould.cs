using System.IO;
using DCL.Controllers;
using DCL.ECS7.UI;
using DCL.Models;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace DCL.ECSComponents.Test
{
    public class ECSUITransformShould
    {
        [Test]
        public void InsertDataIntoContainer()
        {
            // Arrange
            var dataContainer = Substitute.For<IUIDataContainer>();
            UITransformComponentHandler transformComponentHandler = new UITransformComponentHandler(dataContainer);
            var model = CreateModel();
            var parcelScene = Substitute.For<IParcelScene>();
            var entity = Substitute.For<IDCLEntity>();

            // Act
            transformComponentHandler.OnComponentModelUpdated(parcelScene, entity, model );

            // Assert
            dataContainer.Received(1).AddUIComponent(parcelScene, entity, model);
        }
        
        [Test]
        public void RemoveDataFromContainer()
        {
            // Arrange
            var dataContainer = Substitute.For<IUIDataContainer>();
            UITransformComponentHandler transformComponentHandler = new UITransformComponentHandler(dataContainer);
            var parcelScene = Substitute.For<IParcelScene>();
            var entity = Substitute.For<IDCLEntity>();
            entity.parentId = 123;

            // Act
            transformComponentHandler.OnComponentRemoved(parcelScene, entity);

            // Assert
            dataContainer.Received(1).RemoveUITransform(parcelScene, entity);
            entity.Received(1).parentId = SpecialEntityId.SCENE_ROOT_ENTITY;
        }
        
        [Test]
        public void SerializeCorrectly()
        {
            // Arrange
            PBUiTransform model = new PBUiTransform();
            byte[] byteArray;
            
            // Act
            byteArray = UITransformSerializer.Serialize(model);

            // Assert
            Assert.IsNotNull(byteArray);
        }
        
        [Test]
        public void SerializeAndDeserialzeCorrectly()
        {
            // Arrange
            PBUiTransform model = CreateModel();

            // Act
            var newModel = SerializaAndDeserialize(model);
            
            // Assert
            Assert.AreEqual(model.Parent, newModel.Parent);
            Assert.AreEqual(model.Direction, newModel.Direction);
            Assert.AreEqual(model.Display, newModel.Display);
            Assert.AreEqual(model.Overflow, newModel.Overflow);
            Assert.AreEqual(model.AlignContent, newModel.AlignContent);
            Assert.AreEqual(model.Flex, newModel.Flex);
            Assert.AreEqual(model.JustifyContent, newModel.JustifyContent);
            Assert.AreEqual(model.HeightUnit, newModel.HeightUnit);
        }

        private PBUiTransform CreateModel()
        {
            var model = new PBUiTransform();
            model.Parent = 123;
            model.Direction = YGDirection.Inherit;
            model.Display = YGDisplay.Flex;
            model.Overflow = YGOverflow.Hidden;
            model.AlignContent = YGAlign.Auto;
            model.Flex = 3;
            model.JustifyContent = YGJustify.Center;
            model.HeightUnit = YGUnit.Auto;
            return model;
        }
        
        private PBUiTransform SerializaAndDeserialize(PBUiTransform pb)
        {
            var result = UITransformSerializer.Serialize(pb);

            return UITransformSerializer.Deserialize(result);
        }
    }
}
