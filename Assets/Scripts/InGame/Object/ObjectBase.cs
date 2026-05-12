using Cysharp.Threading.Tasks;
using InGame.Component;
using InGame.Component.Controller;
using InGame.Component.Hub;
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
        protected ControllerBase Controller;
        protected InputHub InputHub;
        #endregion

        public async UniTask Init(InGameModel inGameModel)
        {
            InGameModel = inGameModel;
            await UniTask.CompletedTask;
        }
        
        public void SetPlaying()
        {
            if (State.Value is ObjectState.Ready or ObjectState.Sleep) State.Value = ObjectState.Playing;
            else Debug.LogWarning("Object State is not ready");
        }

        public void AttachController(bool isPlayer)
        {
            if (Controller != null)
                Destroy(Controller);

            if (isPlayer)
                Controller = gameObject.AddComponent<ControllerPlayer>();

            Controller.Init(InputHub);
        }

        public void DetachController() => Destroy(Controller);

        public void AttachCamera(CinemachineCamera targetCamera) => ComponentBank.CameraController.AttachCamera(targetCamera);

        public void DetachCamera() => ComponentBank.CameraController.DetachCamera();
    }
}
