using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace VLive.Runtime.Avatars
{
    public class AvatarElement : MonoBehaviour, IPointerClickHandler
    {
        private Image _background;
        private RawImage _image;
        private TMP_Text _title;

        [SerializeField]
        private Sprite selectSprite;
        [SerializeField]
        private Sprite deselectSprite;

        public Action OnClicked;

        private void Awake()
        {
            _background = GetComponent<Image>();
            _image = GetComponentInChildren<RawImage>();
            _title = GetComponentInChildren<TMP_Text>();
        }

        private void SetImage(Texture texture)
        {
            _image.texture = texture;
        }

        private void SetTitle(string title)
        {
            _title.text = title;
        }

        public void Set(AvatarElementInfo info)
        {
            SetImage(info.Tex);
            SetTitle(info.Title);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke();
        }

        public void SelectionUpdate(bool select)
        {
            _background.sprite = select ? selectSprite : deselectSprite;
        }
    }
}
