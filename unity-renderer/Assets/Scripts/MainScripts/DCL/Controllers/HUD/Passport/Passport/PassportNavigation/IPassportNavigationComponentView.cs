namespace DCL.Social.Passports
{
    public interface IPassportNavigationComponentView
    {
        void SetGuestUser(bool isGuest);
        void SetName(string username);

        void SetDescription(string description);
    }
}
