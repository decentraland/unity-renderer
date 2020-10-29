using System;

public interface IUsersAroundListHUDButtonView
{
    void SetUsersCount(int count);
    event Action OnClick;
}
