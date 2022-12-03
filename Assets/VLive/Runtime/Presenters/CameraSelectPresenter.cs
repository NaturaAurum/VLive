using System.Collections.Generic;
using Mediapipe.Unity;
using TMPro;
using UnityEngine;
namespace VLive.Runtime.Presenters
{
    public class CameraSelectPresenter : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown dropDown;

        [SerializeField]
        private WebCamSource webCamSource;

        [SerializeField]
        private ResolutionSelectPresenter resolution;

        private void Start()
        {
            dropDown.ClearOptions();
            dropDown.onValueChanged.RemoveAllListeners();
            var sourceNames = webCamSource.sourceCandidateNames;

            if (sourceNames == null)
            {
                dropDown.enabled = false;
                return;
            }

            var options = new List<string>(sourceNames);
            dropDown.AddOptions(options);
            var currSourceName = webCamSource.sourceName;
            var defaultValue = options.FindIndex(option => option == currSourceName);

            if (defaultValue >= 0)
            {
                dropDown.value = defaultValue;
            }
            
            dropDown.onValueChanged.AddListener(OnDropDownValueChanged);
        }
        private void OnDropDownValueChanged(int value)
        {
            webCamSource.SelectSource(value);
            resolution.Init();
        }
    }
}
