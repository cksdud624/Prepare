using UnityEngine;

namespace Common.Template
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<T>();
                    singleton.name = typeof(T).Name;
                    DontDestroyOnLoad(singleton);
                }
                return _instance;
            }
        }
    }
}
