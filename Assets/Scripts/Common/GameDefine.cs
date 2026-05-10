using UnityEngine;

namespace Common
{
    public static class GameDefine
    {
        public enum SceneType
        {
            BootStrap = 0,
            Main = 1
        }

        public enum ObjectType
        {
            Object,
            Character
        }

        public enum ObjectState
        {
            Raw,
            Loading,
            Ready,
            Playing,
            Sleep,
            Error
        }

        public static readonly Vector3 DefaultPlayerSight = new (0, 1.6f, 0);
    }
}