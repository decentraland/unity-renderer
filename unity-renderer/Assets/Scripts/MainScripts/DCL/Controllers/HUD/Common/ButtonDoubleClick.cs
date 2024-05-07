using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonDoubleClick : MonoBehaviour, IButtonDoubleClick
{
    public event Action OnClick;
    public event Action OnDoubleClick;
    public bool AlwaysPerformSingleClick { get; set; } = false;

    private Button button;
    private CancellationTokenSource cts = new CancellationTokenSource();

    private void Awake() =>
        button = GetComponent<Button>();

    private void OnEnable()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        DetectClicks().Forget();
    }

    private void OnDisable()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    private async UniTaskVoid DetectClicks()
    {
        const int TIME_BETWEEN_CLICKS_MS = 200;
        TimeoutController timeoutController = new TimeoutController(cts);
        IAsyncClickEventHandler clickEventHandler = button.GetAsyncClickEventHandler(cts.Token);
        while (true)
        {
            await clickEventHandler.OnClickAsync();

            if(AlwaysPerformSingleClick)
                OnClick?.Invoke();

            timeoutController.Reset();

            try
            {
                await clickEventHandler.OnClickAsync().AttachExternalCancellation(timeoutController.Timeout(TIME_BETWEEN_CLICKS_MS));
            }
            catch (OperationCanceledException e)
            {
                if(cts.IsCancellationRequested)
                    throw;
            }
            if (timeoutController.IsTimeout())
            {
                OnClick?.Invoke();
                continue;
            }
            OnDoubleClick?.Invoke();
        }
    }
}
