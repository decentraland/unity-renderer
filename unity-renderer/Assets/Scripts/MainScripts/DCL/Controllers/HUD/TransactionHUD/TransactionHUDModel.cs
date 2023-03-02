using System;

namespace DCL.TransactionHUDModel
{
    public enum Type
    {
        REQUIRE_PAYMENT,
        SIGN_MESSAGE,
        SEND_ASYNC
    }

    public class Model
    {
        public string id;
        public Type requestType;
        public int sceneNumber;
        public string message;
        public string toAddress;
        public float amount;
        public string currency;
    }
}