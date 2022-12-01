using DCL;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class CharacterPreviewFactory : ICharacterPreviewFactory
{
    private static readonly Vector3 COORDS_TO_START = new (0, 50, 0);
    private static readonly Vector3 VECTOR_BETWEEN_INSTANCES = new (3, 0, 3);

    private Pool pool;

    private int controllersCount = 0;

    private CharacterPreviewController prefab;

    public ICharacterPreviewController Create(CharacterPreviewMode loadingMode, RenderTexture renderTexture, bool isVisible)
    {
        var instance = Object.Instantiate(prefab);
        instance.transform.position = COORDS_TO_START + (VECTOR_BETWEEN_INSTANCES * controllersCount);

        var characterPreviewController = instance.gameObject.GetComponent<CharacterPreviewController>();

        characterPreviewController.Initialize(loadingMode, renderTexture);
        characterPreviewController.SetEnabled(isVisible);

        controllersCount++;

        return characterPreviewController;
    }

    void IDisposable.Dispose()
    {
        pool.Cleanup();
    }

    void IService.Initialize()
    {
        prefab = Resources.Load<CharacterPreviewController>("CharacterPreview");
    }
}
