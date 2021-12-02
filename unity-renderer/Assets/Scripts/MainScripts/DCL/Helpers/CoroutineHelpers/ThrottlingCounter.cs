using System;
using UnityEngine;

namespace DCL
{
    public class ThrottlingCounter
    {
        public bool enabled = true;
        public double budgetPerFrame { get => enabled ? budgetPerFrameValue : double.MaxValue; set => budgetPerFrameValue = value; }
        private double budgetPerFrameValue = 2 / 1000.0;
        private double timeBudgetCounter = 0f;

        /// <summary>
        /// EvaluateTimeBudget decrements an internal time budget counter according to the given elapsedTime.
        /// The method returns a bool value indicating that the time budget has been exceeded. When this happens, a frame should be skipped.
        /// </summary>
        /// <param name="elapsedTime">The elapsed time in seconds.</param>
        /// <returns>True if a frame skip is needed</returns>
        public bool EvaluateTimeBudget(double elapsedTime)
        {
            if ( elapsedTime <= 0 )
                return false;

            timeBudgetCounter += elapsedTime;

            if ( timeBudgetCounter > budgetPerFrame )
            {
                timeBudgetCounter = 0;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            timeBudgetCounter = 0;
        }
    }
}