using System.Collections.Generic;

namespace Generated.Table
{
    public partial class StageScenarioRecord
    {
        private Dictionary<long, List<StageScenarioData>> datasByStage = new ();

        partial void InitCustomRecord()
        {
            foreach (var data in datas)
            {
                if (!datasByStage.TryGetValue(data.Stage, out var list))
                {
                    list = new List<StageScenarioData>();
                    datasByStage[data.Stage] = list;
                }
                datasByStage[data.Stage].Add(data);
            }
        }

        public List<StageScenarioData> GetRecordByStage(long stage) => datasByStage.GetValueOrDefault(stage);
    }
}
