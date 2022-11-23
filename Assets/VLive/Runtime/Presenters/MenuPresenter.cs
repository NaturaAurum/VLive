using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VLive.Runtime.Models;
namespace VLive.Runtime.Presenters
{
    public class MenuPresenter : MonoBehaviour
    {
        // [SerializeField]
        // private RawImage webCamTexture;

        [SerializeField]
        private Button pointToggleButton;

        private StaticModels Model => StaticModels.Instance;
        
        private void Awake()
        {
            pointToggleButton.OnClickAsObservable().Subscribe().AddTo(this);
        }
    }
}
