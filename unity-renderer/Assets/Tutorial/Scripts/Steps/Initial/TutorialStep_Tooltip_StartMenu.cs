using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to navigate through the Start Menu.
    /// </summary>
    public class TutorialStep_Tooltip_StartMenu : TutorialStep_Tooltip
    {
        private const int TEACHER_CANVAS_SORT_ORDER_START = 4;
        private const string TUTORIAL_COMPLETED_STEP = "TutorialStep_TutorialCompleted";
        private const string TOP_MENU_STEP = "TutorialStep_StartMenuTooltip_TopMenu";
        private const string PLACES_AND_EVENTS_STEP = "TutorialStep_StartMenuTooltip_PlacesAndEventsSection";
        private const string BACKPACK_STEP = "TutorialStep_StartMenuTooltip_BackpackSection";
        private const string MAP_STEP = "TutorialStep_StartMenuTooltip_MapSection";
        private const string BUILDER_STEP = "TutorialStep_StartMenuTooltip_BuilderSection";
        private const string QUEST_STEP = "TutorialStep_StartMenuTooltip_QuestSection";
        private const string SETTINGS_STEP = "TutorialStep_StartMenuTooltip_SettingsSection";
        private const string CAMERA_REEL_STEP = "TutorialStep_StartMenuTooltip_CameraReelSection";

        private int defaultTeacherCanvasSortOrder;

        public override void OnStepStart()
        {
            base.OnStepStart();

            DataStore.i.exploreV2.isOpen.OnChange += ExploreV2IsOpenChanged;
            DataStore.i.exploreV2.placesAndEventsVisible.OnChange += PlacesAndEventsVisibleChanged;
            DataStore.i.HUDs.questsPanelVisible.OnChange += QuestsPanelVisibleChanged;
            DataStore.i.HUDs.avatarEditorVisible.OnChange += AvatarEditorVisibleChanged;
            DataStore.i.HUDs.navmapVisible.OnChange += NavMapVisibleChanged;
            DataStore.i.HUDs.builderProjectsPanelVisible.OnChange += BuilderProjectsPanelVisibleChanged;
            DataStore.i.HUDs.cameraReelSectionVisible.OnChange += CameraReelVisibleChanged;
            DataStore.i.settings.settingsPanelVisible.OnChange += SettingsPanelVisibleChanged;

            if (tutorialController.configuration.teacherCanvas != null)
                defaultTeacherCanvasSortOrder = tutorialController.configuration.teacherCanvas.sortingOrder;

            tutorialController.SetTeacherCanvasSortingOrder(TEACHER_CANVAS_SORT_ORDER_START);
            isRelatedFeatureActived = true;
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            tutorialController.SetTeacherCanvasSortingOrder(defaultTeacherCanvasSortOrder);

            DataStore.i.exploreV2.isOpen.OnChange -= ExploreV2IsOpenChanged;
            DataStore.i.exploreV2.placesAndEventsVisible.OnChange -= PlacesAndEventsVisibleChanged;
            DataStore.i.HUDs.questsPanelVisible.OnChange -= QuestsPanelVisibleChanged;
            DataStore.i.HUDs.avatarEditorVisible.OnChange -= AvatarEditorVisibleChanged;
            DataStore.i.HUDs.navmapVisible.OnChange -= NavMapVisibleChanged;
            DataStore.i.HUDs.builderProjectsPanelVisible.OnChange -= BuilderProjectsPanelVisibleChanged;
            DataStore.i.settings.settingsPanelVisible.OnChange -= SettingsPanelVisibleChanged;
            DataStore.i.HUDs.cameraReelSectionVisible.OnChange -= CameraReelVisibleChanged;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            Transform startMenuTooltipTransform = null;

            switch (gameObject.name)
            {
                case TOP_MENU_STEP:
                    startMenuTooltipTransform = DataStore.i.exploreV2.topMenuTooltipReference.Get();
                    break;
                case PLACES_AND_EVENTS_STEP:
                    startMenuTooltipTransform = DataStore.i.exploreV2.placesAndEventsTooltipReference.Get();
                    break;
                case BACKPACK_STEP:
                    startMenuTooltipTransform = DataStore.i.exploreV2.backpackTooltipReference.Get();
                    break;
                case MAP_STEP:
                    startMenuTooltipTransform = DataStore.i.exploreV2.mapTooltipReference.Get();
                    break;
                case BUILDER_STEP:
                    startMenuTooltipTransform = DataStore.i.exploreV2.builderTooltipReference.Get();
                    break;
                case QUEST_STEP:
                    startMenuTooltipTransform = DataStore.i.exploreV2.questTooltipReference.Get();
                    break;
                case SETTINGS_STEP:
                    startMenuTooltipTransform = DataStore.i.exploreV2.settingsTooltipReference.Get();
                    break;
                case CAMERA_REEL_STEP:
                    startMenuTooltipTransform = DataStore.i.exploreV2.cameraReelTooltipReference.Get();
                    break;
            }

            if (startMenuTooltipTransform != null)
                tooltipTransform.position = startMenuTooltipTransform.position;
        }

        private void ExploreV2IsOpenChanged(bool current, bool previous)
        {
            if (current)
                return;

            tutorialController.GoToSpecificStep(TUTORIAL_COMPLETED_STEP);
        }

        private void PlacesAndEventsVisibleChanged(bool current, bool previous)
        {
            if (!current)
                return;

            tutorialController.GoToSpecificStep(PLACES_AND_EVENTS_STEP);
        }

        private void AvatarEditorVisibleChanged(bool current, bool previous)
        {
            if (!current)
                return;

            tutorialController.GoToSpecificStep(BACKPACK_STEP);
        }

        private void NavMapVisibleChanged(bool current, bool previous)
        {
            if (!current)
                return;

            tutorialController.GoToSpecificStep(MAP_STEP);
        }

        private void BuilderProjectsPanelVisibleChanged(bool current, bool previous)
        {
            if (!current)
                return;

            tutorialController.GoToSpecificStep(BUILDER_STEP);
        }

        private void QuestsPanelVisibleChanged(bool current, bool previous)
        {
            if (!current)
                return;

            tutorialController.GoToSpecificStep(QUEST_STEP);
        }

        private void SettingsPanelVisibleChanged(bool current, bool previous)
        {
            if (!current)
                return;

            tutorialController.GoToSpecificStep(SETTINGS_STEP);
        }

        private void CameraReelVisibleChanged(bool current, bool previous)
        {
            if (!current)
                return;

            tutorialController.GoToSpecificStep(CAMERA_REEL_STEP);
        }
    }
}
