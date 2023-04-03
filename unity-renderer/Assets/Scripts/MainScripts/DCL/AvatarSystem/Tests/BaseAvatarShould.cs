using AvatarSystem;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Test.AvatarSystem
{
    public class BaseAvatarShould
    {
        private GameObject container;
        private BaseAvatar baseAvatar;
        [SetUp]
        public void SetUp()
        {
            container = new GameObject();

            baseAvatar = new BaseAvatar(new BaseAvatarReferences
            {
                armatureContainer = container.transform,
            });
        }


    }
}
