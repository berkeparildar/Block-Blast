using UnityEngine;

namespace Runtime.Extensions
{
    public class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        private static T ms_Instance;
        private const string SINGLETON_HOLDER_NAME = "SingletonHolder";

        public static T Instance
        {
            get
            {
                if (ms_Instance != null) return ms_Instance;

                if (ms_Instance == null)
                {
                    ms_Instance = FindObjectOfType<T>();

                    if (FindObjectsOfType<T>().Length > 1)
                    {
                        Debug.LogError("[SingletonMonoBehaviour] More than 1 singleton instance found!");
                        return ms_Instance;
                    }

                    if (ms_Instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        ms_Instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";
                        GameObject singletonHolder = GameObject.Find(SINGLETON_HOLDER_NAME);
                        ms_Instance.transform.SetParent(singletonHolder.transform);
                    }
                }
                return ms_Instance;
            }
        }

        private void OnDestroy()
        {
            ms_Instance = null;
        }
    }
}