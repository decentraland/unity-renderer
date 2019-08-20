using UnityEngine;

namespace DCL
{
    public class MessageThrottlingController
    {
        public static bool VERBOSE = false;

        const float DOWN_BASE_RATE = 0.0004f;
        const float DOWN_ACCELERATION_RATE = 1.005f;

        const float UP_BASE_RATE = 0.0001f;
        const float UP_ACCELERATION_RATE = 1.0001f;

        const float TIME_BUDGET_MIN = 0.0001f;
        const float SECS_TO_FLUSH_ALL_MESSAGES = 15f;

        const float FIXED_ESTIMATED_MESSAGES_PER_SECOND = 100;

        long lastProcessedMessages;

        float sampleCount;
        float mpsSum;

        public float messagesConsumptionRate;

        public MessageThrottlingController()
        {
            currentTimeBudget = 0.1f;
            counterup = 0.1f;
            counterdown = 0.1f;
        }

        public float currentTimeBudget { get; private set; }
        public float counterup { get; private set; }
        public float counterdown { get; private set; }

        public float Update(int pendingMsgsCount, long processedMsgsCount, float maxBudget = 0.5f)
        {
            if (processedMsgsCount > 100)
            {
                float processedDelta = processedMsgsCount - lastProcessedMessages;

                sampleCount++;

                mpsSum += 1f / (Time.deltaTime / processedDelta);
                float mpsAvg = mpsSum / sampleCount;

                float estimatedMessagesPerSecond = processedMsgsCount > 300 ? mpsAvg : FIXED_ESTIMATED_MESSAGES_PER_SECOND;
                bool noMorePendingMessages = pendingMsgsCount <= 0.1f;

                if (estimatedMessagesPerSecond > (pendingMsgsCount / SECS_TO_FLUSH_ALL_MESSAGES) || noMorePendingMessages)
                {
                    counterdown *= DOWN_ACCELERATION_RATE;
                    counterup = 1;

                    currentTimeBudget -= DOWN_BASE_RATE * counterdown;

                    if (currentTimeBudget < TIME_BUDGET_MIN)
                        currentTimeBudget = TIME_BUDGET_MIN;
                }
                else
                {
                    counterup *= UP_ACCELERATION_RATE;
                    counterdown = 1f;

                    currentTimeBudget += UP_BASE_RATE * counterup;

                    if (currentTimeBudget > maxBudget)
                        currentTimeBudget = maxBudget;
                }
            }
            else
            {
                currentTimeBudget = maxBudget;
            }

            lastProcessedMessages = processedMsgsCount;

            if (VERBOSE)
            {
                float gltfBudget = UnityGLTF.GLTFSceneImporter.budgetPerFrameInMilliseconds;
                float msgBudget = currentTimeBudget * 1000;
                float totalBudget = gltfBudget + msgBudget;

                if (pendingMsgsCount > 0)
                {
                    float targetThrottling = currentTimeBudget;
                    Debug.Log($"Metrics -- pending msg count: {pendingMsgsCount} // Budget -- Total: {totalBudget} ... GLTF: {gltfBudget} ... msg: {msgBudget} ... targetThrottling: {targetThrottling * 1000f}");
                }
            }

            return currentTimeBudget;
        }
    }
}
