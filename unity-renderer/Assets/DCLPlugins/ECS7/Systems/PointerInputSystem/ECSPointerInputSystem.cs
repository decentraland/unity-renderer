using System;
using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace ECSSystems.PointerInputSystem
{
    public static class ECSPointerInputSystem
    {
        private class State
        {
            public IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup;
            public IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup;
            public DataStore_ECS7 dataStoreEcs7;
            public DataStore_Cursor dataStoreCursor;
            public bool isPointerDown;
            public PointerInputResult lastPointerDownResult;
            public PointerHoverResult lastPointerHoverResult;
        }

        public static Action CreateSystem(IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup,
            IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup,
            DataStore_ECS7 dataStoreEcs, DataStore_Cursor dataStoreCursor)
        {
            var state = new State()
            {
                pointerDownGroup = pointerDownGroup,
                pointerUpGroup = pointerUpGroup,
                dataStoreEcs7 = dataStoreEcs,
                dataStoreCursor = dataStoreCursor,
                isPointerDown = false,
                lastPointerDownResult = PointerInputResult.empty,
                lastPointerHoverResult = PointerHoverResult.empty
            };
            return () => Update(state);
        }

        private static void Update(State state)
        {
            bool isPointerDown = state.isPointerDown;

            // process pointer down/up input
            if (state.dataStoreEcs7.lastPointerInputEvent.HasValue)
            {
                isPointerDown = state.dataStoreEcs7.lastPointerInputEvent.Value.isButtonDown;
                DataStore_ECS7.PointerEvent pointerEvent = state.dataStoreEcs7.lastPointerInputEvent.Value;

                PointerInputResult result = PointerDownUpProcessor.ProcessPointerDownUp(pointerEvent,
                    state.pointerDownGroup, state.pointerUpGroup, state.lastPointerDownResult);

                state.lastPointerDownResult = result;
                if (result.hasValue)
                {
                    if (pointerEvent.isButtonDown)
                    {
                        // TODO: add pointer down result
                    }
                    else
                    {
                        // TODO: add pointer up result
                        state.lastPointerDownResult = PointerInputResult.empty;
                    }
                }
            }

            // process pointer hover
            if (state.dataStoreEcs7.lastPointerRayHit.HasValue)
            {
                PointerHoverResult result = PointerHoverProcessor.ProcessPointerHover(isPointerDown,
                    state.dataStoreEcs7.lastPointerRayHit.Value, state.pointerDownGroup, state.pointerUpGroup);

                if (isPointerDown)
                {
                    PointerInputResult lastPointerDown = state.lastPointerDownResult;
                    if (result.hasValue && !result.Equals(state.lastPointerHoverResult) && result.IsSameEntity(lastPointerDown))
                    {
                        state.dataStoreCursor.ShowHoverFeedback(result);
                    }
                }
                else
                {
                    if (result.hasValue && !result.Equals(state.lastPointerHoverResult))
                    {
                        state.dataStoreCursor.ShowHoverFeedback(result);
                    }
                }
                state.lastPointerHoverResult = result;
            }
            else if (state.lastPointerHoverResult.hasValue)
            {
                state.dataStoreCursor.HideHoverFeedback();
                state.lastPointerHoverResult = PointerHoverResult.empty;
            }

            state.dataStoreEcs7.lastPointerInputEvent = null;
            state.dataStoreEcs7.lastPointerRayHit = null;
            state.isPointerDown = isPointerDown;
        }

        private static bool IsSameEntity(this PointerHoverResult self, PointerInputResult other)
        {
            if (!self.hasValue || !other.hasValue)
                return false;

            return self.sceneId == other.sceneId && self.entityId == other.entityId;
        }

        private static void ShowHoverFeedback(this DataStore_Cursor cursor, PointerHoverResult hoverResult)
        {
            if (!hoverResult.hasValue)
                return;

            cursor.hoverFeedbackEnabled.Set(hoverResult.hasFeedback);

            if (hoverResult.hasFeedback)
            {
                cursor.cursorType.Set(DataStore_Cursor.CursorType.HOVER);
                cursor.hoverFeedbackButton.Set(hoverResult.buttonId.ToString());
                cursor.hoverFeedbackText.Set(hoverResult.text);
                cursor.hoverFeedbackHoverState.Set(true);
            }
        }

        private static void HideHoverFeedback(this DataStore_Cursor cursor)
        {
            cursor.cursorType.Set(DataStore_Cursor.CursorType.NORMAL);
            cursor.hoverFeedbackHoverState.Set(false);
        }
    }
}