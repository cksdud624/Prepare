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
		public int WeaponType {get; private set;}
		public string Name {get; private set;}
		public string Description {get; private set;}
		public float AttackSpeed {get; private set;}
		public List<string> CustomWeaponAnimation {get; private set;}

		public WeaponStatusData(BinaryReader reader)
		{
			string[] tableDatas = reader.ReadString().Split('	');
			Id = long.TryParse(tableDatas[0], out long vLong0) ? vLong0 : 0L;
			WeaponType = int.TryParse(tableDatas[1], out int vInt1) ? vInt1 : 0;
			Name = tableDatas[2];
			Description = tableDatas[3];
			AttackSpeed = float.TryParse(tableDatas[4], out float vFloat4) ? vFloat4 : 0f;
			CustomWeaponAnimation = new ();
			string[] items5 = tableDatas[5].Split(',');
			foreach (var item in items5)
			{
				CustomWeaponAnimation.Add(item);
			}
		}
	}
}
