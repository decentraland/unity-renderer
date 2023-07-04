using System;

namespace DCL
{
    public record ExperiencesConfirmationData
    {
        public record ExperienceMetadata
        {
            public string ExperienceId { get; set; }
            public string ExperienceName { get; set; }
            public string IconUrl { get; set; }
            public string[] Permissions { get; set; }
        }

        public Action OnAcceptCallback;
        public Action OnRejectCallback;
        public ExperienceMetadata Experience;
    }

    public record DataStore_ExperiencesConfirmation
    {
        public readonly BaseVariable<ExperiencesConfirmationData> Confirm = new ();
    }
}
