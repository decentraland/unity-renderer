using System;
using System.Collections;
using DCL;
using DCL.Builder;
using DCL.Interface;
using UnityEngine;

internal class UnpublishPopupController : IDisposable
{
    private const string TITLE = "Unpublish Scene";
    private const string DESCRIPTION = "Are you sure you want to unpublish this scene?";
    private const string SUCCESS_DESCRIPTION = "Scene unpublished";
    private const string ERROR_TITLE = "Error";
    private const string PROGRESS_TITLE = "Unpublishing...";

    private IUnpublishPopupView view;
    private Vector2Int coordinates;
    private Vector2Int size;
    private Scene.Source source;
    private Coroutine fakeProgressRoutine = null;
    private IContext context;

    public UnpublishPopupController(IContext context,IUnpublishPopupView view)
    {
        this.context = context;
        this.view = view;
        view.OnCancelPressed += OnCancel;
        view.OnConfirmPressed += OnConfirmUnpublish;
        view.Hide();
    }

    public void Dispose()
    {
        DataStore.i.builderInWorld.unpublishSceneResult.OnChange -= OnSceneUnpublished;
        CoroutineStarter.Stop(fakeProgressRoutine);
        if (view != null)
        {
            view.OnCancelPressed -= OnCancel;
            view.OnConfirmPressed -= OnConfirmUnpublish;
            view.Dispose();
        }
    }

    public void Show(Vector2Int coordinates, Vector2Int sceneSize, Scene.Source source = Scene.Source.BUILDER_IN_WORLD)
    {
        this.coordinates = coordinates;
        this.source = source;
        this.size = sceneSize;
        view.Show(TITLE, DESCRIPTION);
    }

    void OnConfirmUnpublish()
    {
        context.publisher.Unpublish(coordinates,size);
        BIWAnalytics.PlayerUnpublishScene(source.ToString(), coordinates);
        DataStore.i.builderInWorld.unpublishSceneResult.OnChange += OnSceneUnpublished;
        fakeProgressRoutine = CoroutineStarter.Start(ProgressRoutine());
    }

    void OnCancel()
    {
        view.Hide();
        CoroutineStarter.Stop(fakeProgressRoutine);
    }

    IEnumerator ProgressRoutine()
    {
        float progress = 0;
        while (true)
        {
            progress = Mathf.Clamp(progress + UnityEngine.Random.Range(0.01f, 0.03f), 0, 0.99f);
            view.SetProgress(PROGRESS_TITLE, progress);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.5f));
        }
    }

    private void OnSceneUnpublished(PublishSceneResultPayload current, PublishSceneResultPayload previous)
    {
        DataStore.i.builderInWorld.unpublishSceneResult.OnChange -= OnSceneUnpublished;
        CoroutineStarter.Stop(fakeProgressRoutine);

        if (current.ok)
        {
            view.SetSuccess(TITLE, SUCCESS_DESCRIPTION);
        }
        else
        {
            view.SetError(ERROR_TITLE, current.error);
        }
    }
}