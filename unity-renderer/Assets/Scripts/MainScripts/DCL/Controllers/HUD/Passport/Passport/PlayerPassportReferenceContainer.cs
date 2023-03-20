using UnityEngine;

namespace DCL.Social.Passports
{
    public class PlayerPassportReferenceContainer : MonoBehaviour
    {
        [SerializeField] private PlayerPassportHUDView passportView;
        [SerializeField] private PassportPlayerInfoComponentView playerInfoView;
        [SerializeField] private PassportPlayerPreviewComponentView playerPreviewView;
        [SerializeField] private PassportNavigationComponentView passportNavigationView;
        [SerializeField] private ViewAllComponentView viewAllView;

        public IPlayerPassportHUDView PassportView => passportView;
        public IPassportPlayerInfoComponentView PlayerInfoView => playerInfoView;
        public IPassportPlayerPreviewComponentView PlayerPreviewView => playerPreviewView;
        public IPassportNavigationComponentView PassportNavigationView => passportNavigationView;
        public IViewAllComponentView ViewAllView => viewAllView;
    }
}
