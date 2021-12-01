using System;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using DCL.Builder.Manifest;
using DCL.Helpers;
using UnityEngine;

public interface IBuilderAPIController
{
    /// <summary>
    /// When a WebRequest is created it will call this action
    /// </summary>
    event Action<IWebRequestAsyncOperation> OnWebRequestCreated;
    
    /// <summary>
    /// Init the builderAPi
    /// </summary>
    /// <param name="context"></param>
    void Initialize(IContext context);
    
    /// <summary>
    /// Get the catalog of builder with 2 calls, the default one and the specific ethAddress 
    /// </summary>
    /// <param name="ethAddres"></param>
    /// <returns></returns>
    Promise<bool> GetCompleteCatalog(string ethAddres);

    /// <summary>
    /// Get all the projects from an user
    /// </summary>
    /// <returns></returns>
    Promise<List<ProjectData>> GetAllManifests();
    
    /// <summary>
    /// This will get the manifest from the project data
    /// </summary>
    /// <param name="id">Id that we will use to recover the manifest associated</param>
    /// <returns></returns>
    Promise<Manifest> GetManifestById(string id);  
    
    /// <summary>
    /// Set the manifest in the builder API, this will save the manifest
    /// </summary>
    /// <param name="manifest"></param>
    /// <returns></returns>
    Promise<bool> SetManifest(Manifest manifest);
    
    /// <summary>
    /// Set the thumbnail of the project in the builder API
    /// </summary>
    /// <param name="manifest"></param>
    /// <returns></returns>
    Promise<bool> SetThumbnail(string id, Texture2D thumbnail);
    
    /// <summary>
    /// Create a new project and return a promise if it has been created correctly
    /// </summary>
    /// <param name="newProject">Data of the new Project</param>
    /// <returns></returns>
    Promise<Manifest> CreateNewProject(ProjectData newProject);
    
    /// <summary>
    /// Dispose the component
    /// </summary>
    void Dispose();
}