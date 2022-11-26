using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Mediapipe;
using Mediapipe.Unity;
using Mediapipe.Unity.Holistic;
using UnityEngine;
using VLive.Runtime.Extensions;
using VLive.Runtime.Models;
using Logger = Mediapipe.Unity.Logger;
using Screen = Mediapipe.Unity.Screen;
namespace VLive.Runtime.MediaPipe
{
    [RequireComponent(typeof(TextureFramePool))]
    [RequireComponent(typeof(WebCamSource))]
    [RequireComponent(typeof(HolisticTrackingGraph))]
    public class HolisticController : MonoBehaviour
    {
        private const bool EnableGLog = true;
        private const RunningMode RunningMode = Mediapipe.Unity.RunningMode.Sync;

        public InferenceMode InferenceMode { get; private set; }


        private LandmarkList _poseWorldLandmarks;
        private NormalizedLandmarkList _poseLandmarks;
        private NormalizedLandmarkList _faceLandmarks;
        private NormalizedLandmarkList _leftHandLandmarks;
        private NormalizedLandmarkList _rightHandLandmarks;

        private readonly PoseData _poseData = new();
        private readonly HandsData _handsData = new();
        private readonly FaceData _faceData = new();

        [SerializeField]
        private Screen screen;
        [SerializeField]
        private InferenceMode preferableInferenceMode;
        [SerializeField]
        private ImageSourceType imageSourceType;
        [SerializeField]
        private HolisticReceiver receiver;
        [SerializeField]
        private HolisticLandmarkListAnnotationController holisticAnnotationController;
        // [SerializeField] private MaskAnnotationController maskAnnotationController;
        [SerializeField]
        private LandmarkDrawer landmarkDrawer;

        private TextureFramePool _textureFramePool;
        private HolisticTrackingGraph _holisticGraphRunner;

        private bool _gLogInitialized = false;

        private RectTransform _screenRect;

        private ImageSource ImageSource => ImageSourceProvider.ImageSource;

        private ToggleModel PointToggle => StaticModels.Instance.PointToggle;
        
        private void Awake()
        {
            _textureFramePool = GetComponent<TextureFramePool>();
            _holisticGraphRunner = GetComponent<HolisticTrackingGraph>();
            _screenRect = screen.GetComponent<RectTransform>();
            UniTaskAsyncEnumerable
                .EveryUpdate(PlayerLoopTiming.PostLateUpdate)
                .Subscribe(OnLateUpdate)
                .AddTo(this.GetCancellationTokenOnDestroy());
        }
        
        private void OnLateUpdate(AsyncUnit _)
        {
            var poseVisible = _poseLandmarks.IsValid() && _poseWorldLandmarks.IsValid();
            var modelRot = receiver.transform.rotation;
            modelRot *= Quaternion.Euler(0, 180, 0);
            if (!poseVisible)
            {
                _poseData.NewPosList.Clear();
            }
            else
            {
                var pose2D = _poseLandmarks.Landmark;
                var pose3D = _poseWorldLandmarks.Landmark;

                var pose2 = pose2D.ToVector3List(_screenRect);
                var pose3 = pose3D.ToVector3List(_screenRect, 1.3f);
                _poseData.NewPosList = pose2.Select((point, index) =>
                {
                    var point2D = point.Point;
                    point2D = Vector3.Scale(point.Point, Vector3.one * 0.01f);
                    var point3D = pose3[index].Point;
                    point2D.z = point3D.z;
                    // point2D.y = -point2D.y;
                    // point2D.y += 5.7f;
                    point2D.x += 2.4f;
                    point.Point = point2D;
                    return point;
                }).ToList();
                // _poseData.NewPosList = pose3.Select(point =>
                // {
                //     var pt = point.Point;
                //     pt.x += Vector3.left.x;
                //     point.Point = pt;
                //     return point;
                // }).ToList();
                _poseData.NewPosList = _poseData.NewPosList.Select(point => point.ApplyRotation(modelRot)).ToList();
            }

            var leftHandValid = _leftHandLandmarks.IsValid();
            var rightHandValid = _rightHandLandmarks.IsValid();

            if (leftHandValid)
            {
                _handsData.LeftPosList = _leftHandLandmarks.Landmark.ToVector3List(_screenRect);
                // _handsData.LeftPosList = _handsData.LeftPosList.Select(point => point.ApplyRotation(Quaternion.Euler(0, 180, 0))).ToList();
            }
            else
            {
                _handsData.LeftPosList.Clear();
            }

            if (rightHandValid)
            {
                _handsData.RightPosList = _rightHandLandmarks.Landmark.ToVector3List(_screenRect);
                // _handsData.RightPosList = _handsData.RightPosList.Select(point => point.ApplyRotation(Quaternion.Euler(0, 180, 0))).ToList();
            }
            else
            {
                _handsData.RightPosList.Clear();
            }

            var faceValid = _faceLandmarks.IsValid();
            if (faceValid)
            {
                // _faceData.PosList = _faceLandmarks.Landmark.Select(lm =>
                // {
                //     var pointData = lm.ToVector3();
                //     var pt = pointData.Point;
                //     pt.y *= -1;
                //     pointData.Point = pt;
                //     return pointData;
                // }).ToList();
                _faceData.PosList = _faceLandmarks.Landmark.ToVector3List(_screenRect).Select(point =>
                {
                    point.Point = Vector3.Scale(point.Point, Vector3.one * 0.01f);
                    return point;
                }).ToList();
                _faceData.Origin = _faceLandmarks;
            }
            else
            {
                _faceData.PosList.Clear();
            }
            receiver.SolvePose(_poseData);
            receiver.SolveHand(_handsData);
            
            receiver.SolveFace(_faceData);
            landmarkDrawer.Draw(_poseData, _faceData, _handsData);
        }

