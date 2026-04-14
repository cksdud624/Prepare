using System;
using System.Collections.Generic;
using Common.Scene.Parameter;
using Generated.Table;

namespace InGame.Model
{
    public class InGameModel
    {
        #region Events
        public Action<PlayerSpawnData> OnSpawnPlayer { get; set; }
        public Action<List<CharacterSpawnData>> OnSpawnCharacters { get; set; }
        public Action OnAssetInitialized { get; set; }
        #endregion
        
        #region Models
        public InGameObjectModel InGameObjectModel { get; private set; }
        public InGameAssetModel InGameAssetModel { get; private set; }
        #endregion
        
        #region Variables
        public StageData StageData { get; private set; }
        #endregion

        public InGameModel(SceneParameterMain sceneParameterMain)
        {
            InGameObjectModel = new (sceneParameterMain);
            InGameAssetModel = new ();
            StageData = sceneParameterMain.StageData;
        }

        public void Release()
        {
            OnSpawnPlayer = null;
            OnSpawnCharacters = null;
            OnAssetInitialized = null;
        }
    }
}
