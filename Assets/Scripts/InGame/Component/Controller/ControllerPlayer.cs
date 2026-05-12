using InGame.Component.Hub;
using InGame.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InGame.Component.Controller
{
    public class ControllerPlayer : ControllerBase
    {
        private PlayerInputAction _inputAction;

        public override void Init(InputHub inputHub)
        {
            base.Init(inputHub);
            _inputAction = new PlayerInputAction();
            _inputAction.Enable();
            
            _inputAction.Player.Move.performed += OnMove;
            _inputAction.Player.Move.canceled += OnMove;
            _inputAction.Player.Drag.performed += OnDrag;
            _inputAction.Player.Drag.canceled += OnDrag;
            _inputAction.Player.LeftClick.performed += OnLeftPress;
            _inputAction.Player.LeftClick.canceled += OnLeftRelease;
            _inputAction.Player.RightClick.performed += OnRightPress;
            _inputAction.Player.RightClick.canceled += OnRightRelease;
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        #region Events
        private void OnMove(InputAction.CallbackContext context)
        {
            var direction = context.ReadValue<Vector2>();
            InputHub.NotifyMove(direction);
        }

        private void OnDrag(InputAction.CallbackContext context)
        {
            var delta = context.ReadValue<Vector2>();
            InputHub.NotifyDrag(delta);
        }

        private void OnLeftPress(InputAction.CallbackContext context) => InputHub.NotifyLeftClick(true);
        private void OnLeftRelease(InputAction.CallbackContext context) => InputHub.NotifyLeftClick(false);
        private void OnRightPress(InputAction.CallbackContext context) => InputHub.NotifyRightClick(true);
        private void OnRightRelease(InputAction.CallbackContext context) => InputHub.NotifyRightClick(false);
        #endregion

        private void OnDestroy()
        {
            _inputAction?.Disable();
            _inputAction?.Dispose();
        }
    }
}
