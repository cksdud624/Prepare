using Cysharp.Threading.Tasks;
using InGame.Model;
using InGame.Object;
using UnityEngine;

namespace InGame
{
    public class GameController : MonoBehaviour
    {
        /*
         * 인게임 컨트롤러
         * 게임에서 처음부터 끝을 전부 관리하는 형식으로 한다
         */
        [SerializeField] private ObjectSpawner objectSpawner; 
        
        private InGameModel _inGameModel;
        
        public void Init(InGameModel inGameModel)
        {
            _inGameModel = inGameModel;
            _inGameModel.OnInitialized += OnInitialized;
            objectSpawner.Init(_inGameModel);
            InitAsync().Forget();
        }

        private async UniTask InitAsync()
        {
            _inGameModel.NotifyOnInitialized();
            await UniTask.CompletedTask;
        }
        
        #region Events

        private void OnInitialized()
        {
            Debug.Log("초기화");
        }
        #endregion

        public void OnDestroy()
        {
            _inGameModel.Dispose();
        }
    }
}
