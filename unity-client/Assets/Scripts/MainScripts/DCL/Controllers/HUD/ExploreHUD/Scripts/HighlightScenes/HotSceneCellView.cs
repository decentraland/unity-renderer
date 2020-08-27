using UnityEngine;
using TMPro;

internal class HotSceneCellView : BaseSceneCellView, ICrowdDataView
{
    [SerializeField] GameObject crowdCountContainer;
    [SerializeField] TextMeshProUGUI crowdCount;
    [SerializeField] ShowHideAnimator jumpInButtonAnimator;
    [SerializeField] GameObject friendsContainer;
    [SerializeField] GameObject eventsContainer;
    [SerializeField] UIHoverCallback hoverAreaCallback;

    HotScenesController.HotSceneInfo crowdInfo;

    protected override void Awake()
    {
        base.Awake();

        crowdCountContainer.SetActive(crowdInfo.usersTotalCount > 0);
        eventsContainer.SetActive(false);

        hoverAreaCallback.OnPointerEnter += () =>
        {
            jumpInButtonAnimator.gameObject.SetActive(true);
            jumpInButtonAnimator.Show();
        };
        hoverAreaCallback.OnPointerExit += () => jumpInButtonAnimator.Hide();
        sceneInfoButton.OnPointerDown += () => jumpInButtonAnimator.Hide(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        jumpInButtonAnimator.gameObject.SetActive(false);
    }

    void ICrowdDataView.SetCrowdInfo(HotScenesController.HotSceneInfo info)
    {
        crowdInfo = info;
        crowdCount.text = info.usersTotalCount.ToString();
        crowdCountContainer.SetActive(info.usersTotalCount > 0);
    }

    public override void JumpInPressed()
    {
        HotScenesController.HotSceneInfo.Realm realm = new HotScenesController.HotSceneInfo.Realm() { layer = null, serverName = null };
        for (int i = 0; i < crowdInfo.realms.Length; i++)
        {
            if (crowdInfo.realms[i].usersCount < crowdInfo.realms[i].usersMax)
            {
                realm = crowdInfo.realms[i];
                break;
            }
        }

        JumpIn(crowdInfo.baseCoords, realm.serverName, realm.layer);
    }
}
