namespace DCL
{
    public class DataStore_SceneMetrics
    {
        public readonly BaseVariable<bool> worstMetricOffenseComputeEnabled = new BaseVariable<bool>(true);
        public readonly BaseDictionary<int, SceneMetricsModel> worstMetricOffenses = new BaseDictionary<int, SceneMetricsModel>();
    }
}