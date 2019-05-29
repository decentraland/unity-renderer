using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class PerfCounter<T>
    {
        public T[] elements;
        RedBlackTree<T> rbTree;

        public int frameRange;
        int index = 0;

        public PerfCounter(IComparer<T> comparer, int size)
        {
            elements = new T[size];
            rbTree = new RedBlackTree<T>(comparer);
            frameRange = size;
        }

        public void AddMeasurement(T value)
        {
            T dummy;
            try
            {
                rbTree.Delete(elements[index], true, out dummy);
            }
            catch (Exception any)
            {
            }

            rbTree.Insert(value, DuplicatePolicy.InsertFirst, out dummy);
            if (index == frameRange)
            {
                index = 0;
            }

            elements[index++] = value;
        }

        public T GetPercentile(int percentile)
        {
            if (percentile == 100)
            {
                return rbTree.GetItemByIndex(rbTree.ElementCount - 1);
            }

            int element = (int)System.Math.Floor(percentile * rbTree.ElementCount / 100f);
            return rbTree.GetItemByIndex(element);
        }

        public int GetSampleSize()
        {
            return rbTree.ElementCount;
        }
    }

    public class FloatComparer : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            return x == y ? 0 : x < y ? 1 : -1;
        }
    }

    public class FrameTimeCounter : MonoBehaviour
    {
        public float[] stats = new float[6];

        bool first = true;
        float lastTime = 0;
        float delay = 5.0f;

        PerfCounter<float> perf;

        void Start()
        {
            perf = new PerfCounter<float>(new FloatComparer(), 2000);
        }

        void Update()
        {
            delay -= Time.deltaTime;

            if (delay > 0)
            {
                return;
            }

            if (!first && lastTime < Time.time)
            {
                perf.AddMeasurement(Time.time - lastTime);
            }

            lastTime = Time.time;

            first = false;

            if (perf.GetSampleSize() < 100)
            {
                return;
            }

            UpdateStats();
        }

        void UpdateStats()
        {
            stats[0] = perf.GetPercentile(50);
            stats[1] = perf.GetPercentile(25);
            stats[2] = perf.GetPercentile(10);
            stats[3] = perf.GetPercentile(5);
            stats[4] = perf.GetPercentile(1);
            stats[5] = perf.GetPercentile(0);
        }
    }
}