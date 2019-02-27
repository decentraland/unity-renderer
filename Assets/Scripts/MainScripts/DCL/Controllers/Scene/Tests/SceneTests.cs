using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Configuration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SceneTests
    {
        [UnityTest]
        public IEnumerator PerformanceLimitControllerTests()
        {
            var sceneController = TestHelpers.InitializeSceneController();
            var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

            yield return new WaitForSeconds(.1f);

            sceneController.UnloadAllScenes();

            yield return new WaitForSeconds(.1f);

            sceneController.LoadParcelScenes(scenesToLoad);

            yield return new WaitForSeconds(.1f);

            var scene = sceneController.loadedScenes["0,0"];

            TestHelpers.InstantiateEntityWithShape(scene, "1", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(-3, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "2", DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(0, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "3", DCL.Models.CLASS_ID.PLANE_SHAPE, new Vector3(2, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "4", DCL.Models.CLASS_ID.CONE_SHAPE, new Vector3(4, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "5", DCL.Models.CLASS_ID.CYLINDER_SHAPE, new Vector3(6, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "6", DCL.Models.CLASS_ID.GLTF_SHAPE, new Vector3(0, 1, 6), TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb");
            TestHelpers.InstantiateEntityWithShape(scene, "7", DCL.Models.CLASS_ID.OBJ_SHAPE, new Vector3(10, 1, 0), TestHelpers.GetTestsAssetsPath() + "/OBJ/teapot.obj");
            TestHelpers.InstantiateEntityWithShape(scene, "8", DCL.Models.CLASS_ID.GLTF_SHAPE, new Vector3(0, 1, 12), TestHelpers.GetTestsAssetsPath() + "/GLB/CesiumMan/CesiumMan.glb");

            yield return new WaitForSeconds(8f);

            AssertMetricsModel(scene,
                triangles: 17521,
                materials: 5,
                entities: 8,
                meshes: 11,
                bodies: 11,
                textures: 0);

            TestHelpers.RemoveSceneEntity(scene, "8");

            yield return new WaitForSeconds(.1f);

            AssertMetricsModel(scene,
                triangles: 12849,
                materials: 4,
                entities: 7,
                meshes: 10,
                bodies: 10,
                textures: 0);

            yield return null;
        }

        void AssertMetricsModel(ParcelScene scene, int triangles, int materials, int entities, int meshes, int bodies, int textures)
        {
            SceneMetricsController.Model inputModel = scene.metricsController.GetModel();

            Assert.AreEqual(triangles, inputModel.triangles, "Incorrect triangle count, was: " + triangles);
            Assert.AreEqual(materials, inputModel.materials, "Incorrect materials count");
            Assert.AreEqual(entities, inputModel.entities, "Incorrect entities count");
            Assert.AreEqual(meshes, inputModel.meshes, "Incorrect geometries/meshes count");
            Assert.AreEqual(bodies, inputModel.bodies, "Incorrect bodies count");
            Assert.AreEqual(textures, inputModel.textures, "Incorrect textures count");
        }

        [UnityTest]
        public IEnumerator SceneLoading()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            sceneController.LoadParcelScenes((Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text);

            yield return null;

            string loadedSceneID = "0,0";

            Assert.IsTrue(sceneController.loadedScenes.ContainsKey(loadedSceneID));

            Assert.IsTrue(sceneController.loadedScenes[loadedSceneID] != null);
        }

        [UnityTest]
        public IEnumerator SceneUnloading()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            sceneController.LoadParcelScenes((Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text);

            yield return null;

            string loadedSceneID = "0,0";

            Assert.IsTrue(sceneController.loadedScenes.ContainsKey(loadedSceneID));

            // Add 1 entity to the loaded scene
            TestHelpers.CreateSceneEntity(sceneController.loadedScenes[loadedSceneID], "6");

            var sceneRootGameObject = sceneController.loadedScenes[loadedSceneID];
            var sceneEntities = sceneController.loadedScenes[loadedSceneID].entities;

            sceneController.UnloadScene(loadedSceneID);

            yield return new WaitForSeconds(0.01f); // We wait to let unity destroy gameobjects.

            Assert.IsTrue(sceneController.loadedScenes.ContainsKey(loadedSceneID) == false);

            Assert.IsTrue(sceneRootGameObject == null, "Scene root gameobject reference is not getting destroyed.");

            Assert.AreEqual(sceneEntities.Count, 0, "Every entity should be removed");
        }

        [UnityTest]
        public IEnumerator SeveralParcelsFromJSON()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var jsonMessageToLoad = "{\"parcelsToLoad\":[{\"id\":\"-102,100\",\"basePosition\":{\"x\":-102,\"y\":100},\"parcels\":[{\"x\":-102,\"y\":100}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm39j2mCramRURmogUMqtrXNHtZ3z8taShBRLinPZGWmjFzKnL2A1JTortYo77aUYETPmRoHJdn2qhYqWk3acKoqnW\"},{\"file\":\"game.ts\",\"hash\":\"Qm318JHUVnzsZKA8fUeuSvcz6VepGg3oJVryiUwRb3QnDJq4HL5HFRKZKocUtDB96HUwWZReRXPeR9KiLc9q1h2f5Y\"},{\"file\":\"scene.json\",\"hash\":\"Qm4DSDqaibeuwWkXNX6BsafcMU2BhUU64V2MoUxtrqDMpzXFwEUQ1FPLdHNysYCnPhVVpQ6XgTvxCfz4KikaPwkmq1\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm2ko8HNC9m8yHfqsuH76wsZYhhJajEYRVFYkypCsJQKeM18MyN5E3PfE2pLqARqnxD3UNApvg1XYnYFxw8Rkdouhm\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,100\",\"basePosition\":{\"x\":-100,\"y\":100},\"parcels\":[{\"x\":-100,\"y\":100}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm5S3QXrcnq8ScRCw1cwWe3SAWr5c5Mh5eEFgRnayVxp26HwS2mBqEpHMmSP45czoDn6UQ4zT3sETy7ABFU4Q4yiiF\"},{\"file\":\"game.ts\",\"hash\":\"Qm4XSDU2cnWFDFrK74SjmCuysW8SUScozPvbwomXL2AKaNbYC8P33UMqr8EXAxzhdT4bFCXJ1K8z74hMp5RxbcLznV\"},{\"file\":\"scene.json\",\"hash\":\"Qm51UBhp1EsViXFLyX8cpVLqYrafJoVdCr7Ux1j7dB9u7DwCV9gsLZSmjLewHTCkMn2AH7ttnNwYnv5u2g7Q94RNtc\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm5uXQmcazfvBDw1cwortCTRXvhVz4Tx9SzfjQV5NpEbxnaRCtgoDe3cxvwbZBdujZpUkATmmYCAfH1Q69P4oPS2Z6\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,101\",\"basePosition\":{\"x\":-100,\"y\":101},\"parcels\":[{\"x\":-100,\"y\":101}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm48ZepPVVTRFNZXU6Cuu11xSNLc4AWWDf4cXb7zucRMm2VMAWmm8JdXLWfTwJP2T1K1NAWkJJjX185TYbzdqEXUiQ\"},{\"file\":\"game.ts\",\"hash\":\"QmjhK1mT7Q5QRHVtTbMScVdwTSDBXxUuYeyhdHbTYwKMrDLqg8JM9S7vTFUmaaP5FqTVaEruyrHiBrJDcZ9T87gfx\"},{\"file\":\"scene.json\",\"hash\":\"Qm2wa4G47XM6c4zvk2j4hdfSgVPmmS1rnLA1xiJ13kZ4VHBYsBieHkPQ2Eh5FV8hZcUddoMJkbkAvMJA7v8AggsFcN\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm3m45w9a87xaBQk2RCk23F81zjmeHPJ5k11sYnRKzmYUFqbt7Y3yA1ARshwyuTjEVq4x8z6QfJCuEDA2aQH994V7X\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,102\",\"basePosition\":{\"x\":-100,\"y\":102},\"parcels\":[{\"x\":-100,\"y\":102}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"QmgnZBWtSZW76ViogXckDG4qVbUei7tSnCZBp8FdybYFs8GvqjC5vBNg3Bk4KwksprrD9HMVyHqX96kc5ZCwkU45d\"},{\"file\":\"game.ts\",\"hash\":\"Qm4QJQqWvFf3QVMFNdcTWS84CKLCiL2mFbPfp1GJq7NkwKdgY59f5MsLwyhw3xntgQvWbourEuxhJ42qBuAnsWYQmB\"},{\"file\":\"scene.json\",\"hash\":\"Qm2NRxf8pDoMEiKSeNCBGE57F8RCKWYuB8Jin3RpMgEa4w5YCasQfw4ReGPkKbgSFK99XX8GvzSegoqXWjAUnnRKyq\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm8yAaa84utvn1JiemEqiRUf5TmqHKtFBr5AJFcgyhCxrN36zEBWxqtQbHp7S2GRKMdo8sZvi3xvxbgoBnMtkJHfD\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,103\",\"basePosition\":{\"x\":-100,\"y\":103},\"parcels\":[{\"x\":-100,\"y\":103}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm2Mxae9qDm3zxxaj9TiRGYK29mPGKM5ffrBgvfAsarCs9VXLTRRERDibn165TtS7emP3zJ16Co9rYLTYQEety6fyb\"},{\"file\":\"game.ts\",\"hash\":\"Qm5xR3gEV9qppTqUwro1p9oWffMFopyZ1ejnN8rnvQ3gArLh8QMZtezNp82wtE4K4rzvGYpHS4Z3DoonnR19YmzB2z\"},{\"file\":\"scene.json\",\"hash\":\"Qm2vCKt6Vc3eBkHSZ8xjLwpGVwNfqxRDghcsaknMCtKrJE3jeA3KEFqkK9HRK5RDaeH6N8vosd1f7hf3VxBQuZCu5L\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm3JnfFM9xzeeaj2LVAr65sCmUupRcMoHkS4XEgKrwhcmDipzqkLSZKyvpQjxd8F7vu92MMsnqbna23KWcTDaNqGak\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,104\",\"basePosition\":{\"x\":-100,\"y\":104},\"parcels\":[{\"x\":-100,\"y\":104}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"QmwS5L1VbRn7agFYf5W8wx75SrKL4ayF3bYKxMrGq9M4sipT2isc8souk3WBLy2SvBgSU7RyktSLVF7eSVzeR1atC\"},{\"file\":\"game.ts\",\"hash\":\"Qm3KU6Ne5UXCDR4kfuchoRXBJNh85WHn25eX7STRGpfRz56iFza6sPioGc7C9HHEPBDWNJGgPh4nf3zt7XHsXAWAez\"},{\"file\":\"scene.json\",\"hash\":\"Qm5tamJRukS9EqfGhTgCvF1Dz9hFKKh7WT1kFzLZbJpwETBe9Ak8qMfFvXZSyQ7mtQN3BPUKWXp3FRa1G8XDjmYhor\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm4CCi8esaBNe6JTGhPFafSFZMdHyhyEwpniLvpA5ENbBJ3skyT9bQZtXYV9VwGSEkcbRag9ngp4dzaf1ENVVarM72\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,105\",\"basePosition\":{\"x\":-100,\"y\":105},\"parcels\":[{\"x\":-100,\"y\":105}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"atlas.png\",\"hash\":\"Qm3HiKF3yPmxg6kGtBZmNeDxL5GhHwaAqr3eeg49k2EWmh1geRWjVTFjLBCptun2q4uKdohqy2BwMpk6KNBQRTQNcT\"},{\"file\":\"game.js\",\"hash\":\"Qm5M4pYTXht64o156etmchHkxgkQevoFyE6NbLGQzoPS25RfCbsUykXU419k2m81Axkn96yS4Qipqz72cUuhjJKvva\"},{\"file\":\"game.ts\",\"hash\":\"Qm4wDCXm9hhQzJD3FJm1dNdgmK8W82jgEcxCKFLv48NyNBMpsx28PTJjzej6ZrUTy87RT3TVytJKKBv8D73FmFvKcw\"},{\"file\":\"scene.json\",\"hash\":\"Qm2TC7GgDnxKPnixHN6YxjGM6VV3pkHxKKC1Cqzvjzb5kcUWriHD6xpkNVPuef7EntUCt8XvWSYGaV2DwrsHP7cAHF\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm53AtLjaoYbsZvcnwgDv3erd85pnL8zR7WbrQxEnPusfkZ4ysCaLhqBbn99EvdvAvN5U6ZfzJjYdQRjefwaRzsHjd\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,106\",\"basePosition\":{\"x\":-100,\"y\":106},\"parcels\":[{\"x\":-100,\"y\":106}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm4c5CSSu37s55Hd21jRCrBSn1x6bdPT3UGQyshtR9WAwGftGgrsTAK6uEd4H6RV2DdZtRyojQzjKQMyonMCrPLxop\"},{\"file\":\"game.ts\",\"hash\":\"Qm3qB6u32Tzqd1K7Rfu4ZATzgjTtrxPu6FBENiHUNZUrj66wochUYFpoXAqXUXeufnPZTn12RZsT2nbDANXWFevqJg\"},{\"file\":\"scene.json\",\"hash\":\"Qm4MpHJooeFESeQfMZaHiVoE1NhLHpYvSUctnHjUn3L9qR9V4nA9wg3bcXQPoLkXuR31Q9uJDVGnj7MPtmzgyyMBMk\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm4mXeoioxNryxn1741bHi16JtLsVYB6PLrbo7T6LAd9ZYfK4bupRyNRioxvr6fUXnHNDy7W2HHysPrqn4nVxEgHkT\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,107\",\"basePosition\":{\"x\":-100,\"y\":107},\"parcels\":[{\"x\":-100,\"y\":107}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm2FcLJwy5fGZjw7bss2HQiXAB1TGZpoQDhGDFfwBfgwtq9E98VgwZzYgubfvdqoppp61T1ydkZXZvDbV3j2eXuMa4\"},{\"file\":\"game.ts\",\"hash\":\"Qm4urxWNwqg115gcE3QCLZUNMMTNeSpfpVcXhW73dr1cjwySUMuwbduLL7QH94KA84iG3cy7SBBwMHSgmZ8URzj97h\"},{\"file\":\"scene.json\",\"hash\":\"Qm382D2nHx8swSYXaHPtETjpEwkqx35PRF1pMXZktLSHJ5tCTzEUZYYTVMbmzNKZH8ZF6CX9mpe5THBAhj4wcRzxuv\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm4v2uPzypAHRP6UPb19Ed12pLi8hM6hq2kbyndYU386sKHPRTZzs5g2AjTV6CPDNqakS8hnbhZRhzTkZyZQE98NQH\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,108\",\"basePosition\":{\"x\":-100,\"y\":108},\"parcels\":[{\"x\":-100,\"y\":108}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"game.js\",\"hash\":\"Qm4x4T311KqBj67T8wgSKiC5QndfdySRaTLgcSHDKE5Ccqpu7xmB6KYcuWJAUvQYaWcgHc8UTqbJ9g8BhWDmemzR1\"},{\"file\":\"game.ts\",\"hash\":\"Qm2PJ6somc5ijqzjcVe7vxERKpkPHyYEUUFysGrkzCXL6BjabDGVb2fzxgC5nS5hDApSHGBYg7HYG4W4wnhLmxmfDY\"},{\"file\":\"scene.json\",\"hash\":\"Qm51RXkydt57AeYgs9p8B1daURhH5AmbTFc9NFJA5X1eo3goE8qzwTcCHDuc63hi8C9VAU3zaL1NBjj3XAZnPNMbAC\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm2tWYaRPqP9Wh7e7EKdWbjuX34gUPh57gQAE8yCK3jbmMxabJVZybVwiAXiQUfFMgpYjxGWCZ8YEQiBJs8ZzrUdjz\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"},{\"id\":\"-100,109\",\"basePosition\":{\"x\":-100,\"y\":109},\"parcels\":[{\"x\":-100,\"y\":109}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[{\"file\":\"bubble.png\",\"hash\":\"Qm2XNpLngcqqH4Wgy6jDqL3SrthNdUGiAftrdctMZkvvJLT7s1JT4ffGp4oWWRDiGf8vAb5FxSXMjidT6HmbinoY91\"},{\"file\":\"game.js\",\"hash\":\"Qm3YBdwfioGe1Zbd69VnqnDTm497crH6b5fpe7J4gTS69KR53gUdSxnNar5C89F9qN8PnefJZDyujFvHgcF4jACY1X\"},{\"file\":\"game.ts\",\"hash\":\"Qm48ngfKEEtDPfrxSVkdCtNEApNMNTnUPJ8WeFLvdGaGBgFXg9HQnETnz8Z4r8E8th5GREUobBjL164YBK14NHNnFM\"},{\"file\":\"scene.json\",\"hash\":\"Qm5vb5xVA4JENeFXBCJg7npTkYm1eG4gfpHj2GARdMgrsAtcuHqA3PTA3UpWeND9Em7Emgx8YmzbLfaiqDyiXNpS8X\"},{\"file\":\"tsconfig.json\",\"hash\":\"Qm2Rqm29ZNLdFD9Zak72w2k7fUL8dp6G6TNYFLFpCBaNga9dBd4MNfbeVA4EooxZN1bqkt6eXTT8HwJK6XmAsqToyZ\"}],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}]}";

            Assert.AreEqual(sceneController.loadedScenes.Count, 0);
            sceneController.LoadParcelScenes(jsonMessageToLoad);

            yield return null;

            var referenceCheck = new System.Collections.Generic.List<DCL.Controllers.ParcelScene>();

            foreach (var kvp in sceneController.loadedScenes)
            {
                referenceCheck.Add(kvp.Value);
            };

            Assert.AreEqual(sceneController.loadedScenes.Count, 11);

            sceneController.LoadParcelScenes(jsonMessageToLoad);

            Assert.AreEqual(sceneController.loadedScenes.Count, 11);

            foreach (var reference in referenceCheck)
            {
                Assert.IsTrue(sceneController.loadedScenes.ContainsValue(reference), "References must be the same");
            };
        }

        [UnityTest]
        public IEnumerator PositionParcels()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var jsonMessageToLoad = "{\"parcelsToLoad\":[{\"id\":\"xxx\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}]}";

            DCL.Configuration.Environment.DEBUG = true;

            Assert.AreEqual(sceneController.loadedScenes.Count, 0);
            sceneController.LoadParcelScenes(jsonMessageToLoad);

            yield return null;

            Assert.AreEqual(sceneController.loadedScenes.Count, 1);

            var theScene = sceneController.loadedScenes["xxx"];

            Assert.AreEqual(theScene.sceneData.parcels.Length, 3);
            Assert.AreEqual(theScene.transform.childCount, 3);

            Assert.IsTrue(theScene.transform.GetChild(0).localPosition == new Vector3(-ParcelSettings.PARCEL_SIZE / 2, DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE / 2));
            Assert.IsTrue(theScene.transform.GetChild(1).localPosition == new Vector3(ParcelSettings.PARCEL_SIZE / 2, DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE / 2));
            Assert.IsTrue(theScene.transform.GetChild(2).localPosition == new Vector3(-ParcelSettings.PARCEL_SIZE / 2, DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE + ParcelSettings.PARCEL_SIZE / 2));
        }

        [UnityTest]
        public IEnumerator PositionParcels2()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var jsonMessageToLoad = "{\"parcelsToLoad\":[{\"id\":\"xxx\",\"basePosition\":{\"x\":90,\"y\":90},\"parcels\":[{\"x\":89,\"y\":90}, {\"x\":90,\"y\":90}, {\"x\":89,\"y\":91}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}]}";

            DCL.Configuration.Environment.DEBUG = true;

            Assert.AreEqual(sceneController.loadedScenes.Count, 0);
            sceneController.LoadParcelScenes(jsonMessageToLoad);

            yield return null;

            Assert.AreEqual(sceneController.loadedScenes.Count, 1);

            var theScene = sceneController.loadedScenes["xxx"];

            Assert.AreEqual(theScene.sceneData.parcels.Length, 3);
            Assert.AreEqual(theScene.transform.childCount, 3);

            Assert.IsTrue(theScene.transform.GetChild(0).localPosition == new Vector3(-ParcelSettings.PARCEL_SIZE / 2, DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE / 2));
            Assert.IsTrue(theScene.transform.GetChild(1).localPosition == new Vector3(ParcelSettings.PARCEL_SIZE / 2, DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE / 2));
            Assert.IsTrue(theScene.transform.GetChild(2).localPosition == new Vector3(-ParcelSettings.PARCEL_SIZE / 2, DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE + ParcelSettings.PARCEL_SIZE / 2));
        }
    }
}
