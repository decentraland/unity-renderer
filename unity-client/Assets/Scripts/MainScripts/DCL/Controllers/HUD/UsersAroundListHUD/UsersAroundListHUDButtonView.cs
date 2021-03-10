using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UsersAroundListHUDButtonView : MonoBehaviour, IUsersAroundListHUDButtonView, IPointerDownHandler
{
    [SerializeField] private TextMeshProUGUI usersCountText;

    public void SetUsersCount(int count)
    {
        usersCountText.text = count.ToString();
    }

    public event Action OnClick;

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }
}
