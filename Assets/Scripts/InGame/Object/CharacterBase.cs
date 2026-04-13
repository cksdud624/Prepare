using UnityEngine;
using UnityEngine.AddressableAssets;

namespace InGame.Object
{
    public class CharacterBase : ObjectBase
    {
        #region Object Management

        protected override void AddObject()
        {
            inGameModel.InGameObject.AddCharacter(this);
        }

        protected override void OnDestroy()
        {
            inGameModel.InGameObject.RemoveCharacter(this);
        }
        #endregion
        
        #region Asset Management
        protected override void LoadAsset()
        {
        }
        #endregion
        
        
        [SerializeField] protected Animator animator;
        protected GameObject Model;
        
        protected Rigidbody Rigidbody;
    }
}
