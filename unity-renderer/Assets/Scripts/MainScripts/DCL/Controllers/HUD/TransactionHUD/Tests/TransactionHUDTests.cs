using NUnit.Framework;
using System.Collections;
using DCL;
using DCL.Helpers;
using UnityEngine.TestTools;
using DCL.TransactionHUDModel;

namespace Tests
{
    public class TransactionHUDTests : IntegrationTestSuite
    {
        private TransactionHUDController controller;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            MainSceneFactory.CreateBridges();

            TestUtils.CreateTestScene();
            
            controller = new TransactionHUDController();
            controller.Initialize();
        }
        
        protected override IEnumerator TearDown()
        {
            yield return base.TearDown();

            controller.Dispose();
        }

        [Test]
        public void TransactionHUD_ModelDefaulted()
        {
            Assert.IsNotNull(controller.model);
            Assert.IsNotNull(controller.model.transactions);
            Assert.AreEqual(controller.model.transactions.Count, 0);
        }

        [UnityTest]
        public IEnumerator TransactionHUD_ShowTransaction()
        {
            Model model = new Model()
            {
                id = "1",
                requestType = Type.SIGN_MESSAGE,
                message = "message to sign",
            };

            controller.ShowTransaction(model);

            yield return null;
            
            TransactionHUD[] transactions = controller.view.gameObject.GetComponentsInChildren<TransactionHUD>();
            Assert.AreEqual(transactions.Length, 1);

            TransactionHUD n = transactions[0];
            Assert.AreEqual(n.model.id, model.id);
            Assert.AreEqual(n.model.requestType, model.requestType);
            Assert.AreEqual(n.model.message, model.message);
        }

        [UnityTest]
        public IEnumerator TransactionHUD_ShowSeveralTransactions()
        {
            Model model = new Model()
            {
                id = "1",
                requestType = Type.SIGN_MESSAGE,
                message = "message to sign",
            };

            controller.ShowTransaction(model);

            Model model2 = new Model()
            {
                id = "2",
                requestType = Type.REQUIRE_PAYMENT,
                toAddress = "0x11110000000000000000000000000000000002222",
                currency = "MANA",
                amount = 10.0f
            };

            controller.ShowTransaction(model2);

            Model model3 = new Model()
            {
                id = "3",
                requestType = Type.SEND_ASYNC,
                message = "transfer(0x11110000000000000000000000000000000002222, 123)",
            };

            controller.ShowTransaction(model3);

            yield return null;

            TransactionHUD[] transactions = controller.view.gameObject.GetComponentsInChildren<TransactionHUD>();
            Assert.AreEqual(3, transactions.Length);

            TransactionHUD n;
            n = transactions[0];
            Assert.AreEqual(n.model.id, model.id);
            Assert.AreEqual(n.model.requestType, model.requestType);
            Assert.AreEqual(n.model.message, model.message);
            
            n = transactions[1];
            Assert.AreEqual(n.model.id, model2.id);
            Assert.AreEqual(n.model.requestType, model2.requestType);
            Assert.AreEqual(n.model.message, model2.message);
            
            n = transactions[2];
            Assert.AreEqual(n.model.id, model3.id);
            Assert.AreEqual(n.model.requestType, model3.requestType);
            Assert.AreEqual(n.model.message, model3.message);
        }
    }
}