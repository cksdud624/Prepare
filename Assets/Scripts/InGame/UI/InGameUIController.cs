using Cysharp.Threading.Tasks;
using InGame.Model;
using InGame.Object;
using UnityEngine;

namespace InGame.UI
{
    public class InGameUIController : MonoBehaviour
    {
        [SerializeField] private InGameUIView inGameUIView;
        private InGameUIModel _inGameUIModel;
        private InGameModel _inGameModel;

        public async UniTask Init(InGameModel inGameModel)
        {
            _inGameModel = inGameModel;
            _inGameUIModel = new ();
            
            _inGameModel.PlayerInfo.OnPlayerObjectChanged += OnPlayerObjectChanged;
            await UniTask.CompletedTask;
        }
        
        #region Events

        private void OnPlayerObjectChanged(CharacterBase player)
        {
            inGameUIView.SetAimPointActive(true);
        }
        #endregion

        private void OnDestroy()
        {
            _inGameModel.PlayerInfo.OnPlayerObjectChanged -= OnPlayerObjectChanged;
        }
    }
}
