using DCL.Interface;
using System;

public class AirdroppingHUDController : IHUD
{

    [Serializable]
    public class Model
    {
        public string id;
        public string title;
        public string subtitle;
        public ItemModel[] items;
    }

    [Serializable]
    public class ItemModel
    {
        public string name;
        public string thumbnailURL;
        public string type;
        public string rarity;
        public string subtitle;
    }

    public enum State
    {
        Hidden,
        Initial,
        SingleItem,
        Summary,
        Summary_NoItems,
        Finish
    }

    internal AirdroppingHUDView view;
    internal State currentState;
    internal Model model;
    internal int currentItemShown = -1;
    internal int totalItems => model?.items?.Length ?? 0;

    public static event System.Action OnAirdropFinished = null;

    public AirdroppingHUDController()
    {
        view = AirdroppingHUDView.Create();
        view.Initialize(MoveToNextState);
        currentState = State.Hidden;
        ApplyState();
    }

    public void AirdroppingRequested(Model model)
    {
        if (model == null) return;

        this.model = model;

        for (var i = 0; i < this.model.items.Length; i++)
        {
            ThumbnailsManager.PreloadThumbnail(this.model.items[i].thumbnailURL);
        }

        currentState = State.Initial;
        ApplyState();
    }

    public void MoveToNextState()
    {
        SetNextState();
        ApplyState();
    }

    public void SetNextState()
    {
        switch (currentState)
        {
            case State.Initial:
                if (currentItemShown > totalItems - 1)
                    currentState = totalItems != 0 ? State.Summary : State.Summary_NoItems;
                else
                    currentState = State.SingleItem;
                break;
            case State.SingleItem:
                currentItemShown++;
                if (currentItemShown > totalItems - 1)
                    currentState = totalItems != 0 ? State.Summary : State.Summary_NoItems;
                break;
            case State.Summary:
                currentState = State.Finish;
                break;
            case State.Summary_NoItems:
                currentState = State.Hidden;
                break;
            case State.Finish:
            default:
                currentState = State.Hidden;
                break;
        }
    }

    public void ApplyState()
    {
        switch (currentState)
        {
            case State.Initial:
                currentItemShown = 0;
                view.SetContentActive(true);
                view.ShowInitialScreen(model.title, model.subtitle);
                break;
            case State.SingleItem:
                view.ShowItemScreen(model.items[currentItemShown], model.items.Length - (currentItemShown + 1));
                break;
            case State.Summary:
                view.ShowSummaryScreen(model.items);
                break;
            case State.Summary_NoItems:
                view.ShowSummaryNoItemsScreen();
                break;
            case State.Finish:
                WebInterface.SendUserAcceptedCollectibles(model.id);

                OnAirdropFinished?.Invoke();

                MoveToNextState();
                break;
            case State.Hidden:
            default:
                model = null;
                view.SetContentActive(false);
                break;
        }
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

    public void Dispose()
    {
        if (view != null)
            UnityEngine.Object.Destroy(view.gameObject);
    }
}
