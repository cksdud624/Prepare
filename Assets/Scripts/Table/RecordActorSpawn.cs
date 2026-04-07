using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Table
{
    public class RecordActorSpawn : RecordBase
    {
        protected override string TableName => "ActorSpawn";
        
        private readonly List<ActorSpawnData> _dataList = new ();
        private readonly Dictionary<long, ActorSpawnData> _dataMap = new ();

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
                ActorSpawnData data = new ActorSpawnData
                (
                    long.Parse(cols[0]), 
                    long.Parse(cols[1]),
                    long.Parse(cols[2]),
                    new Vector3(float.Parse(cols[3]), float.Parse(cols[4]), float.Parse(cols[5]))
                );
                
                _dataList.Add(data);
                _dataMap.Add(data.Id, data);
            }
        }
        
        public IReadOnlyList<ActorSpawnData> GetAllRecords() => _dataList;

        public ActorSpawnData GetRecord(long id)
        {
            if (_dataMap.TryGetValue(id, out ActorSpawnData data))
                return data;
            Debug.LogError($"{GetType()} {id} not found");
            return null;
        }
    }
    
    public class ActorSpawnData
    {
        public long Id;
        public long ActorSpawnGroup;
        public long Actor;
        public Vector3 SpawnPosition;
        
        public ActorSpawnData(long id, long actorSpawnGroup, long actor, Vector3 spawnPosition)
        {
            Id = id;
            ActorSpawnGroup = actorSpawnGroup;
            Actor = actor;
            SpawnPosition = spawnPosition;
        }
    }
}