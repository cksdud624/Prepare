using Common;
using Cysharp.Threading.Tasks;
using InGame.Component;
using InGame.Model;
using UnityEngine;
using ObjectType = Common.GameDefine.ObjectType;
using ObjectState = Common.GameDefine.ObjectState;

namespace InGame.Object
{
    public class ObjectBase : MonoBehaviour
    {
        public virtual ObjectType ObjectType => ObjectType.Object;
        public ObjectState ObjectState { get; protected set; } = ObjectState.Loading;
        protected InGameModel InGameModel;
        
        protected CommandTranslator CommandTranslator; 

        public void Init(InGameModel inGameModel)
        {
            InGameModel = inGameModel;
            InitAsync().Forget();
        }

        protected virtual async UniTask InitAsync()
        {
            await UniTask.CompletedTask;
        }
    }
}
