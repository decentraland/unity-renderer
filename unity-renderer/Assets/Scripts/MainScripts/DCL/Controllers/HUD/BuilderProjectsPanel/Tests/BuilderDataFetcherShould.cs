using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Helpers;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

public class BuilderDataFetcherShould 
{
    [Test]
    public void FetchProjectDataCorrectly()
    {
        //Arrange
        var api = Substitute.For<IBuilderAPIController>();
        api.Configure().GetAllManifests().Returns(new Promise<List<ProjectData>>());
        
        //Act
        var promise = BuilderPanelDataFetcher.FetchProjectData(api);
        
        //Assert
        api.Received().GetAllManifests();
        Assert.IsNotNull(promise);
    }
    
}
