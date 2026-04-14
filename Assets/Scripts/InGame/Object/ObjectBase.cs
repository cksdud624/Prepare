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
            AddParts();
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
        
        #region Components
        protected virtual void AddParts()
        {
        }
        #endregion

        public ObjectState State { get; private set; }
        protected GameObject Model { get; set; }
        protected Rigidbody Rigidbody { get; set; }
        protected Collider Collider { get; set; }
        protected bool isPlayer;
    }
}
