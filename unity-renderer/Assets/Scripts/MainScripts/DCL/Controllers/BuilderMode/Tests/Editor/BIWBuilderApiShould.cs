using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DCL;
using DCL.Builder;
using DCL.Helpers;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.Core.Arguments;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

public class BIWBuilderApiShould : IntegrationTestSuite
{
    private BuilderAPIController apiController;
    private IWebRequestController mockedRequestController;
    private string baseURL;
    private GameObject gameObjectToDestroy;

    protected override PlatformContext CreatePlatformContext()
    {
        mockedRequestController =  Substitute.For<IWebRequestController>();
        return DCL.Tests.PlatformContextFactory.CreateWithGenericMocks( mockedRequestController
        );
    }

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        gameObjectToDestroy = new GameObject("TestBuilderApi");
        gameObjectToDestroy.AddComponent<AssetCatalogBridge>();

        baseURL = BIWUrlUtils.GetBuilderAPIBaseUrl();
        apiController = new BuilderAPIController();

        var context = BIWTestUtils.CreateMockedContext();
        context.sceneReferences.Configure().biwBridgeGameObject.Returns(gameObjectToDestroy);
        apiController.Initialize(context);
    }

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
        yield return base.TearDown();
        AssetCatalogBridge.i.ClearCatalog();
        apiController.Dispose();
        GameObject.Destroy(gameObjectToDestroy);
    }

    [Test]
    public void AskHeadersToKernelCorrectly()
    {
        //Arrange
        RequestHeader header = CreateRequestHeaders("get", "Test");
        RequestHeader receivedHeader = null;

        //Act
        var promise = apiController.AskHeadersToKernel("get", "Test");
        promise.Then( request =>
        {
            receivedHeader = request;
        });
        apiController.HeadersReceived(header);

        //Assert
        Assert.AreEqual(header, receivedHeader);
    }

    [Test]
    public void GetAllManifestsCorrectly()
    {
        //Arrange
        RequestHeader header = CreateRequestHeaders(BuilderAPIController.GET, BuilderAPIController.GET_PROJECTS_ENDPOINT);

        List<ProjectData> projectDatas = new List<ProjectData>();
        ProjectData data = new ProjectData();
        projectDatas.Add(data);
        projectDatas.Add(data);

        List<ProjectData> result = null;

        string jsonData = JsonConvert.SerializeObject(projectDatas);
        apiController.apiResponseResolver = Substitute.For<IBuilderAPIResponseResolver>();
        apiController.apiResponseResolver.Configure().GetArrayFromCall<ProjectData>(Arg.Any<string>()).Returns(projectDatas.ToArray());

        TestUtils.ConfigureMockedRequestController(jsonData, mockedRequestController, 2);

        //Act
        var promise = apiController.GetAllManifests();
        promise.Then( data =>
        {
            result = data;
        });

        apiController.HeadersReceived(header);

        //Assert
        Assert.AreEqual(projectDatas.Count, result.Count);
    }

    [Test]
    public void GetCompleteCatalogCorrectly()
    {
        //Arrange
        RequestHeader defaultCallheader = CreateRequestHeaders(BuilderAPIController.GET, BuilderAPIController.CATALOG_ENDPOINT);
        RequestHeader addreesCallheader = CreateRequestHeaders(BuilderAPIController.GET, BuilderAPIController.CATALOG_ENDPOINT);
        bool result = false;
        string jsonPath = TestAssetsUtils.GetPathRaw() + "/BuilderInWorldCatalog/multipleSceneObjectsCatalog.json";
        string jsonValue = File.ReadAllText(jsonPath);

        TestUtils.ConfigureMockedRequestController(jsonValue, mockedRequestController, 2);

        //Act
        var promise = apiController.GetCompleteCatalog("Test");
        promise.Then( data =>
        {
            result = data;
        });

        apiController.HeadersReceived(defaultCallheader);
        apiController.HeadersReceived(addreesCallheader);

        //Assert
        Assert.IsTrue(result);
        Assert.Greater(AssetCatalogBridge.i.sceneObjectCatalog.Count, 0);
    }

    [Test]
    public void GetAssetsCorrectly()
    {
        //Arrange
        RequestHeader header = CreateRequestHeaders(BuilderAPIController.GET, BuilderAPIController.ASSETS_ENDPOINT);

        bool result = false;
        List<SceneObject> list = new List<SceneObject>();
        list.Add(new SceneObject(){id ="test id"});
        
        string jsonData = JsonConvert.SerializeObject(list);
        TestUtils.ConfigureMockedRequestController(jsonData, mockedRequestController,2);
        
        apiController.apiResponseResolver = Substitute.For<IBuilderAPIResponseResolver>();
        apiController.apiResponseResolver.Configure().GetArrayFromCall<SceneObject>(Arg.Any<string>()).Returns(list.ToArray());
        
        //Act
        var promise = apiController.GetAssets(new List<string>());
        promise.Then( data =>
        {
            result = data;
        });

        apiController.HeadersReceived(header);

        //Assert
        Assert.IsTrue(result);
        Assert.Greater(AssetCatalogBridge.i.sceneObjectCatalog.Count, 0);
    }

    private RequestHeader CreateRequestHeaders(string method, string endpoint)
    {
        RequestHeader header = new RequestHeader();
        header.method = method;
        header.endpoint = endpoint;
        header.headers = new Dictionary<string, string>();
        return header;
    }
}