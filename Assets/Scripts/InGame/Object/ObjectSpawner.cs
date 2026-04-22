using System.Collections.Generic;
using Generated.Table;
using InGame.Model;
using Unity.Mathematics;
using UnityEngine;

namespace InGame.Object
{
    public class ObjectSpawner : MonoBehaviour
    {
        [SerializeField] private ObjectBase objectPrefab;
        [SerializeField] private CharacterBase characterPrefab;
        private InGameModel _inGameModel;
        
        public void Init(InGameModel inGameModel)
        {
            _inGameModel = inGameModel;
            _inGameModel.OnSpawnPlayer += SpawnPlayer;
            _inGameModel.OnSpawnCharacters += SpawnCharacters;
        }

        private void SpawnPlayer(PlayerSpawnData playerSpawnData)
        {
            var rotation = Quaternion.Euler(new Vector3(0, playerSpawnData.SpawnRotation, 0));
            if (_inGameModel.InGameObjectModel.Player == null)
            {
                if (_inGameModel.InGameObjectModel.PlayerData == null) Debug.LogError("PlayerData is null");
                var player = Instantiate(characterPrefab, playerSpawnData.SpawnPosition, rotation);
                player.Init(_inGameModel, _inGameModel.InGameObjectModel.PlayerData, true);
            }
            else
            {
                var player  = _inGameModel.InGameObjectModel.Player;
                player.transform.position = playerSpawnData.SpawnPosition;
                player.transform.rotation = rotation;
            }
        }

        private void SpawnCharacters(List<CharacterSpawnData> characterSpawnDatas)
        {
            throw new System.NotImplementedException("캐릭터 스폰");
        }

        private void SpawnObjects()
        {
            throw new System.NotImplementedException("오브젝트 스폰");
        }
    }
}
