using System.IO;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Generated.Table
{
	public partial class CharacterRecord
	{
		private const string Key = "Assets/Generated/Table/Character.bytes";
		private List<CharacterData> datas = new();
		private Dictionary<long, CharacterData> datasById = new();
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
					CharacterData data = new (reader);
					datas.Add(data);
					datasById.Add(data.Id, data);
				}
			}
			InitCustomRecord();
		}
		public CharacterData GetRecord(long id)
		{
			datasById.TryGetValue(id, out var record);
			return record;
		}
		public List<CharacterData> GetAllRecord()
		{
			return datas;
		}
	}

	public class CharacterData
	{
		public long Id {get; private set;}
		public string Name {get; private set;}
		public List<long> AnimatorId {get; private set;}

		public CharacterData(BinaryReader reader)
		{
			string[] tableDatas = reader.ReadString().Split('	');
			Id = long.TryParse(tableDatas[0], out long vLong0) ? vLong0 : 0L;
			Name = tableDatas[1];
			AnimatorId = new ();
			string[] items2 = tableDatas[2].Split(',');
			foreach (var item in items2)
			{
				AnimatorId.Add(long.TryParse(item, out long vLong2) ? vLong2 : 0L);
			}
		}
	}
}
