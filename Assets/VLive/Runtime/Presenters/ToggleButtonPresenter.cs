using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VLive.Runtime.Models;
namespace VLive.Runtime.Presenters
{
    public class ToggleButtonPresenter : MonoBehaviour
    {
        [SerializeField]
        private string onText;
        [SerializeField]
        private string offText;
        
        private Button _button;
        private TMP_Text _textComp;
        
        protected virtual ToggleModel Toggle { get; }

        private void Awake()
        {
            _button = GetComponent<Button>();
            _textComp = GetComponentInChildren<TMP_Text>();
        }

        private void Start()
        {
            _button.OnClickAsAsyncEnumerable().Subscribe(OnButtonClicked).AddTo(this.GetCancellationTokenOnDestroy());
            Toggle.Subscribe(OnToggle).AddTo(this.GetCancellationTokenOnDestroy());
        }

        private void OnToggle(bool toggle)
        {
            _textComp.text = toggle ? onText : offText;
        }

        private void OnButtonClicked(AsyncUnit _)
        {
            Toggle.Toggle();
        }
    }

}
