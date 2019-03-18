using System;
using UnityEngine;

namespace IllusionInjector
{
	internal class Bootstrapper : MonoBehaviour
	{
		public event Action Destroyed = delegate { };

		private void Start()
		{
			Destroy(gameObject);
		}

		private void OnDestroy()
		{
			Destroyed();
		}
	}
}