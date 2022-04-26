using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
    UniTask<Material> GetMaterial(MaterialModel model, CancellationToken ct = default);
    
    /// <summary>
    /// Get a texture based in a model, if it is created, it will return the already created texture,
    /// If not, it will create one based on it
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    UniTask<Texture> GetDCLTexture(TextureModel model, CancellationToken ct = default);
}

public class ResourceManagerService : IResourceManagerService
{
    CancellationTokenSource materialCts;
    CancellationTokenSource textureCts;

    public void Initialize()
    {
        materialCts = new CancellationTokenSource();
        textureCts = new CancellationTokenSource();
    }
    
    public void Dispose()
    {
        textureCts?.Cancel();
        textureCts?.Dispose();
        
        materialCts?.Cancel();
        materialCts?.Dispose();
    }

    public async UniTask<Material> GetMaterial(MaterialModel model, CancellationToken ct = default)
    {
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, materialCts.Token);

        linkedCts.Token.ThrowIfCancellationRequested();

        Material material = null;
        AssetPromise_Material materialPromise = new AssetPromise_Material(model);
        materialPromise.OnSuccessEvent += (materialResult) =>   material = materialResult.material;
        materialPromise.OnFailEvent += (texture, error) =>
        {
            material = null;
            Debug.Log($"Unable to get a material {model} due to: " + error.Message);
        };
        AssetPromiseKeeper_Material.i.Keep(materialPromise);
        
        try
        {
            await materialPromise.WithCancellation(linkedCts.Token).AttachExternalCancellation(linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            //No disposing required
            throw;
        }

        return material;
    }

    public async UniTask<Texture> GetDCLTexture(TextureModel model, CancellationToken ct = default)
    {
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, textureCts.Token);

        linkedCts.Token.ThrowIfCancellationRequested();
        Texture texture = null;
        AssetPromise_DCLTexture texturePromise = new AssetPromise_DCLTexture(model);
        texturePromise.OnSuccessEvent += (textureResult) =>   texture = textureResult.texture2D;
        texturePromise.OnFailEvent += (texture, error) =>
        {
            texture = null;
            Debug.Log($"Unable to get a material {model} due to: "+error.Message);
        };
        AssetPromiseKeeper_DCLTexture.i.Keep(texturePromise);

        try
        {
            await texturePromise.WithCancellation(linkedCts.Token).AttachExternalCancellation(linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            //No disposing required
            throw;
        }

        return texture;
    }

}
