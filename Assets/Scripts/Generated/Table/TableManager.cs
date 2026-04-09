using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Generated.Table
{
	public class TableManager : MonoBehaviour
	{
		public ActorRecord ActorRecord {get; private set;}
		public ActorSpawnRecord ActorSpawnRecord {get; private set;}
		public PlayerSpawnRecord PlayerSpawnRecord {get; private set;}
		public StageRecord StageRecord {get; private set;}
		public StageScenarioRecord StageScenarioRecord {get; private set;}

		public async UniTask Init()
		{
			ActorRecord = new ();
			await ActorRecord.Init();
			ActorSpawnRecord = new ();
			await ActorSpawnRecord.Init();
			PlayerSpawnRecord = new ();
			await PlayerSpawnRecord.Init();
			StageRecord = new ();
			await StageRecord.Init();
			StageScenarioRecord = new ();
			await StageScenarioRecord.Init();
		}
	}
}
