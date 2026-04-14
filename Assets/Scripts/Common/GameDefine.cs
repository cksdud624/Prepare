using Unity.VectorGraphics;

namespace Common
{
    public static class GameDefine
    {
        public enum SceneType
        {
            BootStrap = 0,
            Main = 1
        }

        public enum ObjectState
        {
            None,
            Ready,
            Playing,
            Error
        }

        public enum CommonAnimation
        {
            Idle
        }
    }
}