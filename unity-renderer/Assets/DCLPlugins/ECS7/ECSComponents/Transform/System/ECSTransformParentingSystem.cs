public static class ECSTransformParentingSystem
{
    public static void Update()
    {
        if (ECSTransformUtils.orphanEntities == null || ECSTransformUtils.orphanEntities.Count == 0)
        {
            return;
        }

        int count = ECSTransformUtils.orphanEntities.Count;

        for (int i = count - 1; i >= 0; i--)
        {
            var data = ECSTransformUtils.orphanEntities.Pairs[i].value;

            if (ECSTransformUtils.SetParent(data.scene, data.entity, data.parentId))
            {
                ECSTransformUtils.orphanEntities.RemoveAt(i);
            }
        }
    }
}