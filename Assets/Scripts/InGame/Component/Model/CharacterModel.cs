using System.Collections.Generic;
using Common;
using Generated.Table;
using UniRx;
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

        #region Data
        public CharacterData CharacterData { get; private set; }
        public bool IsPlayer { get; set; }
        #endregion
        
        #region Combat
        public IReadOnlyList<WeaponStatusData> WeaponStatusDataList { get; private set; }
        
        public WeaponStatusData CurrentWeapon { get; private set; }
        public readonly ReactiveProperty<CombatState> CombatState = new(GameDefine.CombatState.Standard);

        public void SetWeapon(int index)
        {
            if (WeaponStatusDataList == null || WeaponStatusDataList.Count <= index)
            {
                Debug.LogError("WeaponStatusData is null");
                return;
            }
            
            CurrentWeapon = WeaponStatusDataList[index];
        }
        #endregion
        
        #region Move
        public readonly ReactiveProperty<bool> IsSprint = new ();
        public readonly ReactiveProperty<Vector2> MoveDirection = new ();
        public readonly ReactiveProperty<bool> IsLand = new ();
        public readonly ReactiveProperty<bool> IsSpaceHolding = new();

        #endregion
    }
}
