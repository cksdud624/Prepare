using System.IO;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Generated.Table
{
	public partial class CharacterSpawnRecord
	{
		private const string Key = "Assets/Generated/Table/CharacterSpawn.bytes";
		private List<CharacterSpawnData> datas = new();
		private Dictionary<long, CharacterSpawnData> datasById = new();
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
					CharacterSpawnData data = new (reader);
					datas.Add(data);
					datasById.Add(data.Id, data);
				}
			}
			InitCustomRecord();
		}
		public CharacterSpawnData GetRecord(long id)
		{
			datasById.TryGetValue(id, out var record);
			return record;
		}
		public List<CharacterSpawnData> GetAllRecord()
		{
			return datas;
		}
	}

	public class CharacterSpawnData
	{
		public long Id {get; private set;}
		public long CharacterSpawnGroup {get; private set;}
		public long Character {get; private set;}
		public Vector3 SpawnPos {get; private set;}

		public CharacterSpawnData(BinaryReader reader)
		{
			string[] tableDatas = reader.ReadString().Split('	');
			Id = long.TryParse(tableDatas[0], out long vLong0) ? vLong0 : 0L;
			CharacterSpawnGroup = long.TryParse(tableDatas[1], out long vLong1) ? vLong1 : 0L;
			Character = long.TryParse(tableDatas[2], out long vLong2) ? vLong2 : 0L;
			string[] items3 = tableDatas[3].Split(';');
			if (items3.Length == 3)
			{
				float.TryParse(items3[0], out float resultX3);
				float.TryParse(items3[1], out float resultY3);
				float.TryParse(items3[2], out float resultZ3);
				SpawnPos = new Vector3(resultX3, resultY3, resultZ3);
			}
			else
			{
				SpawnPos = Vector3.zero;
				Debug.LogError(SpawnPos + "is not Vector3");
			}
		}
	}
}
