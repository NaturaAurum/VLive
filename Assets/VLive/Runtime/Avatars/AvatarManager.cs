using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniGLTF;
using UnityEngine;
using UnityEngine.UI;
using VLive.Runtime.MediaPipe;
using VRM;
using VRMShaders;
namespace VLive.Runtime.Avatars
{

    public class AvatarElementInfo
    {
        public string Title;
        public Texture Tex;
        public string Path;
    }
    
    public class AvatarManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject avatarElementPreset;
        [SerializeField]
        private Button startButton;
        [SerializeField]
        private GameObject avatarPanel;
        [SerializeField]
        private Transform modelParent;
        [SerializeField]
        private RuntimeAnimatorController animatorController;
        [SerializeField]
        private HolisticReceiver receiver;
        [SerializeField]
        private HolisticController controller;
        
        private bool _checkFileDone;

        private RuntimeGltfInstance _context;
        private GameObject _avatar;

        private string FilePath => $"{Application.persistentDataPath}/AvatarInfo.json";

        private AvatarFileInfo _info;

        private List<AvatarElementInfo> _elementInfoList = new();
        private List<AvatarElement> _elementList = new();

        private void Awake()
        {
            startButton.interactable = false;
            CheckFile().Forget();
            startButton.OnClickAsAsyncEnumerable().Subscribe(OnStartButtonClicked).AddTo(this.GetCancellationTokenOnDestroy());
        }
        
        private async UniTaskVoid OnStartButtonClicked(AsyncUnit _, CancellationToken token)
        {
            avatarPanel.SetActive(false);
            var path = _info.avatarPathList[_info.selectedIndex];
            using var gltfData = new GlbFileParser(path).Parse();
            var vrmData = new VRMData(gltfData);
            using var vrmImporterContext = new VRMImporterContext(vrmData);
            var measure = new ImporterContextSpeedLog();
            var runtimeGltfInstance = await vrmImporterContext.LoadAsync(new ImmediateCaller(), measure.MeasureTime);
            _context = runtimeGltfInstance;
            _avatar = runtimeGltfInstance.Root;
            runtimeGltfInstance.EnableUpdateWhenOffscreen();
            runtimeGltfInstance.ShowMeshes();

            var animator = _avatar.GetComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;
            animator.Update(0.0f);
            animator.enabled = false;
            _avatar.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            var target = animator.GetBoneTransform(HumanBodyBones.Head);
            if (target)
            {
                var mainCam = Camera.main;
                if (mainCam)
                {
                    var mainCamPos = mainCam.transform.position;

                    var targetPosition = target.position;
                    var avatarPos = _avatar.transform.position;

                    var dir = targetPosition - avatarPos;
                    var dis = dir.magnitude;
                    avatarPos.y = mainCamPos.y - dis;
                    _avatar.transform.position = avatarPos;
                }
            }
            foreach (var vrmSpringBone in _avatar.GetComponentsInChildren<VRMSpringBone>())
            {
                vrmSpringBone.m_updateType = VRMSpringBone.SpringBoneUpdateType.LateUpdate;
            }

            var vrmLookAtHead = _avatar.GetComponent<VRMLookAtHead>();
            vrmLookAtHead.Head = animator.GetBoneTransform(HumanBodyBones.Head);
            var lookAtTarget = new GameObject("LookAtTarget");
            lookAtTarget.transform.SetParent(_avatar.transform);
            lookAtTarget.transform.position = vrmLookAtHead.Head.position + vrmLookAtHead.Head.forward;
            vrmLookAtHead.UpdateType = UpdateType.LateUpdate;
            vrmLookAtHead.Target = lookAtTarget.transform;

            receiver.SetModel(_avatar);
            controller.Run();
        }

        private async UniTaskVoid CheckFile()
        {
            if (!File.Exists(FilePath))
            {
                File.Create(FilePath);
                _info = AvatarFileInfo.Create();
            }
            else
            {
                var json = await File.ReadAllTextAsync(FilePath);
                _info = string.IsNullOrEmpty(json) ? AvatarFileInfo.Create() : JsonUtility.FromJson<AvatarFileInfo>(json);
            }
            _checkFileDone = true;
            UpdateInfo().Forget();
        }

        public void AddFile(string[] paths)
        {
            if (paths.Length == 0)
            {
                return;
            }
            var path = paths[0];
            AddVrm(path).Forget();
        }

        private async UniTask SaveInfo()
        {
            var json = JsonUtility.ToJson(_info);
            await File.WriteAllTextAsync(FilePath, json, this.GetCancellationTokenOnDestroy());
        }

        private void UpdateSelection()
        {
            for (var i = 0; i < _elementList.Count; i++)
            {
                _elementList[i].SelectionUpdate(i == _info.selectedIndex);
            }
            startButton.interactable = _info.selectedIndex > -1;
        }

        private async UniTaskVoid UpdateInfo()
        {
            for (var i = 0; i < _info.avatarPathList.Count; i++)
            {
                if (i > _elementInfoList.Count - 1)
                {
                    _elementInfoList.Add(new AvatarElementInfo());
                    var elementObject = Instantiate(avatarElementPreset, avatarElementPreset.transform.parent);
                    elementObject.SetActive(true);
                    elementObject.transform.SetSiblingIndex(i);
                    var avatarElement = elementObject.GetComponent<AvatarElement>();
                    async void OnAvatarElementOnClicked()
                    {
                        _info.selectedIndex = avatarElement.transform.GetSiblingIndex();
                        await SaveInfo();
                        UpdateSelection();
                    }
                    avatarElement.OnClicked += OnAvatarElementOnClicked;
                    _elementList.Add(avatarElement);
                }

                var path = _info.avatarPathList[i];
                var elem = _elementList[i];
                var elemInfo = _elementInfoList[i];
                if (elemInfo.Path == path)
                {
                    continue;
                }

                using var gltfData = new GlbFileParser(path).Parse();
                var vrmData = new VRMData(gltfData);
                using var vrmImporterContext = new VRMImporterContext(vrmData);
                var vrmMetaObject = await vrmImporterContext.ReadMetaAsync(new ImmediateCaller(), true);
                elemInfo.Path = path;
                elemInfo.Title = vrmMetaObject.Title;
                elemInfo.Tex = vrmMetaObject.Thumbnail;
                elem.Set(elemInfo);
            }
        }

        private async UniTaskVoid AddVrm(string path)
        {
            if (_info.avatarPathList.Contains(path))
            {
                return;
            }
            _info.avatarPathList.Add(path);
            await SaveInfo();
            UpdateInfo().Forget();
        }
    }
}
