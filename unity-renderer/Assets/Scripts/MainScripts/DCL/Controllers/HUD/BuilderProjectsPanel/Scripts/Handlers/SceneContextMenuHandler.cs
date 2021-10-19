using System;
using DCL.Builder;
using DCL.Interface;
using UnityEngine;

internal class SceneContextMenuHandler : IDisposable
{
    private readonly SceneCardViewContextMenu contextMenu;
    private readonly ISectionsController sectionsController;
    private readonly IPlacesViewController placesViewController;
    private readonly UnpublishPopupController unpublishPopupController;

    private Vector2Int sceneCoords;
    private Place.Source sceneSource;

    public SceneContextMenuHandler(SceneCardViewContextMenu contextMenu, ISectionsController sectionsController,
        IPlacesViewController placesViewController, UnpublishPopupController unpublishPopupController)
    {
        this.contextMenu = contextMenu;
        this.sectionsController = sectionsController;
        this.placesViewController = placesViewController;
        this.unpublishPopupController = unpublishPopupController;

        sectionsController.OnRequestContextMenuHide += OnRequestContextMenuHide;

        placesViewController.OnContextMenuPressed += OnContextMenuOpen;

        contextMenu.OnSettingsPressed += OnContextMenuSettingsPressed;
        contextMenu.OnDuplicatePressed += OnContextMenuDuplicatePressed;
        contextMenu.OnDownloadPressed += OnContextMenuDownloadPressed;
        contextMenu.OnSharePressed += OnContextMenuSharePressed;
        contextMenu.OnUnpublishPressed += OnContextMenuUnpublishPressed;
        contextMenu.OnDeletePressed += OnContextMenuDeletePressed;
        contextMenu.OnQuitContributorPressed += OnContextMenuQuitContributorPressed;
    }

    public void Dispose()
    {
        sectionsController.OnRequestContextMenuHide -= OnRequestContextMenuHide;

        placesViewController.OnContextMenuPressed -= OnContextMenuOpen;

        contextMenu.OnSettingsPressed -= OnContextMenuSettingsPressed;
        contextMenu.OnDuplicatePressed -= OnContextMenuDuplicatePressed;
        contextMenu.OnDownloadPressed -= OnContextMenuDownloadPressed;
        contextMenu.OnSharePressed -= OnContextMenuSharePressed;
        contextMenu.OnUnpublishPressed -= OnContextMenuUnpublishPressed;
        contextMenu.OnDeletePressed -= OnContextMenuDeletePressed;
        contextMenu.OnQuitContributorPressed -= OnContextMenuQuitContributorPressed;
    }

    void OnContextMenuOpen(IPlaceData placeData, IPlaceCardView placeCard)
    {
        contextMenu.transform.position = placeCard.contextMenuButtonPosition;
        contextMenu.Show(placeData.id, placeData.isDeployed,
            placeData.isOwner || placeData.isOperator, placeData.isContributor);
        sceneCoords = placeData.coords;
        sceneSource = placeData.source;
    }

    void OnRequestContextMenuHide() { contextMenu.Hide(); }

    void OnContextMenuSettingsPressed(string id) { placesViewController.SelectPlace(id); }

    void OnContextMenuDuplicatePressed(string id) { }

    void OnContextMenuDownloadPressed(string id) { }

    void OnContextMenuSharePressed(string id) { }

    void OnContextMenuUnpublishPressed(string id) { unpublishPopupController.Show(sceneCoords, sceneSource); }

    void OnContextMenuDeletePressed(string id) { }

    void OnContextMenuQuitContributorPressed(string id) { }
}