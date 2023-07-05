using DCL.Controllers;
using DCL.Models;
using DCL.WorldRuntime.PortableExperiences;
using System;

namespace DCL.PortableExperiences.Confirmation
{
    public class TriggerPortableExperienceController
    {
        private readonly DataStore dataStore;
        private readonly IPortableExperiencesController portableExperiencesController;
        private readonly Func<IWorldState> worldStateProvider;

        public TriggerPortableExperienceController(DataStore dataStore,
            IPortableExperiencesController portableExperiencesController,
            Func<IWorldState> worldStateProvider)
        {
            this.dataStore = dataStore;
            this.portableExperiencesController = portableExperiencesController;
            this.worldStateProvider = worldStateProvider;
            dataStore.world.portableExperienceIds.OnAdded += OnPortableExperienceAdded;
        }

        public void Dispose()
        {
            dataStore.world.portableExperienceIds.OnAdded -= OnPortableExperienceAdded;
        }

        private void OnPortableExperienceAdded(string portableExperienceId)
        {
            IParcelScene portableExperience = GetPortableExperience(portableExperienceId);
            if (portableExperience == null) return;

            portableExperiencesController.DisablePortableExperience(portableExperienceId);

            LoadParcelScenesMessage.UnityParcelScene metadata = portableExperience.sceneData;

            dataStore.ExperiencesConfirmation.Confirm.Set(new ExperiencesConfirmationData
            {
                Experience = new ExperiencesConfirmationData.ExperienceMetadata
                {
                    Permissions = metadata.requiredPermissions,
                    ExperienceId = metadata.id,
                    ExperienceName = portableExperience.GetSceneName(),
                    IconUrl = (portableExperience as GlobalScene)?.iconUrl,
                },
                OnAcceptCallback = () => portableExperiencesController.EnablePortableExperience(portableExperienceId),
                OnRejectCallback = () => portableExperiencesController.DisablePortableExperience(portableExperienceId),
            });
        }

        private IParcelScene GetPortableExperience(string pxId) =>
            worldStateProvider.Invoke().GetPortableExperienceScene(pxId);
    }
}
