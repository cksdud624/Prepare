using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Generated.Table
{
	public class TableManager : MonoBehaviour
	{
		public CharacterRecord CharacterRecord {get; private set;}
		public CharacterSpawnRecord CharacterSpawnRecord {get; private set;}
		public PlayerSpawnRecord PlayerSpawnRecord {get; private set;}
		public ScenarioBranchParamRecord ScenarioBranchParamRecord {get; private set;}
		public StageRecord StageRecord {get; private set;}
		public StageScenarioRecord StageScenarioRecord {get; private set;}

		public async UniTask Init()
		{
			CharacterRecord = new ();
			await CharacterRecord.Init();
			CharacterSpawnRecord = new ();
			await CharacterSpawnRecord.Init();
			PlayerSpawnRecord = new ();
			await PlayerSpawnRecord.Init();
			ScenarioBranchParamRecord = new ();
			await ScenarioBranchParamRecord.Init();
			StageRecord = new ();
			await StageRecord.Init();
			StageScenarioRecord = new ();
			await StageScenarioRecord.Init();
		}
	}
}
