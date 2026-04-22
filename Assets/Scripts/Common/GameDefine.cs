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
            Sleep,
            Error
        }

        public enum InGameCommonAnimation
        {
            Idle,
            Walk,
        }
    }
}