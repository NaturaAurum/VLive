using UnityEngine;
namespace VLive.Runtime.Models
{
    public class StaticModels : MonoBehaviour
    {
        public static StaticModels Instance { get; private set; }
        public ToggleModel PointToggle { get; } = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
