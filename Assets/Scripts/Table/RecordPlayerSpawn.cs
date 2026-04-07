using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Table
{
    public class RecordPlayerSpawn : RecordBase
    {
        protected override string TableName => "PlayerSpawn";
        
        private readonly List<PlayerSpawnData> _dataList = new ();
        private readonly Dictionary<long, PlayerSpawnData> _dataMap = new ();

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
                PlayerSpawnData data = new PlayerSpawnData
                (
                    long.Parse(cols[0]), 
                    long.Parse(cols[1]), 
                    new Vector3(float.Parse(cols[2]), float.Parse(cols[3]), float.Parse(cols[4]))
                );
                
                _dataList.Add(data);
                _dataMap.Add(data.Id, data);
            }
        }
        
        public IReadOnlyList<PlayerSpawnData> GetAllRecords() => _dataList;

        public PlayerSpawnData GetRecord(long id)
        {
            if (_dataMap.TryGetValue(id, out PlayerSpawnData data))
                return data;
            Debug.LogError($"{GetType()} {id} not found");
            return null;
        }
    }
    
    public class PlayerSpawnData
    {
        public long Id;
        public long Stage;
        public Vector3 SpawnPosition;
        
        public PlayerSpawnData(long id, long stage, Vector3 spawnPosition)
        {
            Id = id;
            Stage = stage;
            SpawnPosition = spawnPosition;
        }
    }
}