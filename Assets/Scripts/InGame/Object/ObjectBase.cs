using Cysharp.Threading.Tasks;
using Generated.Table;
using InGame.Controller;
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
            Hub = new ();
            Hub.isPlayer = isPlayer;
            inGameModel = model;
            AddObject();
            AddParts();
            Hub.State = ObjectState.Ready;
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
        
        protected ObjectHub Hub { get; set; }
    }
}
