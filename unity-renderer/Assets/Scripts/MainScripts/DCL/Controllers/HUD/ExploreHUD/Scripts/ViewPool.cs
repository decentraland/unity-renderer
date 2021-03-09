using System.Collections.Generic;
using UnityEngine;
using System;

internal class ViewPool<T> : IDisposable where T : MonoBehaviour
{
    T baseView;
    Queue<T> pooledHotScenCells = new Queue<T>();

    public ViewPool(T baseView, int prewarm = 0)
    {
        this.baseView = GameObject.Instantiate(baseView, baseView.transform.parent);
        this.baseView.gameObject.SetActive(false);

        PoolView(baseView);
        for (int i = 0; i < prewarm - 1; i++)
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
        GameObject.Destroy(baseView.gameObject);
    }

    public T GetView()
    {
        T ret = pooledHotScenCells.Count > 0 ? pooledHotScenCells.Dequeue() : CreateView();
        return ret;
    }

    public void PoolView(T cellView)
    {
        cellView.gameObject.SetActive(false);
        pooledHotScenCells.Enqueue(cellView);
    }

    public Queue<T>.Enumerator GetEnumerator()
    {
        return pooledHotScenCells.GetEnumerator();
    }

    T CreateView()
    {
        return GameObject.Instantiate(baseView, baseView.transform.parent);
    }
}
