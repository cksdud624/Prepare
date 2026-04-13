using System.Collections.Generic;
using Generated.Table;
using InGame.Model;
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
            var player = Instantiate(characterPrefab, playerSpawnData.SpawnPos, Quaternion.identity);
            player.Init(_inGameModel, true);
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
