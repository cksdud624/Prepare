using Common;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace InGame.Object
{
    public class CharacterBase : ObjectBase
    {
        #region Object Management
        protected override void AddObject()
        {
            inGameModel.InGameObjectModel.AddCharacter(this);
        }

        protected override void OnDestroy()
        {
            inGameModel.InGameObjectModel.RemoveCharacter(this);
        }
        #endregion
        
        #region Components
        protected override void AddParts()
        {
            var playerData = inGameModel.InGameObjectModel.PlayerData;
            var model = inGameModel.InGameAssetModel.GetModel(inGameModel.InGameObjectModel.PlayerData.Id);
            Model = Instantiate(model, this.transform);
            var animator = Model.GetComponent<Animator>();
            if(Animator != null)
                Animator = animator;
            var collider = model.GetComponent<Collider>();
            if(collider != null)
                Collider = collider;
            
            Rigidbody = gameObject.AddComponent<Rigidbody>();

            var animators = playerData.AnimatorId;
        }
        #endregion

        private Animator Animator { get; set; }
    }
}
