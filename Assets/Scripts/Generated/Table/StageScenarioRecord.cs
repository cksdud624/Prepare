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
					StageScenarioData data = new (reader);
					datas.Add(data);
					datasById.Add(data.Id, data);
				}
			}
			InitCustomRecord();
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
		public int ScenarioType {get; private set;}
		public int ScenarioBranch {get; private set;}
		public long PlayerSpawn {get; private set;}

		public StageScenarioData(BinaryReader reader)
		{
			string[] tableDatas = reader.ReadString().Split('	');
			Id = long.TryParse(tableDatas[0], out long vLong0) ? vLong0 : 0L;
			Stage = long.TryParse(tableDatas[1], out long vLong1) ? vLong1 : 0L;
			ScenarioType = int.TryParse(tableDatas[2], out int vInt2) ? vInt2 : 0;
			ScenarioBranch = int.TryParse(tableDatas[3], out int vInt3) ? vInt3 : 0;
			PlayerSpawn = long.TryParse(tableDatas[4], out long vLong4) ? vLong4 : 0L;
		}
	}
}
