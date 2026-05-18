using System;
using UnityEngine;

namespace InGame.Component.Hub
{
    public class InputHub
    {
        public Vector2 MoveDirection { get; private set; }
        public event Action<Vector2> OnMove;
        public void NotifyMove(Vector2 direction)
        {
            MoveDirection = direction;
            OnMove?.Invoke(direction);
        }
        
        public Vector2 DragDelta { get; private set; }
        public event Action<Vector2> OnDrag;

        public void NotifyDrag(Vector2 drag)
        {
            DragDelta = drag;
            OnDrag?.Invoke(drag);
        }

        public event Action<bool> OnLeftClick;
        public event Action<bool> OnRightClick;
        public event Action<bool> OnShiftClick;
        public void NotifyLeftClick(bool isClick) => OnLeftClick?.Invoke(isClick);
        public void NotifyRightClick(bool isClick) => OnRightClick?.Invoke(isClick);
        public void NotifyShiftClick(bool isClick) => OnShiftClick?.Invoke(isClick);

        public void Dispose()
        {
            OnMove = null;
            OnDrag = null;
            OnLeftClick = null;
            OnRightClick = null;
            OnShiftClick = null;
        }
    }
}