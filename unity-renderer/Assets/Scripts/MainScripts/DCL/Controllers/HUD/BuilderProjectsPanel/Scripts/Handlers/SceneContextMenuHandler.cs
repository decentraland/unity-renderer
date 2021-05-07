using System;
using DCL.Interface;

internal class SceneContextMenuHandler : IDisposable
{
    private readonly SceneCardViewContextMenu contextMenu;
    private readonly ISectionsController sectionsController;
    private readonly IScenesViewController scenesViewController;

    public SceneContextMenuHandler(SceneCardViewContextMenu contextMenu, ISectionsController sectionsController,
        IScenesViewController scenesViewController)
    {
        this.contextMenu = contextMenu;
        this.sectionsController = sectionsController;
        this.scenesViewController = scenesViewController;

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
    }

    void OnRequestContextMenuHide() { contextMenu.Hide(); }

    void OnContextMenuSettingsPressed(string id) { scenesViewController.SelectScene(id); }

    void OnContextMenuDuplicatePressed(string id) { }

    void OnContextMenuDownloadPressed(string id) { }

    void OnContextMenuSharePressed(string id) { }

    void OnContextMenuUnpublishPressed(string id) { }

    void OnContextMenuDeletePressed(string id) { }

    void OnContextMenuQuitContributorPressed(string id) { }
}