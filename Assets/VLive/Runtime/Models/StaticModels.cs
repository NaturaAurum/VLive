using UnityEngine;
namespace VLive.Runtime.Models
{
    public class StaticModels : MonoBehaviour
    {
        public static StaticModels Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
