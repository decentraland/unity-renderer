using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using NSubstitute;
using NUnit.Framework;
using RPC.Context;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class CrdtExecutorsManagerShould
    {
        private Dictionary<int, ICRDTExecutor> crdtExecutors;
        private ECSComponentsManager componentsManager;
        private ISceneController sceneController;
        private CRDTServiceContext rpcCrdtServiceContext;
        private CrdtExecutorsManager executorsManager;
        private ECS7TestUtilsScenesAndEntities testUtils;

        [SetUp]
        public void SetUp()
        {
            crdtExecutors = new Dictionary<int, ICRDTExecutor>();
            componentsManager = new ECSComponentsManager(new Dictionary<int, ECSComponentsFactory.ECSComponentBuilder>());
            rpcCrdtServiceContext = new CRDTServiceContext();
            sceneController = Substitute.For<ISceneController>();
            executorsManager = new CrdtExecutorsManager(crdtExecutors, componentsManager, sceneController, rpcCrdtServiceContext);
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager);
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
            DataStore.Clear();
        }

        [Test]
        public void DisposeCorrectly()
        {
            crdtExecutors.Add(1, new CRDTExecutor(testUtils.CreateScene(1), componentsManager));
            crdtExecutors.Add(2, new CRDTExecutor(testUtils.CreateScene(2), componentsManager));
            crdtExecutors.Add(3, new CRDTExecutor(testUtils.CreateScene(3), componentsManager));

            executorsManager.Dispose();
            Assert.AreEqual(0, crdtExecutors.Count);
        }

        [Test]
        public void RemoveCrdtExecutorOnSceneUnload()
        {
            const int sceneNumber = 666;
            IParcelScene scene = testUtils.CreateScene(sceneNumber);

            crdtExecutors.Add(sceneNumber, new CRDTExecutor(scene, componentsManager));

            sceneController.OnSceneRemoved += Raise.Event<Action<IParcelScene>>(scene);
            Assert.AreEqual(0, crdtExecutors.Count);
        }

        [Test]
        public void IgnoreCrdtMessageWhenSceneNotLoaded()
        {
            const int sceneNumber = 666;
            rpcCrdtServiceContext.CrdtMessageReceived.Invoke(sceneNumber, new CRDTMessage());
            LogAssert.Expect(LogType.Error, $"CrdtExecutor not found for sceneNumber {sceneNumber}");
        }

        [Test]
        public void ReceiveCrdtMessage()
        {
            const int sceneNumber = 666;
            ECS7TestScene scene = testUtils.CreateScene(sceneNumber);
            scene.crdtExecutor = null;

            CRDTMessage crdtMessage = new CRDTMessage()
            {
                timestamp = 1,
                data = new byte[0],
                key1 = 1,
                key2 = 2
            };

            rpcCrdtServiceContext.CrdtMessageReceived.Invoke(sceneNumber, crdtMessage);
            CRDTMessage crtState = crdtExecutors[sceneNumber].crdtProtocol.GetState(crdtMessage.key1, crdtMessage.key2);
            AssertCrdtMessageEqual(crdtMessage, crtState);
        }

        [Test]
        public void UseCachedExecutorWhenSeveralMessagesFromSameScene()
        {
            const int sceneNumber = 666;
            ECS7TestScene scene = testUtils.CreateScene(sceneNumber);
            scene.crdtExecutor = null;

            CRDTMessage crdtMessage1 = new CRDTMessage()
            {
                timestamp = 1,
                data = new byte[0],
                key1 = 1,
                key2 = 2
            };

            CRDTMessage crdtMessage2 = new CRDTMessage()
            {
                timestamp = 1,
                data = new byte[0],
                key1 = 2,
                key2 = 2
            };

            // Send first message
            rpcCrdtServiceContext.CrdtMessageReceived.Invoke(sceneNumber, crdtMessage1);
            ICRDTExecutor sceneExecutor = crdtExecutors[sceneNumber];

            // Clear executors dictionary
            crdtExecutors.Clear();

            // Send second message for same scene
            rpcCrdtServiceContext.CrdtMessageReceived.Invoke(sceneNumber, crdtMessage2);

            CRDTMessage crtStateMsg1 = sceneExecutor.crdtProtocol.GetState(crdtMessage1.key1, crdtMessage1.key2);
            CRDTMessage crtStateMsg2 = sceneExecutor.crdtProtocol.GetState(crdtMessage2.key1, crdtMessage2.key2);
            AssertCrdtMessageEqual(crdtMessage1, crtStateMsg1);
            AssertCrdtMessageEqual(crdtMessage2, crtStateMsg2);
        }

        static void AssertCrdtMessageEqual(CRDTMessage crdt1, CRDTMessage crdt2)
        {
            Assert.AreEqual(crdt1.timestamp, crdt2.timestamp);
            Assert.AreEqual(crdt1.key1, crdt2.key1);
            Assert.AreEqual(crdt1.key2, crdt2.key2);
            Assert.IsTrue(AreEqual((byte[])crdt1.data, (byte[])crdt2.data));
        }

        static bool AreEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null)
                return false;

            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }
    }
}
