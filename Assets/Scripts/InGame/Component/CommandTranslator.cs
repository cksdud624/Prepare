using Cysharp.Threading.Tasks;
using InGame.Model;
using UnityEngine;

namespace InGame.Component
{
    public class CommandTranslator : MonoBehaviour
    {
        private InGameModel _inGameModel;
        private ComponentBank _componentBank;
        
        public async UniTask Init(InGameModel inGameModel, ComponentBank componentBank)
        {
            _inGameModel = inGameModel;
            _componentBank = componentBank;
            await UniTask.CompletedTask;
        }
    }
}
