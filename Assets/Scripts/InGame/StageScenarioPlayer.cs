using System.Collections.Generic;
using System.Linq;
using Common;
using InGame.Model;
using Generated.Table;
using UnityEngine;

namespace InGame
{
    public class StageScenarioPlayer : MonoBehaviour
    {
        private List<StageScenarioData> _stageScenarioDatas;
        private int _currentScenarioIndex;
        private InGameModel _inGameModel;
        
        public void Init(InGameModel inGameModel)
        {
            _inGameModel = inGameModel;
            _stageScenarioDatas = Global.Instance.TableManager.StageScenarioRecord.GetRecordByStage(_inGameModel.StageData.Id)
                .OrderBy(data => data.ScenarioType)
                .ThenBy(data => data.Id).ToList();
            
            if (!(_stageScenarioDatas?.Count > 0))
                Debug.LogError($"{GetType()} stage scenario data not found");
        }
        
        public void SetScenario(int index)
        {
            if (_stageScenarioDatas == null || index >= _stageScenarioDatas.Count)
            {
                Debug.LogError($"Scenario data index out of range: {index}");
                return;
            }
            _currentScenarioIndex = index;
            var playerSpawn = _stageScenarioDatas[index].PlayerSpawn;
            if (playerSpawn != 0)
            {
                var playerSpawnData = Global.Instance.TableManager.PlayerSpawnRecord.GetRecord(playerSpawn);
                if (playerSpawnData == null)
                {
                    Debug.LogError($"PlayerSpawn record not found Id : {playerSpawn}");
                    return;
                }
                
                _inGameModel.OnSpawnPlayer?.Invoke(playerSpawnData);
            }
            //2. 캐릭터 스폰 
            //시나리오 세팅
            //objectspawner에게 스폰 이벤트를 보내야하는데 전부 초기화가 끝나야 제대로 시나리오 세팅을 하는 것이
            //중요하기 때문에 고민좀해봐야함
        }
    }
}
