using UnityEngine;

public class NFTShapeConfig : ScriptableObject
{
    public float loadingMinDistance = 30;
    public float hqImgMinDistance = 7;
    public float hqImgFacingDotProdMinValue = 0.4f;
    public float hqImgInFrontDotProdMinValue = 0.2f;
    public int hqImgResolution = 1024;
    public int hqGifResolution = 512;
    public bool verbose = false;

#if UNITY_EDITOR
    private void OnEnable()
    {
        Application.quitting -= CleanUp;
        Application.quitting += CleanUp;
    }

    private void CleanUp()
    {
        Application.quitting -= CleanUp;
        if (UnityEditor.AssetDatabase.Contains(this))
            Resources.UnloadAsset(this);
    }
#endif    
}
