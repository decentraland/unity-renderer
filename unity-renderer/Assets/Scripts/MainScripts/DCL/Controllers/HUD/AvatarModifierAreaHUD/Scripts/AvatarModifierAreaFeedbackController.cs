namespace DCL.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackController 
    {
        internal IAvatarModifierAreaFeedbackView view;
        private BaseRefCounter<AvatarAreaWarningID> avatarModifiersWarnings;
        
        public AvatarModifierAreaFeedbackController(BaseRefCounter<AvatarAreaWarningID> avatarAreaWarnings, IAvatarModifierAreaFeedbackView view)
        {
            this.view = view;
            view.SetUp(avatarAreaWarnings);
        }

        public void Dispose()
        {
            view.Dispose();
        }
   
    }
}

