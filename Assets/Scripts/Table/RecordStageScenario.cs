using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Table
{
    public class RecordStageScenario : RecordBase
    {
        protected override string TableName => "StageScenario";
        
        private readonly List<StageScenarioData> _dataList = new ();
        private readonly Dictionary<long, StageScenarioData> _dataMap = new ();
        private readonly Dictionary<long, List<StageScenarioData>> _dataMapByStage = new ();

        protected override void SetRecord(TextAsset result)
        {
            using MemoryStream ms = new MemoryStream(result.bytes);
            using BinaryReader reader = new BinaryReader(ms);
            
            reader.ReadInt64();
            int rowCount = reader.ReadInt32();
            
            for (int i = 0; i < rowCount; i++)
            {
                string oneLine = reader.ReadString().Replace(" ", "");
                string[] cols = oneLine.Split('\t');
                StageScenarioData data = new StageScenarioData
                (
                    long.Parse(cols[0]), 
                    long.Parse(cols[1]),
                    cols[2].Split(',').Select(long.Parse).ToList()
                );
                
                _dataList.Add(data);
                _dataMap.Add(data.Id, data);
                if (_dataMapByStage.TryGetValue(data.Stage, out var list))
                    list.Add(data);
                else
                    _dataMapByStage.Add(data.Stage, new List<StageScenarioData> {data});
            }

            foreach (var list in _dataMapByStage.Values)
                list.Sort((a, b) => a.Id.CompareTo(b.Id));
        }
        
        public IReadOnlyList<StageScenarioData> GetAllRecords() => _dataList;

        public StageScenarioData GetRecord(long id)
        {
            if (_dataMap.TryGetValue(id, out StageScenarioData data))
                return data;
            Debug.LogError($"{GetType()} {id} not found");
            return null;
        }

        public List<StageScenarioData> GetRecordsByStage(long stage)
        {
            if(_dataMapByStage.TryGetValue(stage, out List<StageScenarioData> data))
                return data;
            Debug.LogError($"{GetType()} {stage} not found");
            return null;
        }
    }
    
    public class StageScenarioData
    {
        public long Id;
        public long Stage;
        public List<long> ActorSpawnGroup;
        
        public StageScenarioData(long id, long stage, List<long> actorSpawnGroup)
        {
            Id = id;
            Stage = stage;
            ActorSpawnGroup = actorSpawnGroup;
        }
    }
}