namespace Altom.AltDriver
{
    [System.Serializable]
    public class AltAxis
    {
        public string name;
        public string negativeButton;
        public string positiveButton;
        public string altPositiveButton;
        public string altNegativeButton;

        public AltAxis(string name, string negativeButton, string positiveButton, string altPositiveButton, string altNegativeButton)
        {
            this.name = name;
            this.negativeButton = negativeButton;
            this.positiveButton = positiveButton;
            this.altPositiveButton = altPositiveButton;
            this.altNegativeButton = altNegativeButton;
        }
    }
}
