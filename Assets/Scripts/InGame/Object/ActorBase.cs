using UnityEngine;
using UnityEngine.AddressableAssets;

namespace InGame.Object
{
    public class ActorBase : ObjectBase
    {
        [SerializeField] protected Animator animator;
        protected GameObject Model;
        
        protected Rigidbody Rigidbody;
    }
}
