using Common.Info;
using Generated.Table;
using UnityEngine;

namespace Common.Scene.Parameter
{
    public class SceneParameterMain
    {
        public SceneParameterMain(StageData stageData, PlayerInfo playerInfo)
        {
            StageData = stageData;
            PlayerInfo = playerInfo;
        }
        
        public StageData StageData { get; private set; }
        public PlayerInfo PlayerInfo { get; private set; }
    }
}