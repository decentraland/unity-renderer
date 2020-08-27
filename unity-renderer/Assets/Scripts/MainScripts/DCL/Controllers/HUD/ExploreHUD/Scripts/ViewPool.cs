using System.Collections.Generic;
using UnityEngine;
using System;

internal class ViewPool<T> : IDisposable where T : MonoBehaviour
{
    T baseView;
    Queue<T> pooledHotScenCells = new Queue<T>();

    public ViewPool(T baseView, int prewarm = 0)
    {
        this.baseView = baseView;

        PoolView(baseView);
        for (int i = 0; i < prewarm; i++)
        {
            PoolView(CreateView());
        }
    }

    public void Dispose()
    {
        while (pooledHotScenCells.Count > 0)
        {
            var obj = pooledHotScenCells.Dequeue();
            if (obj != null)
            {
                GameObject.Destroy(obj.gameObject);
            }
        }
    }

    public T GetView()
    {
        T ret;
        if (pooledHotScenCells.Count > 0)
        {
            ret = pooledHotScenCells.Dequeue();
        }
        ret = CreateView();
        ret.gameObject.SetActive(false);
        return ret;
    }

    public void PoolView(T cellView)
    {
        cellView.gameObject.SetActive(false);
        pooledHotScenCells.Enqueue(cellView);
    }

    T CreateView()
    {
        return GameObject.Instantiate(baseView, baseView.transform.parent);
    }
}
