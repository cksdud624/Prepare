using System.IO;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Generated.Table
{
	public partial class ScenarioBranchParamRecord
	{
		private const string Key = "Assets/Generated/Table/ScenarioBranchParam.bytes";
		private List<ScenarioBranchParamData> datas = new();
		private Dictionary<long, ScenarioBranchParamData> datasById = new();
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
					ScenarioBranchParamData data = new (reader);
					datas.Add(data);
					datasById.Add(data.Id, data);
				}
			}
			InitCustomRecord();
		}
		public ScenarioBranchParamData GetRecord(long id)
		{
			datasById.TryGetValue(id, out var record);
			return record;
		}
		public List<ScenarioBranchParamData> GetAllRecord()
		{
			return datas;
		}
	}

	public class ScenarioBranchParamData
	{
		public long Id {get; private set;}
		public long StageScenario {get; private set;}
		public string ParameterName {get; private set;}
		public long ParameterValue {get; private set;}

		public ScenarioBranchParamData(BinaryReader reader)
		{
			string[] tableDatas = reader.ReadString().Split('	');
			Id = long.TryParse(tableDatas[0], out long vLong0) ? vLong0 : 0L;
			StageScenario = long.TryParse(tableDatas[1], out long vLong1) ? vLong1 : 0L;
			ParameterName = tableDatas[2];
			ParameterValue = long.TryParse(tableDatas[3], out long vLong3) ? vLong3 : 0L;
		}
	}
}
