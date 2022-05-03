using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Builder;
using UnityEngine;

public class BuilderInWorldAudioHandler : MonoBehaviour
{
    const float MUSIC_DELAY_TIME_ON_START = 4f;
    const float MUSIC_FADE_OUT_TIME_ON_EXIT = 5f;
    const float MUSIC_FADE_OUT_TIME_ON_TUTORIAL = 3f;

    private IBIWCreatorController creatorController;

    private IBIWEntityHandler entityHandler;

    private IBIWModeController modeController;

    [Header("Audio Events")]
    [SerializeField]
    AudioEvent eventAssetSpawn;

    [SerializeField]
    AudioEvent eventAssetPlace;

    [SerializeField]
    AudioEvent eventAssetSelect;

    [SerializeField]
    AudioEvent eventAssetDeselect;

    [SerializeField]
    AudioEvent eventBuilderOutOfBounds;

    [SerializeField]
    AudioEvent eventBuilderOutOfBoundsPlaced;

    [SerializeField]
    AudioEvent eventAssetDelete;

    [SerializeField]
    AudioEvent eventBuilderExit;

    [SerializeField]
    AudioEvent eventBuilderMusic;

    private List<long> entitiesOutOfBounds = new List<long>();
    private int entityCount;
    bool playPlacementSoundOnDeselect;
    private IBIWModeController.EditModeState state = IBIWModeController.EditModeState.Inactive;

    private Coroutine fadeOutCoroutine;
    private Coroutine startBuilderMusicCoroutine;

    private IContext context;

    private void Start() { playPlacementSoundOnDeselect = false; }

    public void Initialize(IContext context)
    {
        this.context = context;
        creatorController = context.editorContext.creatorController;
        entityHandler = context.editorContext.entityHandler;
        modeController = context.editorContext.modeController;

        AddListeners();
    }

    private void AddListeners()
    {
        creatorController.OnCatalogItemPlaced += OnAssetSpawn;
        entityHandler.OnDeleteSelectedEntities += OnAssetDelete;
        modeController.OnChangedEditModeState += OnChangedEditModeState;
        DCL.Environment.i.world.sceneBoundsChecker.OnEntityBoundsCheckerStatusChanged += OnEntityBoundsCheckerStatusChanged;

        if (DCL.Tutorial.TutorialController.i != null)
        {
            DCL.Tutorial.TutorialController.i.OnTutorialEnabled += OnTutorialEnabled;
            DCL.Tutorial.TutorialController.i.OnTutorialDisabled += OnTutorialDisabled;
        }

        entityHandler.OnEntityDeselected += OnAssetDeselect;
        entityHandler.OnEntitySelected += OnAssetSelect;
    }

    public void EnterEditMode(IParcelScene scene)
    {
        UpdateEntityCount();

        if (eventBuilderMusic.source.gameObject.activeSelf)
        {
            if (startBuilderMusicCoroutine != null)
                CoroutineStarter.Stop(startBuilderMusicCoroutine);

            startBuilderMusicCoroutine = CoroutineStarter.Start(StartBuilderMusic());
        }

        if ( context.editorContext.editorHUD != null)
            context.editorContext.editorHUD.OnCatalogItemSelected += OnCatalogItemSelected;

        gameObject.SetActive(true);
    }

    public void ExitEditMode()
    {
        eventBuilderExit.Play();
        if (eventBuilderMusic.source.gameObject.activeSelf)
        {
            if (fadeOutCoroutine != null)
                CoroutineStarter.Stop(fadeOutCoroutine);
            fadeOutCoroutine = CoroutineStarter.Start(eventBuilderMusic.FadeOut(MUSIC_FADE_OUT_TIME_ON_EXIT));
        }

        if ( context.editorContext.editorHUD != null)
            context.editorContext.editorHUD.OnCatalogItemSelected -= OnCatalogItemSelected;

        gameObject.SetActive(false);
    }

    private void OnAssetSpawn() { eventAssetSpawn.Play(); }

    private void OnAssetDelete(List<BIWEntity> entities)
    {
        foreach (BIWEntity deletedEntity in entities)
        {
            if (entitiesOutOfBounds.Contains(deletedEntity.rootEntity.entityId))
            {
                entitiesOutOfBounds.Remove(deletedEntity.rootEntity.entityId);
            }
        }

        eventAssetDelete.Play();
    }

    private void OnAssetSelect() { eventAssetSelect.Play(); }

