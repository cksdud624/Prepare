using System.Collections.Generic;
using Generated.Table;
using InGame.Model;
using UnityEngine;

namespace InGame.Object
{
    public class ObjectSpawner : MonoBehaviour
    {
        protected readonly List<ObjectBase> objects = new ();
        protected readonly List<CharacterBase> characters = new ();
        
        
        public void Init(InGameModel inGameModel)
        {
            inGameModel.OnSpawnPlayer += SpawnPlayer;
            inGameModel.OnSpawnCharacters += SpawnCharacters;
        }

        private void SpawnPlayer(PlayerSpawnData playerSpawnData)
        {
            Debug.Log(playerSpawnData.SpawnPos);
        }

        private void SpawnCharacters(List<CharacterSpawnData> characterSpawnDatas)
        {
            throw new System.NotImplementedException("캐릭터 스폰");
        }
    }
}
