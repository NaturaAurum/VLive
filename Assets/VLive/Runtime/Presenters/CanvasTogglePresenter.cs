using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
namespace VLive.Runtime.Presenters
{
    public class CanvasTogglePresenter : MonoBehaviour
    {
        [SerializeField]
        private GameObject background;
        
        private void Awake()
        {
            UniTaskAsyncEnumerable.EveryUpdate().Subscribe(EveryUpdate).AddTo(this.GetCancellationTokenOnDestroy());
        }
        
        private void EveryUpdate(AsyncUnit _)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                gameObject.SetActive(!gameObject.activeSelf);
                background.SetActive(!background.activeSelf);
            }
        }
    }
}
