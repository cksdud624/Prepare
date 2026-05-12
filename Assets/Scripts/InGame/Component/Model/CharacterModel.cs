using System.Collections.Generic;
using Generated.Table;

namespace InGame.Component.Model
{
    public class CharacterModel
    {
        public CharacterModel(CharacterData characterData, List<WeaponStatusData> weaponStatusDataList)
        {
            CharacterData = characterData;
            WeaponStatusDataList = weaponStatusDataList;
        }
        
        public CharacterData CharacterData { get; private set; }

        public IReadOnlyList<WeaponStatusData> WeaponStatusDataList { get; private set; }
    }
}