        private IEnumerator Start()
        {
            yield return InitSettings();
            Toggle(false);
            PointToggle.Subscribe(Toggle).AddTo(this.GetCancellationTokenOnDestroy());
            yield return Run();
        }

        private void Toggle(bool toggle)
        {
            holisticAnnotationController.gameObject.SetActive(toggle);
        }
        
        private IEnumerator InitSettings()
        {
            Logger.SetLogger(new MemoizedLogger(100));
            Logger.MinLogLevel = Logger.LogLevel.Debug;
            Protobuf.SetLogHandler(Protobuf.DefaultLogHandler);
            GlobalConfigManager.SetFlags();
            if (EnableGLog)
            {
                if (Glog.LogDir != null)
                {
                    if (!Directory.Exists(Glog.LogDir))
                    {
                        Directory.CreateDirectory(Glog.LogDir);
                    }
                    Logger.LogVerbose(nameof(HolisticController), $"Glog will output files under {Glog.LogDir}");
                }
                Glog.Initialize("MediaPipeUnityPlugin");
                _gLogInitialized = true;
            }
            AssetLoader.Provide(new StreamingAssetsResourceManager());

            DecideInferenceMode();
            if (InferenceMode == InferenceMode.GPU)
            {
                yield return GpuManager.Initialize();
            }
            ImageSourceProvider.ImageSource = GetImageSource();
        }

        private ImageSource GetImageSource()
        {
            switch (imageSourceType)
            {
                case ImageSourceType.Image:
                    return GetComponent<StaticImageSource>();
                case ImageSourceType.WebCamera:
                    var webCamSource = GetComponent<WebCamSource>();
                    webCamSource.isHorizontallyFlipped = true;
                    return webCamSource;
                case ImageSourceType.Video:
                case ImageSourceType.Unknown:
                default:
                    return null;
            }
        }
        
        private IEnumerator Run()
        {
            // var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_destroyToken, _runCts.Token).Token;
            var graphInitReq = _holisticGraphRunner.WaitForInit(RunningMode);
            yield return ImageSource.Play();
            if (!ImageSource.isPrepared)
            {
                yield break;
            }

            _textureFramePool.ResizeTexture(ImageSource.textureWidth, ImageSource.textureHeight, TextureFormat.RGBA32);
            screen.Initialize(ImageSource);
            yield return graphInitReq;

            if (graphInitReq.isError)
            {
                yield break;
            }
            AddGraphListener();
            _holisticGraphRunner.StartRun(ImageSource);
            // maskAnnotationController.InitScreen(ImageSource.textureWidth, ImageSource.textureHeight);
            while (true)
            {
                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                ReadFromImageSource(textureFrame);
                _holisticGraphRunner.AddTextureFrameToInputStream(textureFrame);
                yield return new WaitForEndOfFrame();
            }
        }
        
        private void AddGraphListener()
        {
            _holisticGraphRunner.OnFaceLandmarksOutput += OnFaceLandmarks;
            _holisticGraphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarks;
            _holisticGraphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarks;
            _holisticGraphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarks;
            _holisticGraphRunner.OnPoseLandmarksOutput += OnPoseLandmarks;
        }

        private void OnPoseLandmarks(object sender, OutputEventArgs<NormalizedLandmarkList> e)
        {
            _poseLandmarks = e.value;
            holisticAnnotationController.DrawPoseLandmarkListLater(e.value);
        }
        private void OnPoseWorldLandmarks(object sender, OutputEventArgs<LandmarkList> e)
        {
            _poseWorldLandmarks = e.value;
        }
        private void OnRightHandLandmarks(object sender, OutputEventArgs<NormalizedLandmarkList> e)
        {
            _rightHandLandmarks = e.value;
            holisticAnnotationController.DrawRightHandLandmarkListLater(e.value);
        }
        private void OnLeftHandLandmarks(object sender, OutputEventArgs<NormalizedLandmarkList> e)
        {
            _leftHandLandmarks = e.value;
            holisticAnnotationController.DrawLeftHandLandmarkListLater(e.value);
        }
        private void OnFaceLandmarks(object sender, OutputEventArgs<NormalizedLandmarkList> e)
        {
            _faceLandmarks = e.value;
            holisticAnnotationController.DrawFaceLandmarkListLater(e.value);
        }

        private void ReadFromImageSource(TextureFrame textureFrame)
        {
            var sourceTexture = ImageSource.GetCurrentTexture();
            var textureType = sourceTexture.GetType();

            if (textureType == typeof(WebCamTexture))
            {
                textureFrame.ReadTextureFromOnCPU((WebCamTexture)sourceTexture);
            }
            else if (textureType == typeof(Texture2D))
            {
                textureFrame.ReadTextureFromOnCPU((Texture2D)sourceTexture);
            }
            else
            {
                textureFrame.ReadTextureFromOnCPU(sourceTexture);
            }
        }
        
        private void DecideInferenceMode()
        {
#if UNITY_EDITOR
            InferenceMode = InferenceMode.CPU;
#else
            InferenceMode = preferableInferenceMode;
#endif
        }

        private void OnDestroy()
        {
            GpuManager.Shutdown();
            if (_gLogInitialized)
            {
                Glog.Shutdown();
            }

            Protobuf.ResetLogHandler();
            Logger.SetLogger(null);
        }
    }
}
