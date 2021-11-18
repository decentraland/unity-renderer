using System;
using UnityEngine;

namespace DCL
{
    public class ThrottlingCounter
    {
        public static bool VERBOSE = false;
        public DCL.Logger logger = new DCL.Logger("ThrottlingCounter") { verboseEnabled = VERBOSE };

        public bool enabled = true;
        public double budgetPerFrame { get => enabled ? budgetPerFrameValue : double.MaxValue; set => budgetPerFrameValue = value; }
        public double evaluationTimeElapsedCap = 100 / 1000.0;
        private double budgetPerFrameValue = 2 / 1000.0;
        private double timeBudgetCounter = 0f;

        public bool EvaluateTimeBudget(double elapsedTime)
        {
            if ( elapsedTime <= 0 )
                return false;

            elapsedTime = Math.Min( elapsedTime, evaluationTimeElapsedCap );
            timeBudgetCounter += elapsedTime;

            if ( timeBudgetCounter > budgetPerFrame )
            {
                // We don't set the timeBudgetCounter to zero to avoid compounding of precision errors.
                timeBudgetCounter -= budgetPerFrame;

                logger.Verbose($"Elapsed: {elapsedTime * 1000} - Counter: {timeBudgetCounter * 1000} - Total: {budgetPerFrame * 1000} - (Skipping frame)");
                return true;
            }

            logger.Verbose($"Elapsed: {elapsedTime * 1000} - Counter: {timeBudgetCounter * 1000} - Total: {budgetPerFrame * 1000} - (Not skipping frame)");
            return false;
        }

        public void Reset()
        {
            timeBudgetCounter = 0;
        }
    }
}