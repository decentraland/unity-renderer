using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SectionProjectControllerShould
{
    private SectionProjectController sectionProjectController;

    [SetUp]
    public void SetUp()
    {
        sectionProjectController = new SectionProjectController();
    }

    [TearDown]
    public void TearDown() { sectionProjectController.Dispose(); }
    
    [Test]
    public void SearchResultCorrectly()
    {
        //Arrange
        Dictionary<string, IProjectCardView> projects = new Dictionary<string, IProjectCardView>();
        var card = Substitute.For<IProjectCardView>();
        card.Configure().searchInfo.Returns(Substitute.For<ISearchInfo>());
        projects.Add("Test",card);
        ((IProjectsListener)sectionProjectController).OnSetProjects(projects);

        List<ISearchInfo> infoList = new List<ISearchInfo>();
        SearchInfo info = new SearchInfo();
        info.id = "Test";
        infoList.Add(info);
        
        //Act
        sectionProjectController.OnSearchResult(infoList);
        
        //Assert
        card.Received().SetActive(true);
    }
    
}