    private void OnAssetDeselect(BIWEntity entity)
    {
        if (playPlacementSoundOnDeselect)
        {
            eventAssetPlace.Play();
            playPlacementSoundOnDeselect = false;
        }
        else
            eventAssetDeselect.Play();

        UpdateEntityCount();

        if (entitiesOutOfBounds.Contains(entity.rootEntity.entityId))
        {
            eventBuilderOutOfBoundsPlaced.Play();
        }
    }

    private void OnCatalogItemSelected(CatalogItem catalogItem) { playPlacementSoundOnDeselect = true; }

    private void OnTutorialEnabled()
    {
        if (gameObject.activeInHierarchy)
        {
            if (fadeOutCoroutine != null)
                CoroutineStarter.Stop(fadeOutCoroutine);
            fadeOutCoroutine =  CoroutineStarter.Start(eventBuilderMusic.FadeOut(MUSIC_FADE_OUT_TIME_ON_TUTORIAL));
        }
    }

    private void OnTutorialDisabled()
    {
        if (gameObject.activeInHierarchy)
        {
            if (startBuilderMusicCoroutine != null)
                CoroutineStarter.Stop(startBuilderMusicCoroutine);
            startBuilderMusicCoroutine = CoroutineStarter.Start(StartBuilderMusic());
        }
    }

    private IEnumerator StartBuilderMusic()
    {
        yield return new WaitForSeconds(MUSIC_DELAY_TIME_ON_START);

        if (gameObject != null && gameObject.activeInHierarchy)
            eventBuilderMusic.Play();
    }

    private void OnChangedEditModeState(IBIWModeController.EditModeState previous, IBIWModeController.EditModeState current)
    {
        state = current;
        if (previous != IBIWModeController.EditModeState.Inactive)
        {
            switch (current)
            {
                case IBIWModeController.EditModeState.FirstPerson:
                    AudioScriptableObjects.cameraFadeIn.Play();
                    break;
                case IBIWModeController.EditModeState.GodMode:
                    AudioScriptableObjects.cameraFadeOut.Play();
                    break;
                default:
                    break;
            }
        }
    }

    private void OnEntityBoundsCheckerStatusChanged(DCL.Models.IDCLEntity entity, bool isInsideBoundaries)
    {
        if (state == IBIWModeController.EditModeState.Inactive)
            return;

        if (!isInsideBoundaries)
        {
            if (!entitiesOutOfBounds.Contains(entity.entityId))
            {
                entitiesOutOfBounds.Add(entity.entityId);
                eventBuilderOutOfBounds.Play();
            }
        }
        else
        {
            if (entitiesOutOfBounds.Contains(entity.entityId))
            {
                entitiesOutOfBounds.Remove(entity.entityId);
            }
        }
    }

    private void UpdateEntityCount() { entityCount = entityHandler.GetCurrentSceneEntityCount(); }

    private bool EntityHasBeenAddedSinceLastUpdate() { return (entityHandler.GetCurrentSceneEntityCount() > entityCount); }

    private void OnDestroy()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (DataStore.i.common.isApplicationQuitting.Get())
            return;
#endif
        
        Dispose();
    }

    public void Dispose()
    {
        if (startBuilderMusicCoroutine != null)
            CoroutineStarter.Stop(startBuilderMusicCoroutine);

        if (fadeOutCoroutine != null)
            CoroutineStarter.Stop(fadeOutCoroutine);

        startBuilderMusicCoroutine = null;
        fadeOutCoroutine = null;

        RemoveListeners();
    }

    private void RemoveListeners()
    {
        creatorController.OnCatalogItemPlaced -= OnAssetSpawn;
        entityHandler.OnDeleteSelectedEntities -= OnAssetDelete;
        modeController.OnChangedEditModeState -= OnChangedEditModeState;

        if(DCL.Environment.i.world.sceneBoundsChecker != null)
            DCL.Environment.i.world.sceneBoundsChecker.OnEntityBoundsCheckerStatusChanged -= OnEntityBoundsCheckerStatusChanged;

        if (DCL.Tutorial.TutorialController.i != null)
        {
            DCL.Tutorial.TutorialController.i.OnTutorialEnabled -= OnTutorialEnabled;
            DCL.Tutorial.TutorialController.i.OnTutorialDisabled -= OnTutorialDisabled;
        }

        entityHandler.OnEntityDeselected -= OnAssetDeselect;
        entityHandler.OnEntitySelected -= OnAssetSelect;
    }
}