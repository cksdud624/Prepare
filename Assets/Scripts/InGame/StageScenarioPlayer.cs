using System.Collections.Generic;
using Common;
using InGame.Model;
//using Generated.Table;
using UnityEngine;

namespace InGame
{
    public class StageScenarioPlayer : MonoBehaviour
    {
        //private List<StageScenarioData> _stageScenarioDatas;
        //private StageScenarioData _currentScenario;
        
        public void Init(InGameModel inGameModel)
        {
            /*
            _stageScenarioDatas = Global.Instance.TableManager.StageScenarioRecord.GetRecordsByStage(inGameModel.StageData.Id);
            if (!(_stageScenarioDatas?.Count > 0))
            {
                Debug.LogError($"{GetType()} stage scenario data not found");
                return;
            }
            SetScenario(_stageScenarioDatas[0]);
            */
        }
        
        //private void SetScenario(StageScenarioData scenarioData)
        //{
            //_currentScenario = scenarioData;
            //시나리오 세팅
            //objectspawner에게 스폰 이벤트를 보내야하는데 전부 초기화가 끝나야 제대로 시나리오 세팅을 하는 것이
            //중요하기 때문에 고민좀해봐야함
           
        //}
    }
}
