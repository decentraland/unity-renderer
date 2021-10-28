using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Helpers;
using UnityEngine;

public static class BuilderPanelDataFetcher
{
    public static Promise<ProjectData[]> FetchProjectData()
    {
        var promise = new Promise<ProjectData[]>();
        CoroutineStarter.Start(MockedDelay(promise));
        return promise;
    }

    private static IEnumerator MockedDelay(Promise<ProjectData[]> promise)
    {
        yield return new WaitForSeconds(0.5f);
        promise.Resolve(new ProjectData[0]);
    }
}