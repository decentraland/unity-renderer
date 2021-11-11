using System;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using DCL.Builder.Manifest;
using DCL.Helpers;

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
    /// Create a new project and return a promise if it has been created correctly
    /// </summary>
    /// <param name="newProject">Data of the new Project</param>
    /// <returns></returns>
    Promise<APIResponse> CreateNewProject(ProjectData newProject);
    
    /// <summary>
    /// Dispose the component
    /// </summary>
    void Dispose();
}