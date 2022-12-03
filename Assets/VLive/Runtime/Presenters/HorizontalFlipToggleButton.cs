using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Mediapipe.Unity;
using UnityEngine;
using UnityEngine.UI;
namespace VLive.Runtime.Presenters
{
    public class HorizontalFlipToggleButton : MonoBehaviour
    {
        private Button _button;

        [SerializeField]
        private WebCamSource webCamSource;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.OnClickAsAsyncEnumerable().Subscribe(OnButtonClicked).AddTo(this.GetCancellationTokenOnDestroy());
        }
        private void OnButtonClicked(AsyncUnit _)
        {
            webCamSource.isHorizontallyFlipped = !webCamSource.isHorizontallyFlipped;
        }
    }
}
