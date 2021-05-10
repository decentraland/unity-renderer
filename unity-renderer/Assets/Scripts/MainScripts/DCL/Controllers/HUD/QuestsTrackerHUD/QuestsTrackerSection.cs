using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerSection : MonoBehaviour
    {
        private static readonly int ANIMATION_TRIGGER_OUT = Animator.StringToHash("Out");

        public enum SequenceState
        {
            NotStarted,
            ProgressingOngoingTasks,
            ShowingNewTasks,
            Finished
        }

        [SerializeField] internal GameObject titleContainer;
        [SerializeField] internal TextMeshProUGUI sectionTitle;
        [SerializeField] internal RectTransform taskContainer;
        [SerializeField] internal GameObject taskPrefab;
        [SerializeField] internal Animator animator;
        [SerializeField] internal AudioEvent newTaskAudioEvent;

        public event Action OnLayoutRebuildRequested;
        public event Action<string> OnDestroyed;

        public SequenceState sequenceState { get; private set; } = SequenceState.NotStarted;

        private QuestSection section;
        internal readonly Dictionary<string, QuestsTrackerTask> taskEntries = new Dictionary<string, QuestsTrackerTask>();
        private readonly List<Coroutine> tasksRoutines = new List<Coroutine>();

        public void Populate(QuestSection newSection)
        {
            sequenceState = SequenceState.NotStarted;
            ClearTaskRoutines();

            section = newSection;
            QuestTask[] allTasks = section.tasks;
            titleContainer.SetActive(!string.IsNullOrEmpty(section.name));
            sectionTitle.text = section.name;

            bool hasCompletedTasksToShow = false;
            List<string> entriesToRemove = taskEntries.Keys.ToList();

            for (int index = 0; index < allTasks.Length; index++)
            {
                QuestTask task = allTasks[index];
                //We only show completed quests that has been just completed to show the progress
                if (!taskEntries.ContainsKey(task.id) && (task.status == QuestsLiterals.Status.BLOCKED || (task.progress >= 1 && !task.justProgressed)))
                    continue;

                entriesToRemove.Remove(task.id);
                if (!taskEntries.TryGetValue(task.id, out QuestsTrackerTask taskEntry))
                {
                    taskEntry = CreateTask();
                    //New tasks are invisible
                    taskEntry.gameObject.SetActive(!task.justUnlocked);
                    taskEntries.Add(task.id, taskEntry);
                }

                taskEntry.Populate(task);
                taskEntry.transform.SetSiblingIndex(index);
            }

            for (int index = 0; index < entriesToRemove.Count; index++)
            {
                DestroyTaskEntry(entriesToRemove[index]);
            }
            OnLayoutRebuildRequested?.Invoke();

        }

        internal QuestsTrackerTask CreateTask()
        {
            var taskEntry = Instantiate(taskPrefab, taskContainer).GetComponent<QuestsTrackerTask>();
            taskEntry.OnDestroyed += (taskId) =>
            {
                taskEntries.Remove(taskId);
                OnLayoutRebuildRequested?.Invoke();
            };
            return taskEntry;
        }

        private void DestroyTaskEntry(string taskId)
        {
            if (!taskEntries.TryGetValue(taskId, out QuestsTrackerTask taskEntry))
                return;
            Destroy(taskEntry.gameObject);
            taskEntries.Remove(taskId);
        }

        public IEnumerator Sequence()
        {
            sequenceState = SequenceState.ProgressingOngoingTasks;

            ClearTaskRoutines();

            List<QuestsTrackerTask> visibleTasks = new List<QuestsTrackerTask>();
            List<QuestsTrackerTask> newTasks = new List<QuestsTrackerTask>();
            foreach (QuestsTrackerTask task in taskEntries.Values)
            {
                if (task.gameObject.activeSelf)
                    visibleTasks.Add(task);
                else
                    newTasks.Add(task);
            }

            //Progress of currently visible tasks
            for (int i = 0; i < visibleTasks.Count; i++)
            {
                tasksRoutines.Add(StartCoroutine(visibleTasks[i].ProgressAndCompleteSequence()));
            }
            yield return WaitForTaskRoutines();
            OnLayoutRebuildRequested?.Invoke();

            sequenceState = SequenceState.ShowingNewTasks;

            //Play "new task" sound
            if (newTasks.Count > 0)
                newTaskAudioEvent.Play();

            //Show and progress of new tasks
            for (int i = 0; i < newTasks.Count; i++)
            {
                newTasks[i].gameObject.SetActive(true);
                newTasks[i].SetIsNew(true);
                tasksRoutines.Add(StartCoroutine(newTasks[i].ProgressAndCompleteSequence()));
            }
            OnLayoutRebuildRequested?.Invoke();

            yield return WaitForTaskRoutines();

            if (taskEntries.Count == 0)
            {
                animator.SetTrigger(ANIMATION_TRIGGER_OUT);
                yield return WaitForSecondsCache.Get(0.5f);
                Destroy(gameObject);
                OnDestroyed?.Invoke(section.id);
            }
            sequenceState = SequenceState.Finished;
        }

        public void SetExpandCollapseState(bool newIsExpanded)
        {
            foreach (QuestsTrackerTask taskEntry in taskEntries.Values)
            {
                taskEntry.SetExpandedStatus(newIsExpanded);
            }

            OnLayoutRebuildRequested?.Invoke();
        }

        public void ClearTaskRoutines()
        {
            if (tasksRoutines.Count > 0)
            {
                for (int i = 0; i < tasksRoutines.Count; i++)
                {
                    if (tasksRoutines[i] != null)
                        StopCoroutine(tasksRoutines[i]);
                }
                tasksRoutines.Clear();
            }
        }

        private IEnumerator WaitForTaskRoutines()
        {
            for (int i = 0; i < tasksRoutines.Count; i++)
            {
                //yielding Coroutines (not IEnumerators) allows us to wait for them in parallel
                yield return tasksRoutines[i];
            }
        }
    }
}