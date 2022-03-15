using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Builder
{
    public class ButtonColorChange : MonoBehaviour,
                                     IPointerDownHandler, IPointerUpHandler,
                                     IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] internal Image buttonImage;
        [SerializeField] internal Color onIdleColor;
        [SerializeField] internal Color onHoverColor;
        [SerializeField] internal Color onPressedColor;
        [SerializeField] internal Color onDisabledColor;

        private bool isHovered;
        private bool isPressed;
        
        public enum State
        {
            IDLE = 0,
            HOVER = 1,
            PRESSED = 2,
            DISABLED = 3
        }

        private State state = State.IDLE;
        
        private void SetColor(State currentState)
        {
            Color colorToUse = onIdleColor;
            switch (currentState)
            {
                case State.HOVER:
                    colorToUse = onHoverColor;
                    break;
                case State.PRESSED:
                    colorToUse = onPressedColor;
                    break;
                case State.DISABLED:
                    colorToUse = onDisabledColor;
                    break;
              
            }
            buttonImage.color = colorToUse;
        }

        private void SetState(State state)
        {
            this.state = state;
            SetColor(state);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            SetState(State.PRESSED);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            if(!isHovered)
                SetState(State.IDLE);
            else
                SetState(State.HOVER);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            SetState(State.HOVER);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            if(!isPressed)
                SetState(State.IDLE);
            else
                SetState(State.PRESSED);
        }
    }
}