using Common.Scene.Parameter;
using InGame.Model;
using InGame.Object;
using Generated.Table;
using UnityEngine;

namespace InGame
{
    public class GameController : MonoBehaviour
    {
        /*
         * 인게임 컨트롤러
         * 게임에서 처음부터 끝을 전부 관리하는 형식으로 한다
         */
        [SerializeField] private StageScenarioPlayer scenarioPlayer;
        [SerializeField] private ObjectSpawner objectSpawner;
        private StageData _stageData;
        public void Init(InGameModel inGameModel)
        {
            objectSpawner.Init(inGameModel);
            scenarioPlayer.Init(inGameModel);
        }
    }
}
