using Common;
using Cysharp.Threading.Tasks;
using Generated.Table;
using InGame.Component;
using InGame.Model;
using ObjectType = Common.GameDefine.ObjectType;
using ObjectState = Common.GameDefine.ObjectState;

namespace InGame.Object
{
    public class CharacterBase : ObjectBase
    {
        public override ObjectType ObjectType => ObjectType.Character;
        private CharacterData _characterData;
        
        public async UniTask Init(InGameModel inGameModel, CharacterData characterData)
        {
            State.Value = ObjectState.Loading;
            InGameModel = inGameModel;
            _characterData = characterData;
            
            ComponentBank = gameObject.AddComponent<ComponentBank>();
            await ComponentBank.Init(InGameModel, _characterData);
            CommandTranslator = gameObject.AddComponent<CommandTranslator>();
            await CommandTranslator.Init(inGameModel, ComponentBank);
            CameraController = gameObject.AddComponent<CameraController>();
            await CameraController.Init(inGameModel);
            State.Value = ObjectState.Ready;
        }

        private void OnDestroy()
        {
            ComponentBank?.Dispose();
        }
    }
}
