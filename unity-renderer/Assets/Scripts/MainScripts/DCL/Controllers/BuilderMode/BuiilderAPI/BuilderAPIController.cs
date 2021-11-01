using System.Collections;
using System.Collections.Generic;
using DCL.Builder.Manifest;
using UnityEngine;

public interface IBuilderAPIController
{
    void Initialize();
    void Dispose();
}

public class BuilderAPIController : IBuilderAPIController
{
    private BuilderInWorldBridge builderInWorldBridge;

    public void Initialize()
    {
        //TODO: Implement functionality
    }

    public void Dispose()
    {
        //TODO: Implement functionality
    }

    private void AskHeadersToKernel()
    {
        //TODO: Implement functionality
    }

    public SceneObject[] GetAssets(List<string> assetsIds)
    {
        //TODO: Implement functionality
        return null;
    }

    public List<Manifest> GetAllManifests()
    {
        //TODO: Implement functionality
        return new List<Manifest>();
    }

    public Manifest GetManifestFromProjectId(string projectId)
    {
        //TODO: Implement functionality
        return null;
    }

    public Manifest GetManifestFromLandCoordinates(string landCoords)
    {
        //TODO: Implement functionality
        return null;
    }

    public void UpdateProjectManifest(Manifest manifest)
    {
        //TODO: Implement functionality
    }

    public void CreateEmptyProjectWithCoords(string coords)
    {
        Manifest manifest = BIWUtils.CreateEmptyDefaultBuilderManifest(coords);
        //TODO: Implement functionality
    }
}