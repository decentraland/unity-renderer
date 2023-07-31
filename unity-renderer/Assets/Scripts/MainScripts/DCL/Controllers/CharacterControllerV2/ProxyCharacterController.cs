using DCL;
using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    public class ProxyCharacterController : MonoBehaviour
    {
        private const string FEATURE_LOCOMOTION_V2 = "locomotion_v2";

        [SerializeField] private GameObject newController;
        [SerializeField] private GameObject oldController;

        private void Awake()
        {
            var featureFlag = DataStore.i.featureFlags.flags.Get();

            if (featureFlag is { IsInitialized: true })
            {
                Setup(featureFlag);
            }
            else
            {
                DataStore.i.featureFlags.flags.OnChange += OnFeatureFlagSet;
            }
        }

        private void OnFeatureFlagSet(FeatureFlag current, FeatureFlag previous)
        {
            Setup(current);
            DataStore.i.featureFlags.flags.OnChange -= OnFeatureFlagSet;
        }

        private void Setup(FeatureFlag featureFlag)
        {
            // TODO: this reference was inside Player prefab
            FollowWithDamping followWithDamping = FindObjectOfType<FollowWithDamping>();

            GameObject controller;
            if (featureFlag.IsFeatureEnabled(FEATURE_LOCOMOTION_V2))
            {
                controller = Instantiate(newController);
                controller.GetComponent<CharacterView>().PostStart(followWithDamping);
            }
            else
            {
                controller = Instantiate(oldController);
                followWithDamping.target = controller.transform;
            }

            // TODO: Kernel is still sending messages so we cant change the name, we need to get rid of this or setup a bridge
            controller.name = "CharacterController";
        }
    }
}
