using System;
using System.Collections;
using UnityEngine;

internal class UnpublishPopupController : IUnpublishListener, IUnpublishRequester, IDisposable
{
    private const string TITLE = "Unpublish Scene";
    private const string DESCRIPTION = "Are you sure you want to unpublish this scene?";
    private const string SUCCESS_DESCRIPTION = "Scene unpublished";
    private const string ERROR_TITLE = "Error";
    private const string PROGRESS_TITLE = "Unpublishing...";

    public event Action<Vector2Int> OnRequestUnpublish;

    private IUnpublishPopupView view;
    private Vector2Int coordinates;
    private Coroutine fakeProgressRoutine = null;

    public UnpublishPopupController(IUnpublishPopupView view)
    {
        this.view = view;
        view.OnCancelPressed += OnCancel;
        view.OnConfirmPressed += OnConfirmUnpublish;
        view.Hide();
    }

    public void Dispose()
    {
        CoroutineStarter.Stop(fakeProgressRoutine);
        view.OnCancelPressed -= OnCancel;
        view.OnConfirmPressed -= OnConfirmUnpublish;
        view.Dispose();
    }

    public void Show(Vector2Int coordinates)
    {
        this.coordinates = coordinates;
        view.Show(TITLE, DESCRIPTION);
    }

    void IUnpublishListener.OnUnpublishResult(PublishSceneResultPayload result)
    {
        CoroutineStarter.Stop(fakeProgressRoutine);

        if (result.ok)
        {
            view.SetSuccess(TITLE, SUCCESS_DESCRIPTION);
        }
        else
        {
            view.SetError(ERROR_TITLE, result.error);
        }
    }

    void OnConfirmUnpublish()
    {
        OnRequestUnpublish?.Invoke(coordinates);
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
            progress = Mathf.Clamp(progress + UnityEngine.Random.Range(0.01f, 0.03f), progress, 0.99f);
            view.SetProgress(PROGRESS_TITLE, progress);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.5f));
        }
    }
}