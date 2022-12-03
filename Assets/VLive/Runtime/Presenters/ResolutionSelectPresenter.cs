using System.Linq;
using Cysharp.Threading.Tasks;
using Mediapipe.Unity;
using TMPro;
using UnityEngine;
using VLive.Runtime.MediaPipe;
namespace VLive.Runtime.Presenters
{
    public class ResolutionSelectPresenter : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown dropDown;

        [SerializeField]
        private HolisticController holisticController;

        private void Start()
        {
            Init().Forget();
        }

        public async UniTaskVoid Init()
        {
            dropDown.ClearOptions();
            dropDown.onValueChanged.RemoveAllListeners();
            dropDown.enabled = false;
            while (!holisticController.Prepared)
            {
                await UniTask.NextFrame(PlayerLoopTiming.Update);
            }
            
            var webCamSource = ImageSourceProvider.ImageSource;

            var resolutions = webCamSource.availableResolutions;

            if (resolutions == null)
            {
                return;
            }
            dropDown.enabled = true;
            var options = resolutions.Select(resolution => resolution.ToString()).ToList();
            dropDown.AddOptions(options);

            var currStr = webCamSource.resolution.ToString();
            var defaultValue = options.FindIndex(option => option == currStr);

            if (defaultValue >= 0)
            {
                dropDown.value = defaultValue;
            }
            
            dropDown.onValueChanged.AddListener(value => webCamSource.SelectResolution(value));
        }
    }
}
