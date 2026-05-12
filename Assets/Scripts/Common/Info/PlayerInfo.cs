using Generated.Table;
using InGame.Component.Model;
using InGame.Object;

namespace Common.Info
{
    public class PlayerInfo
    {
        public PlayerInfo(CharacterModel characterModel)
        {
            CharacterModel = characterModel;
        }

        public CharacterModel CharacterModel { get; private set; }
        
        public CharacterBase PlayerObject { get; set; }
    }
}