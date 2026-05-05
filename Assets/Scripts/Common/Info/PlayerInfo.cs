using Generated.Table;
using InGame.Object;

namespace Common.Info
{
    public class PlayerInfo
    {
        public PlayerInfo(CharacterData characterData)
        {
            CharacterData = characterData;
        }

        public CharacterData CharacterData { get; private set; }
        
        public CharacterBase PlayerObject { get; set; }
    }
}