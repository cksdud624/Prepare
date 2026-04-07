using Common.Scene.Parameter;
using Table;

namespace InGame.Model
{
    public class InGameModel
    {
        public StageData StageData { get; private set; }

        public InGameModel(SceneParameterMain sceneParameterMain)
        {
            StageData = sceneParameterMain.StageData;
        }
    }
}
