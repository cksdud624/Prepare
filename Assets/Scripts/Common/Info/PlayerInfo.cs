using System;
using InGame.Component.Model;
using InGame.Object;
using UnityEngine;

namespace Common.Info
{
    public class PlayerInfo
    {
        public PlayerInfo(CharacterModel characterModel)
        {
            CharacterModel = characterModel;
        }

        public CharacterModel CharacterModel { get; private set; }
        public CharacterBase PlayerObject { get; private set; }
        
        public event Action<CharacterBase> OnPlayerObjectChanged;

        public void SetPlayer(CharacterBase player)
        {
            if (player == null)
            {
                Debug.LogError("Player is null");
                return;
            }
            
            PlayerObject = player; 
            CharacterModel.IsPlayer = true;
            OnPlayerObjectChanged?.Invoke(player);
        }
    }
}