using InGame.Component.Hub;
using UnityEngine;

namespace InGame.Component.Controller
{
    public abstract class ControllerBase : MonoBehaviour
    {
        protected InputHub InputHub;
        public virtual void Init(InputHub inputHub)
        {
            InputHub = inputHub;
        }
    }
}
