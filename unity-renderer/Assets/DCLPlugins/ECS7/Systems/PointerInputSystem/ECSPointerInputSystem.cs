using System;
using DCL;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace ECSSystems.PointerInputSystem
{
    public static class ECSPointerInputSystem
    {
        private class State
        {
            public IECSComponentWriter componentWriter;
            public IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup;
            public IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup;
            public IInternalECSComponent<InternalColliders> pointerColliderComponent;
            public ECSComponent<PBOnPointerDown> pointerDownComponent;
            public ECSComponent<PBOnPointerUp> pointerUpComponent;
            public DataStore_ECS7 dataStoreEcs7;
            public DataStore_Cursor dataStoreCursor;
            public bool isPointerDown;
            public PointerInputResult lastPointerDownResult;
            public PointerHoverResult lastPointerHoverResult;
        }

        public static Action CreateSystem(
            IECSComponentWriter componentWriter,
            IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup,
            IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup,
            IInternalECSComponent<InternalColliders> pointerColliderComponent,
            ECSComponent<PBOnPointerDown> pointerDownComponent,
            ECSComponent<PBOnPointerUp> pointerUpComponent,
            DataStore_ECS7 dataStoreEcs,
            DataStore_Cursor dataStoreCursor)
        {
            var state = new State()
            {
                componentWriter = componentWriter,
                pointerDownGroup = pointerDownGroup,
                pointerUpGroup = pointerUpGroup,
                pointerColliderComponent = pointerColliderComponent,
                pointerDownComponent = pointerDownComponent,
                pointerUpComponent = pointerUpComponent,
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
                    state.pointerColliderComponent, state.pointerDownComponent, state.pointerUpComponent,
                    state.lastPointerDownResult);

                state.lastPointerDownResult = result;
                if (result.hasValue)
                {
                    if (pointerEvent.isButtonDown)
                    {
                        if (result.shouldTriggerEvent)
                        {
                            state.componentWriter.PutComponent(result.sceneId, result.entityId,
                                ComponentID.ON_POINTER_DOWN_RESULT,
                                ProtoConvertUtils.GetPointerDownResultModel((ActionButton)pointerEvent.buttonId,
                                    null, pointerEvent.rayResult.ray, pointerEvent.rayResult.hitInfo.hit),
                                ECSComponentWriteType.SEND_TO_SCENE);
                        }
                    }
                    else
                    {
                        if (result.shouldTriggerEvent)
                        {
                            state.componentWriter.PutComponent(result.sceneId, result.entityId,
                                ComponentID.ON_POINTER_UP_RESULT,
                                ProtoConvertUtils.GetPointerUpResultModel((ActionButton)pointerEvent.buttonId,
                                    null, pointerEvent.rayResult.ray, pointerEvent.rayResult.hitInfo.hit),
                                ECSComponentWriteType.SEND_TO_SCENE);
                        }
                        state.lastPointerDownResult = PointerInputResult.empty;
                    }
                }
            }

            // process pointer hover
            if (state.dataStoreEcs7.lastPointerRayHit.HasValue)
            {
                PointerHoverResult result = PointerHoverProcessor.ProcessPointerHover(isPointerDown,
                    state.dataStoreEcs7.lastPointerRayHit.Value, state.pointerDownGroup, state.pointerUpGroup);

                if (result.hasFeedback)
                {
                    if (isPointerDown)
                    {
                        PointerInputResult lastPointerDown = state.lastPointerDownResult;
                        if (!result.Equals(state.lastPointerHoverResult)
                            && result.IsSameEntity(lastPointerDown)
                            && result.IsSameButton(state.dataStoreEcs7.lastPointerInputEvent))
                        {
                            state.dataStoreCursor.ShowHoverFeedback(result);
                        }
                    }
                    else
                    {
                        if (!result.Equals(state.lastPointerHoverResult))
                        {
                            state.dataStoreCursor.ShowHoverFeedback(result);
                        }
                    }
                }
                else
                {
                    state.dataStoreCursor.HideHoverFeedback();
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

        private static bool IsSameButton(this PointerHoverResult self, DataStore_ECS7.PointerEvent? other)
        {
            if (!self.hasValue || !other.HasValue)
                return false;

            return (int)self.buttonId == other.Value.buttonId;
        }

        private static void ShowHoverFeedback(this DataStore_Cursor cursor, PointerHoverResult hoverResult)
        {
            if (!hoverResult.hasValue)
                return;

            cursor.hoverFeedbackEnabled.Set(hoverResult.hasFeedback);

            if (hoverResult.hasFeedback)
            {
                cursor.cursorType.Set(DataStore_Cursor.CursorType.HOVER);
                cursor.hoverFeedbackButton.Set(hoverResult.buttonId.GetName());
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