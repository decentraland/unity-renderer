using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class ChatInputContextualMenuController
    {
        private readonly IChatInputContextMenuView view;
        private readonly IClipboard clipboard;
        private bool isAnySelectedText;
        private CancellationTokenSource updateUiCancellationToken;
        private CancellationTokenSource pasteCancellationToken;

        public ChatInputContextualMenuController(
            IChatInputContextMenuView view,
            IClipboard clipboard)
        {
            this.view = view;
            this.clipboard = clipboard;

            view.Hide();

            view.OnShowRequested += OnShowRequested;
            view.OnSelectionChanged += OnSelectionChanged;
            view.OnPasteRequested += PasteFromClipboard;
            view.OnCopyRequested += CopyToClipboard;
        }

        public void Dispose()
        {
            view.OnShowRequested -= OnShowRequested;
            view.OnSelectionChanged -= OnSelectionChanged;
            view.OnPasteRequested -= PasteFromClipboard;
            view.OnCopyRequested -= CopyToClipboard;
            view.Dispose();
        }

        private void OnShowRequested()
        {
            updateUiCancellationToken = updateUiCancellationToken.SafeRestart();
            UpdateView(updateUiCancellationToken.Token)
               .ContinueWith(() => view.Show())
               .Forget();
        }

        private void OnSelectionChanged(string selectedText)
        {
            isAnySelectedText = !string.IsNullOrEmpty(selectedText);
            updateUiCancellationToken = updateUiCancellationToken.SafeRestart();
            UpdateView(updateUiCancellationToken.Token).Forget();
        }

        private void PasteFromClipboard()
        {
            async UniTaskVoid PasteFromClipboardAsync(CancellationToken cancellationToken)
            {
                try
                {
                    string bufferInClipboard = await clipboard.ReadText().ToUniTask(cancellationToken: cancellationToken);
                    view.Paste(bufferInClipboard);
                    view.Hide();
                }
                catch (OperationCanceledException) { }
                catch (Exception e) { Debug.LogException(e); }
            }

            pasteCancellationToken = pasteCancellationToken.SafeRestart();
            PasteFromClipboardAsync(pasteCancellationToken.Token).Forget();
        }

        private void CopyToClipboard(string str)
        {
            clipboard.WriteText(str);
            updateUiCancellationToken = updateUiCancellationToken.SafeRestart();
            UpdateView(updateUiCancellationToken.Token)
               .ContinueWith(() => view.Hide())
               .Forget();
        }

        private async UniTask UpdateView(CancellationToken cancellationToken)
        {
            try
            {
                string bufferInClipboard = await clipboard.ReadText().ToUniTask(cancellationToken: cancellationToken);

                view.SetModel(new ChatInputContextualMenuModel
                {
                    IsCopyButtonEnabled = isAnySelectedText,
                    IsPasteButtonEnabled = !string.IsNullOrEmpty(bufferInClipboard),
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { Debug.LogException(e); }
        }
    }
}
