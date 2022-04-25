using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DCL;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class DCLResourcesShould : IntegrationTestSuite
{
    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        base.InitializeServices(serviceLocator);
        serviceLocator.Register<IResourceManagerService>(() => new ResourceManagerService());
    }
    
    [UnityTest]
    public IEnumerator GiveTheSameMaterial()
    {
        yield return Task().ToCoroutine();
        async UniTask Task()
        {
            //Arrange
            var newMaterialModel = new MaterialModel
            {
                 albedoTexture = CreateTextureModel(),
                metallic = 0,
                roughness = 1,
            };
        
            var newMaterialModel2 = new MaterialModel
            {
                 albedoTexture = CreateTextureModel(),
                metallic = 0,
                roughness = 1,
            };
        
            //Act
            var material = await Environment.i.serviceLocator.Get<IResourceManagerService>().GetMaterial(newMaterialModel);
            var material2 = await Environment.i.serviceLocator.Get<IResourceManagerService>().GetMaterial(newMaterialModel2);

            //Assert
            Assert.AreEqual(material,material2);
        }
    }
    
    [UnityTest]
    public IEnumerator GiveDifferentsMaterials()
    {
        yield return Task().ToCoroutine();
        async UniTask Task()
        {
            //Arrange
            var newMaterialModel = new MaterialModel
            {
                albedoTexture = CreateTextureModel(),
                metallic = 0,
                roughness = 0,
            };
        
            var newMaterialModel2 = new MaterialModel
            {
                albedoTexture = CreateTextureModel(),
                metallic = 0,
                roughness = 1,
            };
        
            //Act
            var material = await Environment.i.serviceLocator.Get<IResourceManagerService>().GetMaterial(newMaterialModel);
            var material2 = await Environment.i.serviceLocator.Get<IResourceManagerService>().GetMaterial(newMaterialModel2);

            //Assert
            Assert.AreNotEqual(material,material2);
        }
    }
    
    [UnityTest]
    public IEnumerator GiveTheSameTexture()
    {
        yield return Task().ToCoroutine();
        async UniTask Task()
        {
            //Arrange
            var newTextureModel = CreateTextureModel();
            
            var newTextureModel2 = CreateTextureModel();
        
            //Act
            var dclTexture = await Environment.i.serviceLocator.Get<IResourceManagerService>().GetDCLTexture(newTextureModel);
            var dclTexture2 = await Environment.i.serviceLocator.Get<IResourceManagerService>().GetDCLTexture(newTextureModel2);

            //Assert
            Assert.AreEqual(dclTexture,dclTexture2);
        }
    }
    
    [UnityTest]
    public IEnumerator GiveDifferentsTexture()
    {
        yield return Task().ToCoroutine();
        async UniTask Task()
        {
            //Arrange
            var textureModel = CreateTextureModel();
            
            var textureModel2 = CreateTextureModel();
            textureModel2.wrap = TextureModel.BabylonWrapMode.MIRROR;
        
            //Act
            var texture = await Environment.i.serviceLocator.Get<IResourceManagerService>().GetDCLTexture(textureModel);
            var texture2 = await Environment.i.serviceLocator.Get<IResourceManagerService>().GetDCLTexture(textureModel2);

            //Assert
            Assert.AreNotEqual(texture,texture2);
        }
    }
    
    private TextureModel CreateTextureModel()
    {
        return new TextureModel
        {
            src = "ThisUrl",
            wrap = TextureModel.BabylonWrapMode.WRAP,
            samplingMode = FilterMode.Bilinear,
        };
    }
}
