using System;
using System.Collections.Generic;
using DCL;
using DCL.Components.Interfaces;
using UnityEngine;

namespace DCLPlugins.UIRefresherPlugin
{
    public class UIRefresherController : IDisposable
    {
        private const float DIRTY_WATCHER_UPDATE_BUDGET = 2/1000f;

        private readonly IUpdateEventHandler eventHandler;
        private readonly StringVariable sceneID;
        private readonly BaseVariable<Dictionary<string, Queue<IUIRefreshable>>> dirtyShapes;

        public UIRefresherController( IUpdateEventHandler updateEventHandler, 
            StringVariable sceneIDVariable, 
            BaseVariable<Dictionary<string, Queue<IUIRefreshable>>> dirtyShapesVariable)
        {
            eventHandler = updateEventHandler;
            sceneID = sceneIDVariable;
            dirtyShapes = dirtyShapesVariable;
            eventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, OnLateUpdate);
        }
        
        public void Dispose()
        {
            eventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, OnLateUpdate);
        }
        
        private void OnLateUpdate()
        {
            var currentScene = sceneID.Get();

            if (string.IsNullOrEmpty(currentScene))
                currentScene = "default";

            var dirtyShapesByScene = dirtyShapes.Get();
            var startTime = Time.realtimeSinceStartup;

            // prioritize current scene
            if (dirtyShapesByScene.ContainsKey(currentScene))
            {
                var queue = dirtyShapesByScene[sceneID];
            
                while (queue.Count > 0)
                {
                    var uiShape = queue.Dequeue();
                    uiShape.Refresh();
                    
                    if (!CanRefreshMore(startTime)) return;
                }
            }
            
            // update other scenes, this prevents hiccups when entering scenes with huge UIs that already loaded them
            foreach (KeyValuePair<string,Queue<IUIRefreshable>> valuePair in dirtyShapesByScene)
            {
                if (valuePair.Key == currentScene) continue;

                var queue = valuePair.Value;
            
                while (queue.Count > 0)
                {
                    var uiShape = queue.Dequeue();
                    uiShape.Refresh();
                    
                    if (!CanRefreshMore(startTime)) return;
                }
            }
        }
        private static bool CanRefreshMore(float startTime)
        {
            if (Application.isBatchMode) return true;
            return Time.realtimeSinceStartup - startTime < DIRTY_WATCHER_UPDATE_BUDGET;
        }
    }
}