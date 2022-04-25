using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;

public interface IResourceManagerService : IService
{
    /// <summary>
    /// Get a material based in a model, if it is created, it will return the already created material,
    /// If not, it will create one based on it
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    UniTask<Material> GetMaterial(MaterialModel model);
    
    /// <summary>
    /// Get a texture based in a model, if it is created, it will return the already created texture,
    /// If not, it will create one based on it
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    UniTask<Texture> GetDCLTexture(TextureModel model);
}

public class ResourceManagerService : IResourceManagerService
{
    public void Dispose() { }
    public void Initialize() { }
    
    public async UniTask<Material> GetMaterial(MaterialModel model)
    {
        Material material = null;
        AssetPromise_Material materialPromise = new AssetPromise_Material(model);
        materialPromise.OnSuccessEvent += (materialResult) =>   material = materialResult.material;
        materialPromise.OnFailEvent += (texture, error) =>
        {
            material = null;
            Debug.Log($"Unable to get a material {model} due to: "+error.Message);
        };
        AssetPromiseKeeper_Material.i.Keep(materialPromise);

        await materialPromise;

        return material;
    }
    
    public async UniTask<Texture> GetDCLTexture(TextureModel model)
    {
        Texture texture = null;
        AssetPromise_DCLTexture texturePromise = new AssetPromise_DCLTexture(model);
        texturePromise.OnSuccessEvent += (textureResult) =>   texture = textureResult.texture2D;
        texturePromise.OnFailEvent += (texture, error) =>
        {
            texture = null;
            Debug.Log($"Unable to get a material {model} due to: "+error.Message);
        };
        AssetPromiseKeeper_DCLTexture.i.Keep(texturePromise);

        await texturePromise;

        return texture;
    }

}
