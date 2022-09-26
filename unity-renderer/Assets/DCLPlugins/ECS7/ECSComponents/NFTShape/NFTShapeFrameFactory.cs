using UnityEngine;

namespace DCL.ECSComponents
{
    public interface INFTShapeFrameFactory
    {
        /// <summary>
        /// Return the instance of a NFT shape frame based on the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        NFTShapeFrame InstantiateLoaderController(int index);

        /// <summary>
        /// This will instantiate an error feedback and it will be attached to gameObject as a child
        /// </summary>
        /// <param name="gameObjectToAttach"></param>
        /// <returns></returns>
        GameObject InstantiateErrorFeedback(GameObject gameObjectToAttach);
    }
    
    public class NFTShapeFrameFactory : ScriptableObject, INFTShapeFrameFactory
    {
        [SerializeField] internal NFTShapeFrame[] loaderControllersPrefabs;
        [SerializeField] private NFTShapeFrame defaultFramePrefabs;
        [SerializeField] private GameObject errorFeedback;
        
        public NFTShapeFrame InstantiateLoaderController(int index)
        {
            if (index >= 0 && index < loaderControllersPrefabs.Length)
                return Instantiate(loaderControllersPrefabs[index]);
            
            return Instantiate(defaultFramePrefabs);
        }

        public GameObject InstantiateErrorFeedback(GameObject gameObjectToAttach)
        {
            return Instantiate(errorFeedback,gameObjectToAttach.transform); 
        }
    }
}