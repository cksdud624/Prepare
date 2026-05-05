using Common;
using Cysharp.Threading.Tasks;
using Generated.Table;
using InGame.Component;
using InGame.Model;
using UnityEngine;
using ObjectType = Common.GameDefine.ObjectType;

namespace InGame.Object
{
    public class CharacterBase : ObjectBase
    {
        public override ObjectType ObjectType => ObjectType.Character;
        
        public void Init(InGameModel inGameModel, CharacterData characterData)
        {
            InGameModel = inGameModel;
            CommandTranslator = gameObject.AddComponent<CommandTranslator>();
            CommandTranslator.Init(inGameModel);
            InitAsync().Forget();
        }

        protected override async UniTask InitAsync()
        {
            ObjectState = GameDefine.ObjectState.Ready;
            await UniTask.CompletedTask;
        }
    }
}
