using System.IO;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Generated.Table
{
	public partial class PlayerSpawnRecord
	{
		private const string key = "Assets/Generated/Table/PlayerSpawn.bytes";
		private List<PlayerSpawnData> datas = new();
		private Dictionary<long, PlayerSpawnData> datasById = new();
		public async UniTask Init()
		{
			var asset = await Addressables.LoadAssetAsync<TextAsset>(key).ToUniTask();
			if(asset == null)
				throw new System.OperationCanceledException($"Load failed: {key}");
			using (MemoryStream ms = new MemoryStream(asset.bytes))
			using (BinaryReader reader = new BinaryReader(ms))
			{
				while (reader.BaseStream.Position < reader.BaseStream.Length)
				{
					PlayerSpawnData data = new (reader);
					datas.Add(data);
					datasById.Add(data.Id, data);
				}
			}
		}
		public PlayerSpawnData GetRecord(long id)
		{
			datasById.TryGetValue(id, out var record);
			return record;
		}
		public List<PlayerSpawnData> GetAllRecord()
		{
			return datas;
		}
	}

	public class PlayerSpawnData
	{
		public long Id {get; private set;}
		public long Stage {get; private set;}
		public Vector3 SpawnPos {get; private set;}

		public PlayerSpawnData(BinaryReader reader)
		{
			string[] tableDatas = reader.ReadString().Split('	');
			Id = long.TryParse(tableDatas[0], out long vLong0) ? vLong0 : 0L;
			Stage = long.TryParse(tableDatas[1], out long vLong1) ? vLong1 : 0L;
			string[] items2 = tableDatas[2].Split(',');
			if (items2.Length == 3)
			{
				float.TryParse(items2[0], out float resultX2);
				float.TryParse(items2[1], out float resultY2);
				float.TryParse(items2[2], out float resultZ2);
				SpawnPos = new Vector3(resultX2, resultY2, resultZ2);
			}
			else
			{
				SpawnPos = Vector3.zero;
				Debug.LogWarning(SpawnPos + "is not Vector3");
			}
		}
	}
}
