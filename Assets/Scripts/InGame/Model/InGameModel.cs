using System;
using System.Collections.Generic;
using Common.Scene.Parameter;
using Generated.Table;

namespace InGame.Model
{
    public class InGameModel
    {
        public StageData StageData { get; private set; }

        public Action<PlayerSpawnData> OnSpawnPlayer { get; set; }

        public Action<List<CharacterSpawnData>> OnSpawnCharacters { get; set; }
        /*
         * 1. 플레이어 스폰
         * 2. 캐릭터 스폰
         */

        public InGameModel(SceneParameterMain sceneParameterMain)
        {
            StageData = sceneParameterMain.StageData;
        }

        public void Release()
        {
            OnSpawnPlayer = null;
            OnSpawnCharacters = null;
        }
    }
}
