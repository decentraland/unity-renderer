using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public interface IBenchmarkController
    {
        void StartProfiling();
        void StopProfiling();
    }

    [RequireComponent(typeof(StatsPanel))]
    public class MessageBenchmarkController : MonoBehaviour, IBenchmarkController
    {
        public static MessageBenchmarkController i { get; private set; }

        public enum Columns
        {
            NONE,
            LOGIC_MS,
            DECODE_MS,
            QUEUED_COUNT,
            RATIO,
            TOTAL_TIME,
            PROCESSED_COUNT,
        }

        public enum Rows
        {
            HEADER,
            TOTAL,
            BREAK_0,
            SCENE_LOAD,
            SCENE_DESTROY,
            BREAK_1,
            ENTITY_CREATE,
            ENTITY_DESTROY,
            ENTITY_REPARENT,
            BREAK_2,
            ENTITY_COMPONENT_CREATE,
            ENTITY_COMPONENT_DESTROY,
            BREAK_3,
            SHARED_COMPONENT_CREATE,
            SHARED_COMPONENT_UPDATE,
            SHARED_COMPONENT_DISPOSE,
            SHARED_COMPONENT_ATTACH,
            BREAK_4,
            MISC,
        }

        const string COLUMN_MESSAGE_TYPE_TEXT = "Message type";
        const string COLUMN_LOGIC_MS_TEXT = "Process";
        const string COLUMN_DECODE_MS_TEXT = "Decode";
        const string COLUMN_QUEUED_COUNT_TEXT = "Queued";
        const string COLUMN_PROCESSED_COUNT_TEXT = "Processed";
        const string COLUMN_RATIO_TEXT = "Ratio";
        const string COLUMN_RATIO_TIMES_MS_TEXT = "Total Time";

        const string ROW_TOTAL_TEXT = "Total";

        const string ROW_ENTITY_CREATE_TEXT = "Entity Create";
        const string ROW_ENTITY_DESTROY_TEXT = "Entity Destroy";
        const string ROW_ENTITY_REPARENT_TEXT = "Entity Reparent";

        const string ROW_SCENE_LOAD_TEXT = "Scene Load";
        const string ROW_SCENE_DESTROY_TEXT = "Scene Destroy";

        const string ROW_ENTITY_COMPONENT_CREATE_TEXT = "Entity Comp. Create / Update";
        const string ROW_ENTITY_COMPONENT_UPDATE_TEXT = "";
        const string ROW_ENTITY_COMPONENT_DESTROY_TEXT = "Entity Component Destroy";

        const string ROW_SHARED_COMPONENT_CREATE_TEXT = "Shared Component Create";
        const string ROW_SHARED_COMPONENT_UPDATE_TEXT = "Shared Component Update";
        const string ROW_SHARED_COMPONENT_DISPOSE_TEXT = "Shared Component Destroy";
        const string ROW_SHARED_COMPONENT_ATTACH_TEXT = "Shared Component Attach";
        const string ROW_MISC_TEXT = "Misc";

        StatsPanel statsPanel;

        Dictionary<(Columns, Rows), float> statsTracker;
        Dictionary<Rows, float> processTimeTracker;
        Dictionary<Rows, float> decodeTimeTracker;
        List<Columns> columnsList;
        List<Rows> rowsList;
        Dictionary<string, Rows> stringToRow;

        public float messagesTotalTime
        {
            get { return logicTotalTime + decodeTotalTime; }
        }

        private float logicTotalTime;
        private float decodeTotalTime;
        private string msgBeingProcessed;

        void Init()
        {
            this.statsPanel = GetComponent<StatsPanel>();

            columnsList = Enum.GetValues(typeof(Columns)).Cast<Columns>().ToList();
            rowsList = Enum.GetValues(typeof(Rows)).Cast<Rows>().ToList();

            statsPanel.PopulateTable(columnsList.Count - 1, rowsList.Count);

            ResetTracker();

            //NOTE(Brian): Top-left cell, unused.
            statsPanel.SetCellText(0, 0, "");

            //NOTE(Brian): Column stuff (top horizontal header)
            statsPanel.SetCellText((int) Columns.LOGIC_MS, 0, COLUMN_LOGIC_MS_TEXT);
            statsPanel.SetCellText((int) Columns.DECODE_MS, 0, COLUMN_DECODE_MS_TEXT);
            statsPanel.SetCellText((int) Columns.QUEUED_COUNT, 0, COLUMN_QUEUED_COUNT_TEXT);
            statsPanel.SetCellText((int) Columns.RATIO, 0, COLUMN_RATIO_TEXT);
            statsPanel.SetCellText((int) Columns.TOTAL_TIME, 0, COLUMN_RATIO_TIMES_MS_TEXT);

            //NOTE(Brian): Row stuff (left vertical header)
            statsPanel.SetCellText(0, (int) Rows.TOTAL, ROW_TOTAL_TEXT);
            statsPanel.SetCellText(0, (int) Rows.BREAK_0, "");
            statsPanel.SetCellText(0, (int) Rows.SCENE_LOAD, ROW_SCENE_LOAD_TEXT);
            statsPanel.SetCellText(0, (int) Rows.SCENE_DESTROY, ROW_SCENE_DESTROY_TEXT);
            statsPanel.SetCellText(0, (int) Rows.BREAK_1, "");
            statsPanel.SetCellText(0, (int) Rows.ENTITY_CREATE, ROW_ENTITY_CREATE_TEXT);
            statsPanel.SetCellText(0, (int) Rows.ENTITY_DESTROY, ROW_ENTITY_DESTROY_TEXT);
            statsPanel.SetCellText(0, (int) Rows.ENTITY_REPARENT, ROW_ENTITY_REPARENT_TEXT);
            statsPanel.SetCellText(0, (int) Rows.BREAK_2, "");
            statsPanel.SetCellText(0, (int) Rows.ENTITY_COMPONENT_CREATE, ROW_ENTITY_COMPONENT_CREATE_TEXT);
            statsPanel.SetCellText(0, (int) Rows.ENTITY_COMPONENT_DESTROY, ROW_ENTITY_COMPONENT_DESTROY_TEXT);
            statsPanel.SetCellText(0, (int) Rows.BREAK_3, "");
            statsPanel.SetCellText(0, (int) Rows.SHARED_COMPONENT_CREATE, ROW_SHARED_COMPONENT_CREATE_TEXT);
            statsPanel.SetCellText(0, (int) Rows.SHARED_COMPONENT_ATTACH, ROW_SHARED_COMPONENT_ATTACH_TEXT);
            statsPanel.SetCellText(0, (int) Rows.SHARED_COMPONENT_DISPOSE, ROW_SHARED_COMPONENT_DISPOSE_TEXT);
            statsPanel.SetCellText(0, (int) Rows.SHARED_COMPONENT_UPDATE, ROW_SHARED_COMPONENT_UPDATE_TEXT);
            statsPanel.SetCellText(0, (int) Rows.BREAK_4, "");
            statsPanel.SetCellText(0, (int) Rows.MISC, ROW_MISC_TEXT);

            stringToRow = new Dictionary<string, Rows>();

            stringToRow.Add("Total", Rows.TOTAL);
            stringToRow.Add("CreateEntity", Rows.ENTITY_CREATE);
            stringToRow.Add("SetEntityParent", Rows.ENTITY_REPARENT);
            stringToRow.Add("RemoveEntity", Rows.ENTITY_DESTROY);

            stringToRow.Add("UpdateEntityComponent", Rows.ENTITY_COMPONENT_CREATE);
            stringToRow.Add("ComponentRemoved", Rows.ENTITY_COMPONENT_DESTROY);

            stringToRow.Add("AttachEntityComponent", Rows.SHARED_COMPONENT_ATTACH);
            stringToRow.Add("ComponentCreated", Rows.SHARED_COMPONENT_CREATE);
            stringToRow.Add("ComponentDisposed", Rows.SHARED_COMPONENT_DISPOSE);
            stringToRow.Add("ComponentUpdated", Rows.SHARED_COMPONENT_UPDATE);

            stringToRow.Add("LoadScene", Rows.SCENE_LOAD);
            stringToRow.Add("UnloadScene", Rows.SCENE_DESTROY);
            stringToRow.Add("Misc", Rows.MISC);
        }

        private void Awake()
        {
            i = this;
        }

        public void StartProfiling()
        {
            if (enabled)
            {
                return;
            }

            enabled = true;

            if (statsPanel == null)
            {
                Init();
            }
            else
            {
                ResetTracker();
            }

            ProfilingEvents.OnMessageWillQueue += OnMessageWillQueue;
            ProfilingEvents.OnMessageWillDequeue += OnMessageWillDequeue;

            ProfilingEvents.OnMessageProcessStart += OnMessageProcessStart;
            ProfilingEvents.OnMessageProcessEnds += OnMessageProcessEnds;

            ProfilingEvents.OnMessageDecodeStart += OnMessageDecodeStart;
            ProfilingEvents.OnMessageDecodeEnds += OnMessageDecodeEnds;

            updateCoroutine = CoroutineStarter.Start(UpdateTotalMs());
        }

        private Coroutine updateCoroutine;

        public void StopProfiling()
        {
            if (!enabled)
            {
                return;
            }

            enabled = false;

            ProfilingEvents.OnMessageWillQueue -= OnMessageWillQueue;
            ProfilingEvents.OnMessageWillDequeue -= OnMessageWillDequeue;

            ProfilingEvents.OnMessageProcessStart -= OnMessageProcessStart;
            ProfilingEvents.OnMessageProcessEnds -= OnMessageProcessEnds;

            ProfilingEvents.OnMessageDecodeStart -= OnMessageDecodeStart;
            ProfilingEvents.OnMessageDecodeEnds -= OnMessageDecodeEnds;

            CoroutineStarter.Stop(updateCoroutine);
        }

        IEnumerator UpdateTotalMs()
        {
            while (true)
            {
                logicTotalTime = 0;
                decodeTotalTime = 0;

                foreach (var strRowPair in stringToRow)
                {
                    float processedCount = statsTracker[(Columns.PROCESSED_COUNT, strRowPair.Value)];

                    if (processedCount > 0)
                    {
                        logicTotalTime += statsTracker[(Columns.LOGIC_MS, strRowPair.Value)] / processedCount;
                        decodeTotalTime += statsTracker[(Columns.DECODE_MS, strRowPair.Value)] / processedCount;
                    }
                }

                statsPanel.SetCellText((int) Columns.LOGIC_MS, (int) Rows.TOTAL, logicTotalTime.ToString("N3", CultureInfo.InvariantCulture) + "ms");
                statsPanel.SetCellText((int) Columns.DECODE_MS, (int) Rows.TOTAL, decodeTotalTime.ToString("N3", CultureInfo.InvariantCulture) + "ms");
                yield return WaitForSecondsCache.Get(0.5f);
            }
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                ResetTracker();
            }
        }

        private void OnMessageDecodeStart(string s)
        {
            if (!stringToRow.ContainsKey(s))
            {
                return;
            }

            if (!string.IsNullOrEmpty(msgBeingProcessed) && stringToRow.ContainsKey(msgBeingProcessed))
            {
                s = msgBeingProcessed;
            }

            decodeTimeTracker[stringToRow[s]] = DCLTime.realtimeSinceStartup;
        }

        private void OnMessageDecodeEnds(string s)
        {
            if (!stringToRow.ContainsKey(s))
            {
                return;
            }

            if (!string.IsNullOrEmpty(msgBeingProcessed) && stringToRow.ContainsKey(msgBeingProcessed))
            {
                s = msgBeingProcessed;
            }

            float dt = DCLTime.realtimeSinceStartup - decodeTimeTracker[stringToRow[s]];

            AvgTrackedValue(Columns.DECODE_MS, stringToRow[s], dt * 1000);
        }

        private void OnMessageProcessEnds(string s)
        {
            if (!stringToRow.ContainsKey(s))
            {
                return;
            }

            statsTracker[(Columns.PROCESSED_COUNT, stringToRow[s])]++;
            statsTracker[(Columns.PROCESSED_COUNT, Rows.TOTAL)]++;

            //NOTE(Brian): Process ratio
            float current = statsTracker[(Columns.PROCESSED_COUNT, stringToRow[s])];
            float total = statsTracker[(Columns.PROCESSED_COUNT, Rows.TOTAL)];
            float finalRatio = current / total * 100;

            statsPanel.SetCellText((int) Columns.RATIO, (int) stringToRow[s], finalRatio.ToString("N1", CultureInfo.InvariantCulture) + "%");

            //NOTE(Brian): Process total time
            float totalSecs = statsTracker[(Columns.LOGIC_MS, stringToRow[s])] +
                              statsTracker[(Columns.DECODE_MS, stringToRow[s])];

            totalSecs /= 1000;

            statsPanel.SetCellText((int) Columns.TOTAL_TIME, (int) stringToRow[s], totalSecs.ToString("N2", CultureInfo.InvariantCulture) + "secs");

            float dt = DCLTime.realtimeSinceStartup - processTimeTracker[stringToRow[s]];

            //NOTE(Brian): Hack because I expect decode events to be called inside process events,
            //             with the exception of scene load one.
            if (stringToRow[s] != Rows.SCENE_LOAD)
            {
                statsTracker[(Columns.LOGIC_MS, stringToRow[s])] += dt * 1000;

                //NOTE(Brian): I substract decode from logic because <check last comment>
                int processedCount = (int) statsTracker[(Columns.PROCESSED_COUNT, stringToRow[s])];
                float avg = statsTracker[(Columns.LOGIC_MS, stringToRow[s])] -
                            statsTracker[(Columns.DECODE_MS, stringToRow[s])];

                if (processedCount > 0)
                {
                    avg /= processedCount;
                }

                statsPanel.SetCellText((int) Columns.LOGIC_MS, (int) stringToRow[s], avg.ToString("N3", CultureInfo.InvariantCulture) + "ms");
                msgBeingProcessed = string.Empty;
                return;
            }

            AvgTrackedValue(Columns.LOGIC_MS, stringToRow[s], dt * 1000);
            msgBeingProcessed = string.Empty;
        }

        private void OnMessageProcessStart(string s)
        {
            if (!stringToRow.ContainsKey(s))
            {
                return;
            }

            msgBeingProcessed = s;
            processTimeTracker[stringToRow[s]] = DCLTime.realtimeSinceStartup;
        }

        void SumTrackedValue(Columns x, Rows y, int delta)
        {
            statsTracker[(x, y)] += delta;
            statsPanel.SetCellText((int) x, (int) y, statsTracker[(x, y)].ToString(CultureInfo.InvariantCulture));
        }

        void AvgTrackedValue(Columns x, Rows y, float ms)
        {
            int processedCount = (int) statsTracker[(Columns.PROCESSED_COUNT, y)];
            float result = 0;

            if (processedCount > 0)
            {
                statsTracker[(x, y)] += ms;
                result = statsTracker[(x, y)] / processedCount;
            }
            else
            {
                //NOTE(Brian): If we aren't tracking TOTAL for this particular row, we use the overall msgs total
                //             for computing the avg. This is currently used by misc row.
                processedCount = (int) statsTracker[(Columns.PROCESSED_COUNT, Rows.TOTAL)];
                statsTracker[(x, y)] += ms;
                result = statsTracker[(x, y)] / (processedCount > 0 ? processedCount : 1.0f);
            }

            statsPanel.SetCellText((int) x, (int) y, result.ToString("N3", CultureInfo.InvariantCulture) + "ms");
        }

        private void OnMessageWillDequeue(string obj)
        {
            CountMessage(obj, -1);
        }

        private void OnMessageWillQueue(string obj)
        {
            CountMessage(obj, 1);
        }

        void CountMessage(string obj, int delta)
        {
            if (!stringToRow.ContainsKey(obj))
            {
                return;
            }

            SumTrackedValue(Columns.QUEUED_COUNT, Rows.TOTAL, delta);
            SumTrackedValue(Columns.QUEUED_COUNT, stringToRow[obj], delta);

            if (statsTracker[(Columns.QUEUED_COUNT, Rows.TOTAL)] < 0)
                statsTracker[(Columns.QUEUED_COUNT, Rows.TOTAL)] = 0;

            if (statsTracker[(Columns.QUEUED_COUNT, stringToRow[obj])] < 0)
                statsTracker[(Columns.QUEUED_COUNT, stringToRow[obj])] = 0;
        }

        void ResetTracker()
        {
            //TODO(Brian): Dictionary with enum keys will generate unneeded boxing, fix later. 
            if (statsTracker == null)
            {
                statsTracker = new Dictionary<(Columns, Rows), float>();
            }

            if (processTimeTracker == null)
            {
                processTimeTracker = new Dictionary<Rows, float>();
            }

            if (decodeTimeTracker == null)
            {
                decodeTimeTracker = new Dictionary<Rows, float>();
            }

            for (int x = 1; x < columnsList.Count; x++)
            {
                Columns c = columnsList[x];

                //NOTE(Brian): Ignore queued when resetting to avoid queued count go < 0.
                if (c == Columns.QUEUED_COUNT && DictionaryContainsColumn(statsTracker, c))
                {
                    continue;
                }

                for (int y = 1; y < rowsList.Count; y++)
                {
                    statsTracker[(c, rowsList[y])] = 0;
                    statsPanel.SetCellText(x, y, "");
                }
            }
        }

        private bool DictionaryContainsColumn(Dictionary<(Columns, Rows), float> dictionary, Columns col)
        {
            return dictionary.Any(x => x.Key.Item1 == col);
        }

        private bool DictionaryContainsRow(Dictionary<(Columns, Rows), float> dictionary, Columns col, Rows row)
        {
            //It's faster to check Col again than using DictionaryContainsColumn method
            return dictionary.Any(x => x.Key.Item1 == col && x.Key.Item2 == row);
        }
    }
}