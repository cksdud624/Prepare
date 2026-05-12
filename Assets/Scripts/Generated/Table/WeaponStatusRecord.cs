using System.IO;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Generated.Table
{
	public partial class WeaponStatusRecord
	{
		private const string Key = "Assets/Generated/Table/WeaponStatus.bytes";
		private List<WeaponStatusData> datas = new();
		private Dictionary<long, WeaponStatusData> datasById = new();
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
					WeaponStatusData data = new (reader);
					datas.Add(data);
					datasById.Add(data.Id, data);
				}
			}
			InitCustomRecord();
		}
		public WeaponStatusData GetRecord(long id)
		{
			datasById.TryGetValue(id, out var record);
			return record;
		}
		public List<WeaponStatusData> GetAllRecord()
		{
			return datas;
		}
	}

	public class WeaponStatusData
	{
		public long Id {get; private set;}
		public string Name {get; private set;}
		public string Description {get; private set;}
		public float AttackSpeed {get; private set;}

		public WeaponStatusData(BinaryReader reader)
		{
			string[] tableDatas = reader.ReadString().Split('	');
			Id = long.TryParse(tableDatas[0], out long vLong0) ? vLong0 : 0L;
			Name = tableDatas[1];
			Description = tableDatas[2];
			AttackSpeed = float.TryParse(tableDatas[3], out float vFloat3) ? vFloat3 : 0f;
		}
	}
}
