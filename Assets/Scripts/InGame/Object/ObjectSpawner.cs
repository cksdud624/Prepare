using System.Collections.Generic;
using Common.Info;
using Generated.Table;
using InGame.Model;
using Unity.VisualScripting;
using UnityEngine;

namespace InGame.Object
{
    public class ObjectSpawner : MonoBehaviour
    {
        [SerializeField] private ObjectBase objectBase;
        [SerializeField] private CharacterBase characterBase;
        private InGameModel _inGameModel;

        private List<ObjectBase> _objects = new ();
        private List<CharacterBase> _characters = new ();
        
        public void Init(InGameModel inGameModel)
        {
            _inGameModel = inGameModel;
        }

        public void SpawnPlayer(PlayerInfo playerInfo)
        {
            if (playerInfo.PlayerObject != null)
                return;

            var player = Instantiate(characterBase);
            player.Init(_inGameModel, playerInfo.CharacterData);
            _objects.Add(player);
            _characters.Add(player);
            playerInfo.PlayerObject = player;
        }

        public void SpawnCharacter(CharacterData characterData)
        {
            
        }
    }
}