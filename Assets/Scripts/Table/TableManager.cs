using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Table
{
    public class TableManager : MonoBehaviour
    {
        public RecordActor RecordActor { get; private set; }
        public RecordStage RecordStage { get; private set; }
        public RecordStageScenario RecordStageScenario { get; private set; }
        public RecordActorSpawn  RecordActorSpawn { get; private set; }
        public RecordPlayerSpawn  RecordPlayerSpawn { get; private set; }
        public bool IsLoaded { get; private set; }
        
        public async UniTask Init()
        {
            await LoadAllRecords();
        }

        private async UniTask LoadAllRecords()
        {
            RecordActor = new RecordActor();
            await RecordActor.LoadRecord();
            RecordStage = new RecordStage();
            await RecordStage.LoadRecord();
            RecordStageScenario = new RecordStageScenario();
            await RecordStageScenario.LoadRecord();
            RecordActorSpawn = new RecordActorSpawn();
            await RecordActorSpawn.LoadRecord();
            RecordPlayerSpawn = new RecordPlayerSpawn();
            await RecordPlayerSpawn.LoadRecord();

            IsLoaded = true;
            Debug.Log("All records loaded");
        }
    }
}
