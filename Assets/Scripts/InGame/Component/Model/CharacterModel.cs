using System.Collections.Generic;
using Generated.Table;
using UnityEngine;
using CombatState = Common.GameDefine.CombatState;

namespace InGame.Component.Model
{
    public class CharacterModel
    {
        public CharacterModel(CharacterData characterData, List<WeaponStatusData> weaponStatusDataList)
        {
            CharacterData = characterData;
            WeaponStatusDataList = weaponStatusDataList;
            SetWeapon(0);
        }
        
        public CharacterData CharacterData { get; private set; }

        public IReadOnlyList<WeaponStatusData> WeaponStatusDataList { get; private set; }
        
        public WeaponStatusData CurrentWeapon { get; private set; }
        public CombatState CombatState { get; set; } = CombatState.Standard;

        public void SetWeapon(int index)
        {
            if (WeaponStatusDataList == null || WeaponStatusDataList.Count <= index)
            {
                Debug.LogError("WeaponStatusData is null");
                return;
            }
            
            CurrentWeapon = WeaponStatusDataList[index];
        }
    }
}
