using System;
using System.Collections.Generic;
using Common.Scene.Parameter;
using Generated.Table;

namespace InGame.Model
{
    public class InGameModel
    {
        public InGameModel(SceneParameterMain sceneParameterMain)
        {
            InGameObjectModel = new (sceneParameterMain);
            InGameAssetModel = new ();
            StageData = sceneParameterMain.StageData;
        }
        
        #region Events

        public event Action<PlayerSpawnData> OnSpawnPlayer;
        public void NotifyOnSpawnPlayer(PlayerSpawnData data) => OnSpawnPlayer?.Invoke(data);
        public event Action<List<CharacterSpawnData>> OnSpawnCharacters;
        public void NotifyOnSpawnCharacters(List<CharacterSpawnData> data) => OnSpawnCharacters?.Invoke(data);
        public event Action OnInitialized;
        public void NotifyOnInitialized() => OnInitialized?.Invoke();
        #endregion
        
        #region Models
        public InGameObjectModel InGameObjectModel { get; private set; }
        public InGameAssetModel InGameAssetModel { get; private set; }
        #endregion
        
        #region Variables
        public StageData StageData { get; private set; }
        #endregion

        public void Release()
        {
            OnSpawnPlayer = null;
            OnSpawnCharacters = null;
            OnInitialized = null;
        }
    }
}
