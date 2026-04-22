using System.IO;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Generated.Table
{
	public partial class PlayerSpawnRecord
	{
		private const string Key = "Assets/Generated/Table/PlayerSpawn.bytes";
		private List<PlayerSpawnData> datas = new();
		private Dictionary<long, PlayerSpawnData> datasById = new();
		partial void InitCustomRecord();
		public async UniTask Init()
		{
			var asset = await Addressables.LoadAssetAsync<TextAsset>(Key).ToUniTask();
			if(asset == null)
				throw new System.OperationCanceledException($"Load failed: {Key}");
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
			InitCustomRecord();
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
		public Vector3 SpawnPosition {get; private set;}
		public float SpawnRotation {get; private set;}

		public PlayerSpawnData(BinaryReader reader)
		{
			string[] tableDatas = reader.ReadString().Split('	');
			Id = long.TryParse(tableDatas[0], out long vLong0) ? vLong0 : 0L;
			string[] items1 = tableDatas[1].Split(';');
			if (items1.Length == 3)
			{
				float.TryParse(items1[0], out float resultX1);
				float.TryParse(items1[1], out float resultY1);
				float.TryParse(items1[2], out float resultZ1);
				SpawnPosition = new Vector3(resultX1, resultY1, resultZ1);
			}
			else
			{
				SpawnPosition = Vector3.zero;
				Debug.LogError(SpawnPosition + "is not Vector3");
			}
			SpawnRotation = float.TryParse(tableDatas[2], out float vFloat2) ? vFloat2 : 0f;
		}
	}
}
