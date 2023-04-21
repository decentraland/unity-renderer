using System.Collections.Generic;
using UnityEngine;

public interface IDynamicListComponentView
{
    void AddIcon(Sprite sprite);
    void AddIcons(List<Sprite> spriteList);
    void RemoveIcons();
}
