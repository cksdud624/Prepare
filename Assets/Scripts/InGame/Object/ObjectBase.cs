using Cysharp.Threading.Tasks;
using InGame.Component;
using InGame.Model;
using UniRx;
using Unity.Cinemachine;
using UnityEngine;
using ObjectType = Common.GameDefine.ObjectType;
using ObjectState = Common.GameDefine.ObjectState;

namespace InGame.Object
{
    public class ObjectBase : MonoBehaviour
    {
        public virtual ObjectType ObjectType => ObjectType.Object;
        protected readonly ReactiveProperty<ObjectState> State = new (ObjectState.Raw);
        public ObjectState ObjectState => State.Value;
        
        protected InGameModel InGameModel;
        
        #region Components
        protected CommandTranslator CommandTranslator; 
        protected ComponentBank ComponentBank;
        protected CameraController CameraController;
        #endregion

        public async UniTask Init(InGameModel inGameModel)
        {
            InGameModel = inGameModel;
            await UniTask.CompletedTask;
        }

        public void AttachCamera(CinemachineCamera targetCamera) => CameraController.AttachCamera(targetCamera);

        public void DetachCamera() => CameraController.DetachCamera();
    }
}
