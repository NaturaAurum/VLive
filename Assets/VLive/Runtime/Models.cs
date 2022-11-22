using System;
using UnityEngine;
namespace VLive.Runtime
{
    public class Models : MonoBehaviour
    {
        public static Models Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
