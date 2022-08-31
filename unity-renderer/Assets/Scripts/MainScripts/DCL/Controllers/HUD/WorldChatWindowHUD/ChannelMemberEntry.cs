using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChannelMemberEntry : BaseComponentView, IComponentModelConfig<ChannelMemberEntryModel>
    {
        [Header("Configuration")]
        [SerializeField] internal ChannelMemberEntryModel model;

        public ChannelMemberEntryModel Model => model;

        public void Configure(ChannelMemberEntryModel newModel)
        {
            
        }

        public override void RefreshControl()
        {
            
        }
    }
}