using System.IO;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Generated.Table
{
	public partial class StageScenarioRecord
	{
		private const string Key = "Assets/Generated/Table/StageScenario.bytes";
		private List<StageScenarioData> datas = new();
		private Dictionary<long, StageScenarioData> datasById = new();
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
					StageScenarioData data = new (reader);
					datas.Add(data);
					datasById.Add(data.Id, data);
				}
			}
		}
		public StageScenarioData GetRecord(long id)
		{
			datasById.TryGetValue(id, out var record);
			return record;
		}
		public List<StageScenarioData> GetAllRecord()
		{
			return datas;
		}
	}

	public class StageScenarioData
	{
		public long Id {get; private set;}
		public long Stage {get; private set;}
		public List<long> ActorSpawnGroup {get; private set;}

		public StageScenarioData(BinaryReader reader)
		{
			string[] tableDatas = reader.ReadString().Split('	');
			Id = long.TryParse(tableDatas[0], out long vLong0) ? vLong0 : 0L;
			Stage = long.TryParse(tableDatas[1], out long vLong1) ? vLong1 : 0L;
			ActorSpawnGroup = new ();
			string[] items2 = tableDatas[2].Split(',');
			foreach (var item in items2)
			{
				ActorSpawnGroup.Add(long.TryParse(item, out long vLong2) ? vLong2 : 0L);
			}
		}
	}
}
