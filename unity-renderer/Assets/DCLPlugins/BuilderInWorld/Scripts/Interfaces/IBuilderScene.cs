using DCL.Controllers;
using UnityEngine;

namespace DCL.Builder
{
    public interface IBuilderScene
    {
        /// <summary>
        /// This enum represent the possibles scenes types supported in the builder
        /// </summary>
        public enum SceneType
        {
            PROJECT = 0,
            LAND = 1
        }
        
        /// <summary>
        /// This enum represent the version of the scene, legacy is the old ECS while ECS is the new runtime
        /// </summary>
        public enum SceneVersion
        {
            LEGACY = 0,
            ECS = 1
        }

        /// <summary>
        /// Current manifest of the scene, the manifest is used to save the project
        /// </summary>
        Manifest.IManifest manifest { get; }

        /// <summary>
        /// Current screenshot of the project
        /// </summary>
        Texture2D sceneScreenshotTexture { get; set; }

        /// <summary>
        /// Aerial screenshot of the project
        /// </summary>
        Texture2D aerialScreenshotTexture { get; set; }

        /// <summary>
        /// Parcel scene associated with the scene
        /// </summary>
        IParcelScene scene { get; }
        
        /// <summary>
        /// Version of the scene, legacy or new ECS
        /// </summary>
        SceneVersion sceneVersion { get; }

        /// <summary>
        /// Type of scene 
        /// </summary>
        SceneType sceneType { get; }
        
        /// <summary>
        /// If there is a land associated to the project, this will be the coords associated to them
        /// </summary>
        Vector2Int landCoordsAsociated { get; set; }

        /// <summary>
        /// This will update the manifest with the current state of the parcelScene
        /// </summary>
        void UpdateManifestFromScene();

        /// <summary>
        /// This will override the current scene of the builder
        /// </summary>
        /// <param name="scene"></param>
        void SetScene(IParcelScene scene);

        /// <summary>
        /// This will return true is the scene has been created this session
        /// </summary>
        /// <returns></returns>
        bool HasBeenCreatedThisSession();
    }
}