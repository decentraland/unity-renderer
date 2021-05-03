using System;
using DCL.Interface;

internal class SceneContextMenuHandler : IDisposable
{
    private readonly SceneCardViewContextMenu contextMenu;
    private readonly IBuilderProjectsPanelBridge bridge;
    private readonly ISectionsController sectionsController;
    private readonly IScenesViewController scenesViewController;

    public SceneContextMenuHandler(SceneCardViewContextMenu contextMenu, ISectionsController sectionsController,
        IScenesViewController scenesViewController, IBuilderProjectsPanelBridge bridge)
    {
        this.contextMenu = contextMenu;
        this.bridge = bridge;
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
    
    void OnRequestContextMenuHide()
    {
        contextMenu.Hide();
    }

    void OnContextMenuSettingsPressed(string id)
    {
        scenesViewController.SelectScene(id);
    }

    void OnContextMenuDuplicatePressed(string id)
    {
        bridge?.SendDuplicateProject(id);
    }

    void OnContextMenuDownloadPressed(string id)
    {
        bridge?.SendDownload(id);
    }

    void OnContextMenuSharePressed(string id)
    {
        bridge?.SendShare(id);
    }

    void OnContextMenuUnpublishPressed(string id)
    {
        bridge?.SendUnPublish(id);
    }

    void OnContextMenuDeletePressed(string id)
    {
        bridge?.SendDelete(id);
    }

    void OnContextMenuQuitContributorPressed(string id)
    {
        bridge?.SendQuitContributor(id);
    }
}
