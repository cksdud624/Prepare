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
        [SerializeField] private AssetLoadSignal assetLoadSignal;
        private InGameModel _inGameModel;
        
        public void Init(InGameModel inGameModel)
        {
            _inGameModel = inGameModel;
            _inGameModel.OnInitialized += OnInitialized;
            
            objectSpawner.Init(inGameModel);
            scenarioPlayer.Init(inGameModel);
            assetLoadSignal.Init(inGameModel);
        }
        
        #region Event

        private void OnInitialized()
        {
            scenarioPlayer.SetScenario(0);
        }
        #endregion

        private void OnDestroy()
        {
            assetLoadSignal.Dispose();
        }
    }
}
