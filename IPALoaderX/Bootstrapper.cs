using System;
using UnityEngine;

namespace IPALoaderX
{
    class Bootstrapper : MonoBehaviour
    {
        public event Action Destroyed = delegate {};

        void Start()
        {
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            Destroyed();
        }
    }
}
