using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Table
{
    public class RecordActor : RecordBase
    {
        protected override string TableName => "Actor";
        
        private readonly List<ActorData> _dataList = new ();
        private readonly Dictionary<long, ActorData> _dataMap = new ();

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
                ActorData data = new ActorData
                    (
                        long.Parse(cols[0]), 
                        cols[1], 
                        cols[2].Split(',').Select(long.Parse).ToList()
                    );
                
                _dataList.Add(data);
                _dataMap.Add(data.Id, data);
            }
        }
        
        public IReadOnlyList<ActorData> GetAllRecords() => _dataList;

        public ActorData GetRecord(long id)
        {
            if (_dataMap.TryGetValue(id, out ActorData data))
                return data;
            Debug.LogError($"{GetType()} {id} not found");
            return null;
        }
    }
    
    public class ActorData
    {
        public long Id;
        public string Name;
        public List<long> AnimatorIds;
        
        public ActorData(long id, string name,  List<long> animatorIds)
        {
            Id = id;
            Name = name;
            AnimatorIds = animatorIds;
        }
    }
}
