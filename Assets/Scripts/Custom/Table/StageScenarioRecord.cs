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
                datasByStage[data.Stage] ??= new();
                datasByStage[data.Stage].Add(data);
            }
        }
    }
}
