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
		public List<string> CustomAnimation {get; private set;}
		public Vector3 MoveColliderCenter {get; private set;}
		public float MoveColliderRadius {get; private set;}
		public float MoveColliderHeight {get; private set;}

		public CharacterData(BinaryReader reader)
		{
			string[] tableDatas = reader.ReadString().Split('	');
			Id = long.TryParse(tableDatas[0], out long vLong0) ? vLong0 : 0L;
			Name = tableDatas[1];
			CustomAnimation = new ();
			string[] items2 = tableDatas[2].Split(',');
			foreach (var item in items2)
			{
				CustomAnimation.Add(item);
			}
			string[] items3 = tableDatas[3].Split(';');
			if (items3.Length == 3)
			{
				float.TryParse(items3[0], out float resultX3);
				float.TryParse(items3[1], out float resultY3);
				float.TryParse(items3[2], out float resultZ3);
				MoveColliderCenter = new Vector3(resultX3, resultY3, resultZ3);
			}
			else
			{
				MoveColliderCenter = Vector3.zero;
				Debug.LogError(MoveColliderCenter + "is not Vector3");
			}
			MoveColliderRadius = float.TryParse(tableDatas[4], out float vFloat4) ? vFloat4 : 0f;
			MoveColliderHeight = float.TryParse(tableDatas[5], out float vFloat5) ? vFloat5 : 0f;
		}
	}
}
