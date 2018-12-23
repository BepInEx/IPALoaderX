using UnityEngine;
using UnityEngine.SceneManagement;

namespace IPALoaderX
{
    public class PluginComponent : MonoBehaviour
    {
        CompositePlugin plugins;
        bool freshlyLoaded = false;
        bool quitting = false;

        public static void Create()
        {
            new GameObject("IPA_PluginManager").AddComponent<PluginComponent>();
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            plugins = new CompositePlugin(PluginManager.Plugins);
            plugins.OnApplicationStart();
        }

        void Start()
        {
            OnLevelWasLoaded(SceneManager.GetActiveScene().buildIndex);
        }

        void Update()
        {
            if (freshlyLoaded)
            {
                freshlyLoaded = false;
                plugins.OnLevelWasInitialized(SceneManager.GetActiveScene().buildIndex);
            }

            plugins.OnUpdate();
        }

        void LateUpdate()
        {
            plugins.OnLateUpdate();
        }

        void FixedUpdate()
        {
            plugins.OnFixedUpdate();
        }

        void OnDestroy()
        {
            if (!quitting)
                Create();
        }
        
        void OnApplicationQuit()
        {
            plugins.OnApplicationQuit();
            quitting = true;
        }

        void OnLevelWasLoaded(int level)
        {
            plugins.OnLevelWasLoaded(level);
            freshlyLoaded = true;
        }
    }
}
