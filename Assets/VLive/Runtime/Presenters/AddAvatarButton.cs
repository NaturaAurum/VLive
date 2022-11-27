using System;
using SFB;
using UnityEngine;
using UnityEngine.EventSystems;
using VLive.Runtime.Avatars;
namespace VLive.Runtime.Presenters
{
    public class AddAvatarButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private AvatarManager avatarManager;

        public void OnPointerClick(PointerEventData eventData)
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            StandaloneFileBrowser.OpenFilePanelAsync("VRM 아바타 찾기", directory, "vrm", false, avatarManager.AddFile);
        }
    }
}
