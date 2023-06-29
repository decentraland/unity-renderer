using UnityEngine;

namespace DCL.Social.Passports
{
    public interface IAdditionalInfoFieldIconProvider
    {
        Sprite Get(AdditionalInfoField field);
    }
}
