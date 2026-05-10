using System;
using System.Collections.Generic;
using Common.Info;
using Cysharp.Threading.Tasks;
using Generated.Table;
using InGame.Model;
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
        
        public async UniTask Init(InGameModel inGameModel)
        {
            _inGameModel = inGameModel;
            await UniTask.CompletedTask;
        }

        public async UniTask SpawnPlayer(PlayerInfo playerInfo, Action onPlayerSpawned = null)
        {
            if (playerInfo.PlayerObject != null)
            {
                Debug.LogError("PlayerObject is null");
                return;
            }

            var player = Instantiate(characterBase);
            await player.Init(_inGameModel, playerInfo.CharacterData);
            _objects.Add(player);
            _characters.Add(player);    
            playerInfo.PlayerObject = player;
        }
    }
}