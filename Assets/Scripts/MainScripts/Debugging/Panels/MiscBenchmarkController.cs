using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        MESSAGES_PER_SECOND_REAL,
    }

    const string BREAK_0_TEXT = "";
    const string BREAK_1_TEXT = "";

    const string SHARED_OBJECTS_COUNT_TEXT = "Shared Objects Count";
    const string COMPONENT_OBJECTS_COUNT_TEXT = "Components Count";
    const string ENTITY_OBJECTS_COUNT_TEXT = "Entity Count";
    const string MATERIAL_COUNT_TEXT = "Material Count";
    const string MESHES_COUNT_TEXT = "Meshes Count";
    const string GLTF_BEING_LOADED_TEXT = "GLTFs being loaded";
    const string MESSAGES_PER_SECOND_REAL_TEXT = "Messages x sec";

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

        statsPanel.PopulateTable(columnsList.Count, rowsList.Count);

        //NOTE(Brian): Top-left cell, unused.
        statsPanel.SetCellText(0, 0, "");

        //NOTE(Brian): Column stuff (top horizontal header)
        statsPanel.SetCellText((int)Columns.NONE, 0, "");

        //NOTE(Brian): Row stuff (left vertical header)
        statsPanel.SetCellText(0, (int)Rows.SHARED_OBJECTS_COUNT, SHARED_OBJECTS_COUNT_TEXT);
        statsPanel.SetCellText(0, (int)Rows.COMPONENT_OBJECTS_COUNT, COMPONENT_OBJECTS_COUNT_TEXT);
        statsPanel.SetCellText(0, (int)Rows.ENTITY_OBJECTS_COUNT, ENTITY_OBJECTS_COUNT_TEXT);
        statsPanel.SetCellText(0, (int)Rows.BREAK_0, BREAK_0_TEXT);
        statsPanel.SetCellText(0, (int)Rows.MATERIAL_COUNT, MATERIAL_COUNT_TEXT);
        statsPanel.SetCellText(0, (int)Rows.MESHES_COUNT, MESHES_COUNT_TEXT);
        statsPanel.SetCellText(0, (int)Rows.BREAK_1, BREAK_1_TEXT);
        statsPanel.SetCellText(0, (int)Rows.GLTF_BEING_LOADED, GLTF_BEING_LOADED_TEXT);
        statsPanel.SetCellText(0, (int)Rows.MESSAGES_PER_SECOND_REAL, MESSAGES_PER_SECOND_REAL_TEXT);
    }

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

        SceneController.i.StartCoroutine(RefreshProfilingData());
        enabled = true;
    }

    public void StopProfiling()
    {
        if (!enabled)
        {
            return;
        }

        SceneController.i.StopCoroutine(RefreshProfilingData());
        enabled = false;
    }

    void Update()
    {
        int messagesProcessedLastFrame = lastPendingMessages - SceneController.i.pendingMessagesCount;

        if (messagesProcessedLastFrame > 0)
        {
            sampleCount++;
            mps += 1 / (Time.deltaTime / messagesProcessedLastFrame);
            statsPanel.SetCellText(1, (int)Rows.MESSAGES_PER_SECOND_REAL, (mps / sampleCount).ToString());
        }

        lastPendingMessages = SceneController.i.pendingMessagesCount;
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

            foreach (var v in SceneController.i.loadedScenes)
            {
                meshesCount += v.Value.metricsController.GetModel().meshes;
                materialCount += v.Value.metricsController.GetModel().materials;

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

            statsPanel.SetCellText(1, (int)Rows.SHARED_OBJECTS_COUNT, sharedCount.ToString());
            statsPanel.SetCellText(1, (int)Rows.COMPONENT_OBJECTS_COUNT, componentCount.ToString());
            statsPanel.SetCellText(1, (int)Rows.ENTITY_OBJECTS_COUNT, entityCount.ToString());
            statsPanel.SetCellText(1, (int)Rows.MATERIAL_COUNT, materialCount.ToString());
            statsPanel.SetCellText(1, (int)Rows.MESHES_COUNT, meshesCount.ToString());
            statsPanel.SetCellText(1, (int)Rows.GLTF_BEING_LOADED, UnityGLTF.GLTFComponent.downloadingCount.ToString() + " / " + UnityGLTF.GLTFComponent.queueCount.ToString());
            yield return new WaitForSeconds(0.2f);
        }
    }
}
