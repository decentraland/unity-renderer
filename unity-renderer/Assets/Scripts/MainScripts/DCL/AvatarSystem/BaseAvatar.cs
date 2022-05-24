using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public class BaseAvatar : IBaseAvatar
    {

        private GameObject baseCharacter;

        public BaseAvatar(GameObject baseCharacter) 
        {
            this.baseCharacter = baseCharacter;
        }

        public void Initialize() 
        {
            Debug.Log("Init");
            if(baseCharacter != null)
                baseCharacter.SetActive(true);
            
            FadeIn();
        }

        public void FadeIn() 
        {
            Debug.Log("Fade in");
        }

        public void FadeOut() 
        {
            Debug.Log("Fade out");
            if (baseCharacter != null)
                baseCharacter?.SetActive(false);
        }

    }
}
