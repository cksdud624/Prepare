using Common;
using Cysharp.Threading.Tasks;
using InGame.Model;
using InGame.Object;
using Unity.Cinemachine;
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
        [SerializeField] private CinemachineCamera cinemachineCamera;
        
        private InGameModel _inGameModel;
        
        public async UniTask Init(InGameModel inGameModel)
        {
            _inGameModel = inGameModel;
            await objectSpawner.Init(_inGameModel);
            await objectSpawner.SpawnPlayer(_inGameModel.PlayerInfo);
            var player = _inGameModel.PlayerInfo.PlayerObject;
            player.AttachCamera(cinemachineCamera);
            player.AttachController(true);
            player.SetPlaying();
        }

        public void OnDestroy()
        {
            _inGameModel?.Dispose();
        }
    }
}
