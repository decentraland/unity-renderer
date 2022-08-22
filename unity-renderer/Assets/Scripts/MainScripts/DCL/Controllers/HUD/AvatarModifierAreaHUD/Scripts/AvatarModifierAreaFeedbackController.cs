namespace DCL.AvatarModifierAreaFeedback
{
    public class AvatarModifierAreaFeedbackController 
    {
        internal IAvatarModifierAreaFeedbackView view;
        private BaseRefCounter<AvatarModifierAreaID> avatarModifiersWarnings;
        
        public AvatarModifierAreaFeedbackController(BaseRefCounter<AvatarModifierAreaID> avatarAreaWarnings, IAvatarModifierAreaFeedbackView view)
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

