using System.Collections.Generic;
using InGame.Model;
using UnityEngine;

namespace InGame.Object
{
    public class ObjectSpawner : MonoBehaviour
    {
        protected readonly List<int> Characters = new ();
        
        public void Init(InGameModel inGameModel)
        {
        }
    }
}
