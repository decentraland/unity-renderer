using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    [RequireComponent(typeof(StatsPanel))]
    public class UserBenchmarkController : MonoBehaviour, IBenchmarkController
    {
        public enum Columns
        {
            LABEL,
            VALUE,
        }

        public enum Rows
        {
            NONE,
            PROCESSED_MESSAGES,
            PENDING_MESSAGES,
            BREAK_0,
            CURRENT_SCENE,
            POLYGONS_VS_LIMIT,
            TEXTURES_VS_LIMIT,
            MATERIALS_VS_LIMIT,
            ENTITIES_VS_LIMIT,
            MESHES_VS_LIMIT,
            BODIES_VS_LIMIT,
            COMPONENT_OBJECTS_COUNT
        }

        const string BREAK_0_TEXT = "";

        const string PROCESSED_MESSAGES_TEXT = "Processed Messages";
        const string PENDING_MESSAGES_TEXT = "Pending on Queue";


        const string CURRENT_SCENE_TEST = "Scene Location:";
        const string POLYGON_VS_LIMIT_TEXT = "Poly Count";
        const string TEXTURES_VS_LIMIT_TEXT = "Textures Count";
        const string MATERIALS_VS_LIMIT_TEXT = "Materials Count";
        const string ENTITIES_VS_LIMIT_TEXT = "Entities Count";
        const string MESHES_VS_LIMIT_TEXT = "Meshes Count";
        const string BODIES_VS_LIMIT_TEXT = "Bodies Count";
        const string COMPONENT_OBJECTS_COUNT_TEXT = "Components Count";

        int totalMessagesCurrent;
        int totalMessagesGlobal;

        StatsPanel statsPanel;
        List<Columns> columnsList;
        List<Rows> rowsList;

        private void Awake()
        {
            StartProfiling();
        }

        private void OnDestroy()
        {
            StopProfiling();
        }

        public void Init()
        {
            this.statsPanel = GetComponent<StatsPanel>();

            columnsList = Enum.GetValues(typeof(Columns)).Cast<Columns>().ToList();
            rowsList = Enum.GetValues(typeof(Rows)).Cast<Rows>().ToList();

            statsPanel.PopulateTable(columnsList.Count, rowsList.Count, 240, 240);

            statsPanel.SetCellText((int) Columns.LABEL, (int) Columns.LABEL, "");

            //Init Labels
            statsPanel.SetCellText((int) Columns.LABEL, (int) Rows.PROCESSED_MESSAGES, PROCESSED_MESSAGES_TEXT);
            statsPanel.SetCellText((int) Columns.LABEL, (int) Rows.PENDING_MESSAGES, PENDING_MESSAGES_TEXT);

            statsPanel.SetCellText((int) Columns.LABEL, (int) Rows.BREAK_0, BREAK_0_TEXT);

            statsPanel.SetCellText((int) Columns.LABEL, (int) Rows.CURRENT_SCENE, CURRENT_SCENE_TEST);
            statsPanel.SetCellText((int) Columns.LABEL, (int) Rows.POLYGONS_VS_LIMIT, POLYGON_VS_LIMIT_TEXT);
            statsPanel.SetCellText((int) Columns.LABEL, (int) Rows.TEXTURES_VS_LIMIT, TEXTURES_VS_LIMIT_TEXT);
            statsPanel.SetCellText((int) Columns.LABEL, (int) Rows.MATERIALS_VS_LIMIT, MATERIALS_VS_LIMIT_TEXT);
            statsPanel.SetCellText((int) Columns.LABEL, (int) Rows.ENTITIES_VS_LIMIT, ENTITIES_VS_LIMIT_TEXT);
            statsPanel.SetCellText((int) Columns.LABEL, (int) Rows.MESHES_VS_LIMIT, MESHES_VS_LIMIT_TEXT);
            statsPanel.SetCellText((int) Columns.LABEL, (int) Rows.BODIES_VS_LIMIT, BODIES_VS_LIMIT_TEXT);
            statsPanel.SetCellText((int) Columns.LABEL, (int) Rows.COMPONENT_OBJECTS_COUNT, COMPONENT_OBJECTS_COUNT_TEXT);

            //Init Values

            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.PROCESSED_MESSAGES, "=/=");
            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.PENDING_MESSAGES, "=/=");

            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.CURRENT_SCENE, "=/=");
            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.POLYGONS_VS_LIMIT, "=/=");
            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.TEXTURES_VS_LIMIT, "=/=");
            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.MATERIALS_VS_LIMIT, "=/=");
            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.ENTITIES_VS_LIMIT, "=/=");
            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.MESHES_VS_LIMIT, "=/=");
            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.BODIES_VS_LIMIT, "=/=");
            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.COMPONENT_OBJECTS_COUNT, "=/=");

            Canvas.ForceUpdateCanvases();
        }

        public void StartProfiling()
        {
            if (statsPanel == null)
            {
                Init();
            }

            profilingCoroutine = CoroutineStarter.Start(RefreshProfilingData());
            ProfilingEvents.OnMessageWillQueue += OnMessageWillQueue;
            ProfilingEvents.OnMessageWillDequeue += OnMessageWillDequeue;
        }

        private void OnMessageWillDequeue(string obj)
        {
            totalMessagesCurrent = Math.Min(totalMessagesCurrent + 1, totalMessagesGlobal);
            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.PROCESSED_MESSAGES, $"{totalMessagesCurrent} of {totalMessagesGlobal}");
            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.PENDING_MESSAGES, $"{totalMessagesGlobal - totalMessagesCurrent}");
        }

        private void OnMessageWillQueue(string obj)
        {
            totalMessagesGlobal++;
            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.PROCESSED_MESSAGES, $"{totalMessagesCurrent}:{totalMessagesGlobal}");
            statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.PENDING_MESSAGES, $"{totalMessagesGlobal - totalMessagesCurrent}");
        }

        private Coroutine profilingCoroutine;

        public void StopProfiling()
        {
            if (profilingCoroutine != null)
                CoroutineStarter.Stop(profilingCoroutine);

            ProfilingEvents.OnMessageWillQueue -= OnMessageWillQueue;
            ProfilingEvents.OnMessageWillDequeue -= OnMessageWillDequeue;
        }

        private IEnumerator RefreshProfilingData()
        {
            while (true)
            {
                var currentPos = Utils.WorldToGridPosition(DCLCharacterController.i.characterPosition.worldPosition);

                var activeScene = Environment.i.worldState.loadedScenes.Values.FirstOrDefault(x => x.sceneData.parcels != null && x.sceneData.parcels.Any(y => y == currentPos));
                if (activeScene != null && activeScene.metricsController != null)
                {
                    var metrics = activeScene.metricsController.GetModel();
                    var limits = activeScene.metricsController.GetLimits();
                    statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.CURRENT_SCENE, $"{activeScene.sceneData.id}");
                    statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.POLYGONS_VS_LIMIT, $"{metrics.triangles} of {limits.triangles}");
                    statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.TEXTURES_VS_LIMIT, $"{metrics.textures} of {limits.textures}");
                    statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.MATERIALS_VS_LIMIT, $"{metrics.materials} of {limits.materials}");
                    statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.ENTITIES_VS_LIMIT, $"{metrics.entities} of {limits.entities}");
                    statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.MESHES_VS_LIMIT, $"{metrics.meshes} of {limits.meshes}");
                    statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.BODIES_VS_LIMIT, $"{metrics.bodies} of {limits.bodies}");
                    statsPanel.SetCellText((int) Columns.VALUE, (int) Rows.COMPONENT_OBJECTS_COUNT, activeScene.disposableComponents.Count + activeScene.entities.Select(x => x.Value.components.Count).Sum().ToString());
                }

                yield return WaitForSecondsCache.Get(0.2f);
            }
        }
    }
}