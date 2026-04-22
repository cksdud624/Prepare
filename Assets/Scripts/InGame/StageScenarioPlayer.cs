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
            
            //플레이어 스폰(0은 없는 것)
            var playerSpawn = _stageScenarioDatas[index].PlayerSpawn;
            if (playerSpawn != 0)
            {
                var playerSpawnData = Global.Instance.TableManager.PlayerSpawnRecord.GetRecord(playerSpawn);
                if (playerSpawnData == null)
                {
                    Debug.LogError($"PlayerSpawn record not found Id : {playerSpawn}");
                    return;
                }
                
                _inGameModel.NotifyOnSpawnPlayer(playerSpawnData);
            }
        }
    }
}
