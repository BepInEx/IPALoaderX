using UnityEngine;

namespace IllusionInjector
{
	public class PluginComponent : MonoBehaviour
	{
		private bool freshlyLoaded;
		private CompositePlugin plugins;
		private bool quitting;

		public static PluginComponent Create()
		{
			return new GameObject("IPA_PluginManager").AddComponent<PluginComponent>();
		}

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);

			plugins = new CompositePlugin(PluginManager.Plugins);
			plugins.OnApplicationStart();
		}

		private void Start()
		{
			OnLevelWasLoaded(Application.loadedLevel);
		}

		private void Update()
		{
			if (freshlyLoaded)
			{
				freshlyLoaded = false;
				plugins.OnLevelWasInitialized(Application.loadedLevel);
			}

			plugins.OnUpdate();
		}

		private void LateUpdate()
		{
			plugins.OnLateUpdate();
		}

		private void FixedUpdate()
		{
			plugins.OnFixedUpdate();
		}

		private void OnDestroy()
		{
			if (!quitting)
				Create();
		}

		private void OnApplicationQuit()
		{
			plugins.OnApplicationQuit();

			quitting = true;
		}

		private void OnLevelWasLoaded(int level)
		{
			plugins.OnLevelWasLoaded(level);
			freshlyLoaded = true;
		}
	}
}