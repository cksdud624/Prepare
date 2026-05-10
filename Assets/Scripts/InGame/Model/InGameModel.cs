using System;
using Common.Info;
using Common.Scene.Parameter;

namespace InGame.Model
{
    public class InGameModel
    {
        public InGameModel(SceneParameterMain parameter)
        {
            PlayerInfo = parameter.PlayerInfo;
        }
        
        #region Variables
        public PlayerInfo PlayerInfo { get; private set; }
        #endregion

        public void Dispose()
        {
        }
    }
}
