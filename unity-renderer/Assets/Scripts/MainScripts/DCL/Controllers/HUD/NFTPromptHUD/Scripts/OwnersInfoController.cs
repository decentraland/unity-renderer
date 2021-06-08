using System;
using System.Collections.Generic;
using DCL.Helpers.NFT;
using Object = UnityEngine.Object;

internal class OwnersInfoController : IDisposable
{
    private readonly Queue<IOwnerInfoElement> elementsPool = new Queue<IOwnerInfoElement>();
    private readonly List<IOwnerInfoElement> activeElements = new List<IOwnerInfoElement>();
    private readonly OwnerInfoElement ownerElementPrefab;

    public OwnersInfoController(OwnerInfoElement ownerElementPrefab) { this.ownerElementPrefab = ownerElementPrefab; }

    public void Dispose()
    {
        while (elementsPool.Count > 0)
        {
            elementsPool.Dequeue().Dispose();
        }
        for (int i = 0; i < activeElements.Count; i++)
        {
            activeElements[i].Dispose();
        }
    }

    public void SetOwners(NFTInfoSingleAsset.Owners[] owners)
    {
        int activeElementsCount = activeElements.Count;
        for (int i = activeElementsCount - 1; i >= 0; i--)
        {
            if (i < owners.Length)
            {
                activeElements[i].SetOwner(owners[i].owner);
            }
            else
            {
                PoolElement(activeElements[i]);
                activeElements.RemoveAt(i);
            }
        }

        if (activeElementsCount < owners.Length)
        {
            for (int i = activeElementsCount; i < owners.Length; i++)
            {
                var element = GetElement();
                element.SetOwner(owners[i].owner);
                activeElements.Add(element);
            }
        }
    }

    public List<IOwnerInfoElement> GetElements() { return activeElements; }

    private IOwnerInfoElement GetElement()
    {
        IOwnerInfoElement ret;
        if (elementsPool.Count > 0)
        {
            ret = elementsPool.Dequeue();
        }
        else
        {
            ret = Object.Instantiate(ownerElementPrefab);
        }
        return ret;
    }

    private void PoolElement(IOwnerInfoElement element)
    {
        elementsPool.Enqueue(element);
        element.SetActive(false);
    }
}