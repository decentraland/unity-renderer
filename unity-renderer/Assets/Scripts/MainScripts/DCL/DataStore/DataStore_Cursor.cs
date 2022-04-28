namespace DCL
{
    public class DataStore_Cursor
    {
        public enum CursorType
        {
            NORMAL,
            HOVER
        }

        public readonly BaseVariable<bool> cursorVisible = new BaseVariable<bool>(true);
        public readonly BaseVariable<CursorType> cursorType = new BaseVariable<CursorType>(CursorType.NORMAL);
        public readonly BaseVariable<bool> hoverFeedbackEnabled = new BaseVariable<bool>(false);
        public readonly BaseVariable<bool> hoverFeedbackHoverState = new BaseVariable<bool>(false);
        public readonly BaseVariable<string> hoverFeedbackButton = new BaseVariable<string>();
        public readonly BaseVariable<string> hoverFeedbackText = new BaseVariable<string>();
    }
}