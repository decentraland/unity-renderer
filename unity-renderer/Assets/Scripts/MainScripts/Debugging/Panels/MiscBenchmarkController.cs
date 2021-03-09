using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace DCL
{
    [RequireComponent(typeof(StatsPanel))]
    public class MiscBenchmarkController : MonoBehaviour, IBenchmarkController
    {
        public enum Columns
        {
            NONE,
            VALUE,
        }

        public enum Rows
        {
            NONE,
            SHARED_OBJECTS_COUNT,
            COMPONENT_OBJECTS_COUNT,
            ENTITY_OBJECTS_COUNT,
            BREAK_0,
            MATERIAL_COUNT,
            MESHES_COUNT,
            BREAK_1,
            GLTF_BEING_LOADED,
            AB_BEING_LOADED,
            MESSAGES_PER_SECOND_REAL,
            RENDERER_UNLOCK_SEGS,
            MESSAGE_BUSES,
        }

        const string BREAK_0_TEXT = "";
        const string BREAK_1_TEXT = "";

        const string RENDERER_UNLOCK_TEXT = "Time Until Renderer Enable";
        const string SHARED_OBJECTS_COUNT_TEXT = "Shared Objects Count";
        const string COMPONENT_OBJECTS_COUNT_TEXT = "Components Count";
        const string ENTITY_OBJECTS_COUNT_TEXT = "Entity Count";
        const string MATERIAL_COUNT_TEXT = "Material Count";
        const string MESHES_COUNT_TEXT = "Meshes Count";
        const string GLTF_BEING_LOADED_TEXT = "GLTFs active requests";
        const string AB_BEING_LOADED_TEXT = "Asset bundles active requests";
        const string MESSAGES_PER_SECOND_REAL_TEXT = "Messages x sec";
        const string MESSAGES_BUSES_TEXT = "Message buses";

        StatsPanel statsPanel;
        List<Columns> columnsList;
        List<Rows> rowsList;

        int lastPendingMessages;
        int sampleCount = 0;
        float mps = 0;

        public void Init()
        {
            this.statsPanel = GetComponent<StatsPanel>();

            columnsList = Enum.GetValues(typeof(Columns)).Cast<Columns>().ToList();
            rowsList = Enum.GetValues(typeof(Rows)).Cast<Rows>().ToList();

            statsPanel.PopulateTable(columnsList.Count, rowsList.Count, 240, 240);

            //NOTE(Brian): Top-left cell, unused.
            statsPanel.SetCellText(0, 0, "");

            //NOTE(Brian): Row stuff (left vertical header)
            statsPanel.SetCellText(0, (int) Rows.SHARED_OBJECTS_COUNT, SHARED_OBJECTS_COUNT_TEXT);
            statsPanel.SetCellText(0, (int) Rows.COMPONENT_OBJECTS_COUNT, COMPONENT_OBJECTS_COUNT_TEXT);
            statsPanel.SetCellText(0, (int) Rows.ENTITY_OBJECTS_COUNT, ENTITY_OBJECTS_COUNT_TEXT);
            statsPanel.SetCellText(0, (int) Rows.BREAK_0, BREAK_0_TEXT);
            statsPanel.SetCellText(0, (int) Rows.MATERIAL_COUNT, MATERIAL_COUNT_TEXT);
            statsPanel.SetCellText(0, (int) Rows.MESHES_COUNT, MESHES_COUNT_TEXT);
            statsPanel.SetCellText(0, (int) Rows.BREAK_1, BREAK_1_TEXT);
            statsPanel.SetCellText(0, (int) Rows.GLTF_BEING_LOADED, GLTF_BEING_LOADED_TEXT);
            statsPanel.SetCellText(0, (int) Rows.AB_BEING_LOADED, AB_BEING_LOADED_TEXT);
            statsPanel.SetCellText(0, (int) Rows.MESSAGES_PER_SECOND_REAL, MESSAGES_PER_SECOND_REAL_TEXT);
            statsPanel.SetCellText(0, (int) Rows.RENDERER_UNLOCK_SEGS, RENDERER_UNLOCK_TEXT);
            statsPanel.SetCellText(0, (int) Rows.MESSAGE_BUSES, MESSAGES_BUSES_TEXT);
        }

        private Coroutine updateCoroutine;

        public void StartProfiling()
        {
            if (enabled)
            {
                return;
            }

            if (statsPanel == null)
            {
                Init();
            }

            updateCoroutine = CoroutineStarter.Start(RefreshProfilingData());
            enabled = true;
        }

        public void StopProfiling()
        {
            if (!enabled)
            {
                return;
            }

            CoroutineStarter.Stop(updateCoroutine);
            enabled = false;
        }

        static float budgetMax = 0;

        void Update()
        {
            MessagingControllersManager messagingManager = Environment.i.messaging.manager as MessagingControllersManager;

            int messagesProcessedLastFrame = lastPendingMessages - messagingManager.pendingMessagesCount;

            if (messagesProcessedLastFrame > 0)
            {
                sampleCount++;
                mps += 1 / (Time.deltaTime / messagesProcessedLastFrame);
                statsPanel.SetCellText(1, (int) Rows.MESSAGES_PER_SECOND_REAL, (mps / sampleCount).ToString(CultureInfo.InvariantCulture));
            }

            lastPendingMessages = messagingManager.pendingMessagesCount;
        }


        private IEnumerator RefreshProfilingData()
        {
            while (true)
            {
                int sharedCount = 0;
                int sharedAttachCount = 0;
                int componentCount = 0;
                int entityCount = 0;
                int materialCount = 0;
                int meshesCount = 0;

                var loadedScenes = Environment.i.world.state.loadedScenes;

                foreach (var v in loadedScenes)
                {
                    if (v.Value.metricsController != null)
                    {
                        meshesCount += v.Value.metricsController.GetModel().meshes;
                        materialCount += v.Value.metricsController.GetModel().materials;
                    }

                    sharedCount += v.Value.disposableComponents.Count;

                    foreach (var e in v.Value.disposableComponents)
                    {
                        sharedAttachCount += e.Value.attachedEntities.Count;
                    }

                    entityCount += v.Value.entities.Count;

                    foreach (var e in v.Value.entities)
                    {
                        componentCount += e.Value.components.Count;
                    }
                }

                statsPanel.SetCellText(1, (int) Rows.SHARED_OBJECTS_COUNT, sharedCount.ToString());
                statsPanel.SetCellText(1, (int) Rows.COMPONENT_OBJECTS_COUNT, componentCount.ToString());
                statsPanel.SetCellText(1, (int) Rows.ENTITY_OBJECTS_COUNT, entityCount.ToString());
                statsPanel.SetCellText(1, (int) Rows.MATERIAL_COUNT, materialCount.ToString());
                statsPanel.SetCellText(1, (int) Rows.MESHES_COUNT, meshesCount.ToString());
                statsPanel.SetCellText(1, (int) Rows.GLTF_BEING_LOADED, UnityGLTF.GLTFComponent.downloadingCount.ToString() + " ... In Queue: " + UnityGLTF.GLTFComponent.queueCount.ToString());
                statsPanel.SetCellText(1, (int) Rows.AB_BEING_LOADED, AssetPromise_AB.downloadingCount.ToString() + " ...  In Queue: " + AssetPromise_AB.queueCount.ToString());
                statsPanel.SetCellText(1, (int) Rows.RENDERER_UNLOCK_SEGS, RenderingController.firstActivationTime.ToString(CultureInfo.InvariantCulture));

                string busesLog = "";
                Dictionary<string, int> pendingMessagesCount = new Dictionary<string, int>();
                Dictionary<string, int> messagesReplaced = new Dictionary<string, int>();

                MessagingControllersManager messagingManager = Environment.i.messaging.manager as MessagingControllersManager;

                using (var controllersIter = messagingManager.messagingControllers.GetEnumerator())
                {
                    while (controllersIter.MoveNext())
                    {
                        using (var iterator = controllersIter.Current.Value.messagingBuses.GetEnumerator())
                        {
                            while (iterator.MoveNext())
                            {
                                //access to pair using iterator.Current
                                MessagingBusType key = iterator.Current.Key;
                                MessagingBus bus = controllersIter.Current.Value.messagingBuses[key];

                                string keyString = key.ToString();

                                if (!pendingMessagesCount.ContainsKey(keyString))
                                    pendingMessagesCount[keyString] = 0;

                                if (!messagesReplaced.ContainsKey(keyString))
                                    messagesReplaced[keyString] = 0;

                                pendingMessagesCount[keyString] += bus.pendingMessagesCount;
                                messagesReplaced[keyString] += bus.unreliableMessagesReplaced;
                            }
                        }
                    }
                }

                busesLog += $"{MessagingBusType.UI.ToString()} bus: {pendingMessagesCount[MessagingBusType.UI.ToString()]} replaced: {messagesReplaced[MessagingBusType.UI.ToString()]}\n";
                busesLog += $"{MessagingBusType.INIT.ToString()} bus: {pendingMessagesCount[MessagingBusType.INIT.ToString()]} replaced: {messagesReplaced[MessagingBusType.INIT.ToString()]}\n";
                busesLog += $"{MessagingBusType.SYSTEM.ToString()} bus: {pendingMessagesCount[MessagingBusType.SYSTEM.ToString()]} replaced: {messagesReplaced[MessagingBusType.SYSTEM.ToString()]}\n";

                statsPanel.SetCellText(1, (int) Rows.MESSAGE_BUSES, busesLog);

                yield return WaitForSecondsCache.Get(0.2f);
            }
        }
    }
}