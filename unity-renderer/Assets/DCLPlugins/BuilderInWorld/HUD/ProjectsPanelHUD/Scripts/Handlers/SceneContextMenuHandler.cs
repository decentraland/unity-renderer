using System;
using DCL.Builder;
using DCL.Interface;
using UnityEngine;

internal class SceneContextMenuHandler : IDisposable
{
    private readonly SceneCardViewContextMenu contextMenu;
    private readonly ISectionsController sectionsController;
    private readonly IScenesViewController scenesViewController;
    private readonly UnpublishPopupController unpublishPopupController;

    private Vector2Int sceneCoords;
    private Vector2Int sceneSize;
    private Scene.Source sceneSource;

    public SceneContextMenuHandler(SceneCardViewContextMenu contextMenu, ISectionsController sectionsController,
        IScenesViewController scenesViewController, UnpublishPopupController unpublishPopupController)
    {
        this.contextMenu = contextMenu;
        this.sectionsController = sectionsController;
        this.scenesViewController = scenesViewController;
        this.unpublishPopupController = unpublishPopupController;

        sectionsController.OnRequestContextMenuHide += OnRequestContextMenuHide;

        scenesViewController.OnContextMenuPressed += OnContextMenuOpen;

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

        scenesViewController.OnContextMenuPressed -= OnContextMenuOpen;

        contextMenu.OnSettingsPressed -= OnContextMenuSettingsPressed;
        contextMenu.OnDuplicatePressed -= OnContextMenuDuplicatePressed;
        contextMenu.OnDownloadPressed -= OnContextMenuDownloadPressed;
        contextMenu.OnSharePressed -= OnContextMenuSharePressed;
        contextMenu.OnUnpublishPressed -= OnContextMenuUnpublishPressed;
        contextMenu.OnDeletePressed -= OnContextMenuDeletePressed;
        contextMenu.OnQuitContributorPressed -= OnContextMenuQuitContributorPressed;
    }

    void OnContextMenuOpen(ISceneData sceneData, ISceneCardView sceneCard)
    {
        contextMenu.transform.position = sceneCard.contextMenuButtonPosition;
        contextMenu.Show(sceneData.id, sceneData.isDeployed,
            sceneData.isOwner || sceneData.isOperator, sceneData.isContributor);
        sceneCoords = sceneData.coords;
        sceneSource = sceneData.source;
        sceneSize = sceneData.size;
    }

    public void OnContextMenuOpen(IProjectSceneCardView sceneCard)
    {
        contextMenu.transform.position = sceneCard.contextMenuButtonPosition;
        contextMenu.Show( sceneCard.scene.id, true,
            sceneCard.scene.land?.role == LandRole.OWNER || sceneCard.scene.land?.role == LandRole.OPERATOR, false);
        sceneCoords = sceneCard.scene.@base;
        sceneSource = sceneCard.scene.source;
        sceneSize =  BIWUtils.GetSceneSize(sceneCard.scene.parcels);
    }

    void OnRequestContextMenuHide() { contextMenu.Hide(); }

    void OnContextMenuSettingsPressed(string id) { scenesViewController.SelectScene(id); }

    void OnContextMenuDuplicatePressed(string id) { }

    void OnContextMenuDownloadPressed(string id) { }

    void OnContextMenuSharePressed(string id) { }

    void OnContextMenuUnpublishPressed(string id) { unpublishPopupController.Show(sceneCoords,sceneSize, sceneSource); }

    void OnContextMenuDeletePressed(string id) { }

    void OnContextMenuQuitContributorPressed(string id) { }
}