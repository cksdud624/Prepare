using Common;
using Cysharp.Threading.Tasks;
using Generated.Table;
using InGame.Component;
using InGame.Component.Hub;
using InGame.Component.Model;
using InGame.Model;
using UnityEngine;
using ObjectType = Common.GameDefine.ObjectType;
using ObjectState = Common.GameDefine.ObjectState;

namespace InGame.Object
{
    public class CharacterBase : ObjectBase
    {
        public override ObjectType ObjectType => ObjectType.Character;
        private CharacterModel _characterModel;

        public async UniTask Init(InGameModel inGameModel, CharacterModel characterModel)
        {
            State.Value = ObjectState.Loading;
            InGameModel = inGameModel;
            _characterModel = characterModel;
            InputHub = new InputHub();

            ComponentBank = gameObject.AddComponent<ComponentBank>();
            await ComponentBank.Init(InGameModel, InputHub, characterModel);
            CommandTranslator = gameObject.AddComponent<CommandTranslator>();
            await CommandTranslator.Init(inGameModel, InputHub, ComponentBank);
            State.Value = ObjectState.Ready;
        }

        private void OnDestroy()
        {
            InputHub?.Dispose();
            ComponentBank?.Dispose();
        }
    }
}
