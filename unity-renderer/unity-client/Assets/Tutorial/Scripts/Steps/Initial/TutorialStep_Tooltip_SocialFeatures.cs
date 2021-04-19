using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to show the social features available in the taskbar.
    /// </summary>
    public class TutorialStep_Tooltip_SocialFeatures : TutorialStep_Tooltip
    {
        private const int TEACHER_CANVAS_SORT_ORDER_START = 4;

        [SerializeField] InputAction_Hold voiceChatAction;

        private int defaultTeacherCanvasSortOrder;

        public override void OnStepStart()
        {
            base.OnStepStart();

            defaultTeacherCanvasSortOrder = tutorialController.teacherCanvas.sortingOrder;
            tutorialController.SetTeacherCanvasSortingOrder(TEACHER_CANVAS_SORT_ORDER_START);

            if (tutorialController != null &&
                tutorialController.hudController != null)
            {
                if (tutorialController.hudController.worldChatWindowHud != null)
                {
                    tutorialController.hudController.worldChatWindowHud.OnOpen += WorldChatWindowHud_OnOpen;
                }

                if (tutorialController.hudController.friendsHud != null)
                {
                    tutorialController.hudController.friendsHud.OnFriendsOpened += FriendsHud_OnFriendsOpened;
                    tutorialController.hudController.friendsHud.OnFriendsClosed += FriendsHud_OnFriendsClosed;
                }

                if (voiceChatAction != null)
                {
                    voiceChatAction.OnStarted += VoiceChatAction_OnStarted;
                    voiceChatAction.OnFinished += VoiceChatAction_OnFinished;
                }
            }
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            tutorialController.SetTeacherCanvasSortingOrder(defaultTeacherCanvasSortOrder);

            if (tutorialController != null &&
                tutorialController.hudController != null)
            {
                if (tutorialController.hudController.worldChatWindowHud != null)
                {
                    tutorialController.hudController.worldChatWindowHud.OnOpen -= WorldChatWindowHud_OnOpen;
                }

                if (tutorialController.hudController.friendsHud != null)
                {
                    tutorialController.hudController.friendsHud.OnFriendsOpened -= FriendsHud_OnFriendsOpened;
                    tutorialController.hudController.friendsHud.OnFriendsClosed -= FriendsHud_OnFriendsClosed;
                }

                if (voiceChatAction != null)
                {
                    voiceChatAction.OnStarted -= VoiceChatAction_OnStarted;
                    voiceChatAction.OnFinished -= VoiceChatAction_OnFinished;
                }
            }
        }

        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.taskbarHud != null)
            {
                if (tutorialController.hudController.taskbarHud.socialTooltipReference)
                {
                    tooltipTransform.position =
                        tutorialController.hudController.taskbarHud.socialTooltipReference.position;
                }
            }
        }

        private void WorldChatWindowHud_OnOpen()
        {
            stepIsFinished = true;
        }

        private void FriendsHud_OnFriendsOpened()
        {
            SocialFeatureIsOpen(true);
        }

        private void FriendsHud_OnFriendsClosed()
        {
            SocialFeatureIsOpen(false);
        }

        private void VoiceChatAction_OnStarted(DCLAction_Hold action)
        {
            SocialFeatureIsOpen(true);
        }

        private void VoiceChatAction_OnFinished(DCLAction_Hold action)
        {
            SocialFeatureIsOpen(false);
        }

        private void SocialFeatureIsOpen(bool isOpen)
        {
            if (isOpen)
            {
                isRelatedFeatureActived = true;
                stepIsFinished = true;
                tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
            }
            else if (isRelatedFeatureActived)
            {
                isRelatedFeatureActived = false;
            }
        }
    }
}