using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace VLive.Runtime.Presenters
{
    public class LoadAvatarButton : MonoBehaviour
    {
        [SerializeField]
        private GameObject avatarPanel;
        
        private Button _button;
        private TMP_Text _buttonText;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.OnClickAsAsyncEnumerable().Subscribe(OnClicked).AddTo(this.GetCancellationTokenOnDestroy());
            _buttonText = GetComponentInChildren<TMP_Text>();
            UniTaskAsyncEnumerable.EveryUpdate().Subscribe(EveryUpdate).AddTo(this.GetCancellationTokenOnDestroy());
        }
        private void EveryUpdate(AsyncUnit _)
        {
            _buttonText.text = avatarPanel.activeSelf ? "닫기" : "아바타 불러오기";
        }

        private void OnClicked(AsyncUnit _)
        {
            avatarPanel.SetActive(!avatarPanel.activeSelf);
        }
    }
}
