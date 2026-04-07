using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Table
{
    public class RecordStage : RecordBase
    {
        protected override string TableName => "Stage";
        
        private readonly List<StageData> _dataList = new ();
        private readonly Dictionary<long, StageData> _dataMap = new ();

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
                StageData data = new
                (
                    long.Parse(cols[0]), 
                    cols[1], 
                    cols[2]
                );
                
                _dataList.Add(data);
                _dataMap.Add(data.Id, data);
            }
        }
        
        public IReadOnlyList<StageData> GetAllRecords() => _dataList;

        public StageData GetRecord(long id)
        {
            if (_dataMap.TryGetValue(id, out StageData data))
                return data;
            Debug.LogError($"{GetType()} {id} not found");
            return null;
        }
    }
    
    public class StageData
    {
        public long Id;
        public string Name;
        public string Description;
        
        public StageData(long id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}