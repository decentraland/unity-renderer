using UnityEngine;

public class TransactionBridge : MonoBehaviour
{
    public ITransactionHUDController transactionController;
    
    [System.Serializable]
    class RequestWeb3ApiUseMessage
    {
        [System.Serializable]
        public class RequestWeb3ApiUsePayload
        {
            // public string sceneId;
            public int sceneNumber;
            public string message;
            public string toAddress;
            public float amount;
            public string currency;
        }
        public string id;
        public string requestType;
        public RequestWeb3ApiUsePayload payload;
    }

    public void RequestWeb3ApiUse(string payload)
    {
        if (transactionController == null)
            return;
        
        var model = JsonUtility.FromJson<RequestWeb3ApiUseMessage>(payload);

        var requestType = DCL.TransactionHUDModel.Type.REQUIRE_PAYMENT;
        switch (model.requestType)
        {
            case "requirePayment":
                requestType = DCL.TransactionHUDModel.Type.REQUIRE_PAYMENT;
                break;
            case "signMessage":
                requestType = DCL.TransactionHUDModel.Type.SIGN_MESSAGE;
                break;
            case "sendAsync":
                requestType = DCL.TransactionHUDModel.Type.SEND_ASYNC;
                break;
        }

        transactionController.ShowTransaction(new DCL.TransactionHUDModel.Model
        {
            id = model.id,
            requestType = requestType,
            // sceneId = model.payload.sceneId,
            sceneNumber = model.payload.sceneNumber,
            message = model.payload.message,
            toAddress = model.payload.toAddress,
            amount = model.payload.amount,
            currency = model.payload.currency
        });
    }
}
