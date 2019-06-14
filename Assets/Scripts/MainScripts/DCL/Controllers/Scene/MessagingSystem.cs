using UnityEngine;

namespace DCL
{
    public class MessagingBusId
    {
        public const string CHAT = "CHAT";
        public const string UI = "UI";
        public const string INIT = "INIT";
        public const string SYSTEM = "SYSTEM";
    }

    public class MessagingSystem
    {

        public MessagingBus bus;
        public MessageThrottlingController throttler;

        float budgetMin = 0;

        public float Update(float prevTimeBudget)
        {
            bus.timeBudget = throttler.Update(
                pendingMsgsCount: bus.pendingMessagesCount,
                processedMsgsCount: bus.processedMessagesCount,
                maxBudget: Mathf.Max(budgetMin, bus.budgetMax - prevTimeBudget)
            );

            return bus.timeBudget;
        }

        public MessagingSystem(IMessageHandler handler, float budgetMin = 0.01f, float budgetMax = 0.1f)
        {
            this.budgetMin = budgetMin;
            this.bus = new MessagingBus(handler, budgetMax);
            this.throttler = new MessageThrottlingController();
        }
    }
}

