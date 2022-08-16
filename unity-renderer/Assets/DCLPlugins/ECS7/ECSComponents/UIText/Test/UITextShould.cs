using DCL.Controllers;
using DCL.ECS7.UI;
using DCL.ECSComponents;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;

namespace DCLPlugins.ECS7.ECSComponents.UIText.Test
{
    public class UITextShould
    {
          [Test]
        public void InsertDataIntoContainer()
        {
            // Arrange
            var dataContainer = Substitute.For<IUIDataContainer>();
            UITextComponentHandler componentHandler = new UITextComponentHandler(dataContainer);
            var model = CreateModel();
            var parcelScene = Substitute.For<IParcelScene>();
            var entity = Substitute.For<IDCLEntity>();

            // Act
            componentHandler.OnComponentModelUpdated(parcelScene, entity, model);

            // Assert
            dataContainer.Received(1).AddUIComponent(parcelScene, entity, model);
        }
        
        [Test]
        public void RemoveDataFromContainer()
        {
            // Arrange
            var dataContainer = Substitute.For<IUIDataContainer>();
            UITextComponentHandler componentHandler = new UITextComponentHandler(dataContainer);
            var parcelScene = Substitute.For<IParcelScene>();
            var entity = Substitute.For<IDCLEntity>();

            // Act
            componentHandler.OnComponentRemoved(parcelScene, entity);

            // Assert
            dataContainer.Received(1).RemoveUIText(parcelScene, entity);
        }
        
        [Test]
        public void SerializeCorrectly()
        {
            // Arrange
            PBUiText model = new PBUiText();
            byte[] byteArray;
            
            // Act
            byteArray = UITextSerializer.Serialize(model);

            // Assert
            Assert.IsNotNull(byteArray);
        }
        
        [Test]
        public void SerializeAndDeserialzeCorrectly()
        {
            // Arrange
            PBUiText model = CreateModel();

            // Act
            var newModel = SerializaAndDeserialize(model);
            
            // Assert
            Assert.AreEqual(model.Text, newModel.Text);
            Assert.AreEqual(model.TextColor, newModel.TextColor);
        }

        private PBUiText CreateModel()
        {
            var model = new PBUiText();
            model.Text = "Hello World<>'";
            model.TextColor = new Color3();
            model.TextColor.B = 1;
            model.TextColor.G = 1;
            model.TextColor.R = 1;
            return model;
        }
        
        private PBUiText SerializaAndDeserialize(PBUiText pb)
        {
            var result = UITextSerializer.Serialize(pb);

            return UITextSerializer.Deserialize(result);
        }
    }
}