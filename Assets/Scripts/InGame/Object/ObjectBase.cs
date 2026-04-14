using Cysharp.Threading.Tasks;
using InGame.Model;
using UnityEngine;
using ObjectState = Common.GameDefine.ObjectState;

namespace InGame.Object
{
    public class ObjectBase : MonoBehaviour
    {
        #region Object Management
        protected InGameModel inGameModel;

        public void Init(InGameModel model, bool isPlayer = false)
        {
            this.isPlayer = isPlayer;
            inGameModel = model;
            AddObject();
            LoadAsset();
            State = ObjectState.Ready;
        }

        protected virtual void AddObject()
        {
            inGameModel.InGameObjectModel.AddObject(this);
        }

        protected virtual void OnDestroy()
        {
            inGameModel.InGameObjectModel.RemoveObject(this);
        }
        #endregion
        
        #region Asset Management
        protected virtual void LoadAsset()
        {
        }
        #endregion

        public ObjectState State { get; private set; }
        public GameObject Model { get; protected set; }
        protected bool isPlayer;
    }
}
