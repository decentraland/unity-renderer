using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

internal class ViewPool<T> : IDisposable where T : MonoBehaviour
{
    T baseView;
    Queue<T> viewPool = new Queue<T>();

    public ViewPool(T baseView, int prewarm = 0)
    {
        this.baseView = Object.Instantiate(baseView, baseView.transform.parent);
        this.baseView.gameObject.SetActive(false);

        PoolView(baseView);

        for (int i = 0; i < prewarm - 1; i++)
        {
            PoolView(CreateView());
        }
    }

    public void Dispose()
    {
        while (viewPool.Count > 0)
        {
            var obj = viewPool.Dequeue();

            if (obj != null)
                Object.Destroy(obj.gameObject);
        }

        Object.Destroy(baseView.gameObject);
    }

    public T GetView()
    {
        T ret = viewPool.Count > 0 ? viewPool.Dequeue() : CreateView();
        return ret;
    }

    public void PoolView(T cellView)
    {
        cellView.gameObject.SetActive(false);
        viewPool.Enqueue(cellView);
    }

    public Queue<T>.Enumerator GetEnumerator() { return viewPool.GetEnumerator(); }

    T CreateView() { return Object.Instantiate(baseView, baseView.transform.parent); }
}