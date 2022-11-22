using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace VLive.Runtime
{
    public class HandsSolver : MonoBehaviour, IHandsSolver
    {
        [SerializeField]
        private Material skeletonMat;
        [SerializeField]
        private Mesh skeletonMesh;
        [SerializeField]
        private List<Transform> leftDrawer;
        [SerializeField]
        private List<Transform> rightDrawer;

        private static readonly string[] FingerNames = {
            "Thumb",
            "Index",
            "Middle",
            "Ring",
            "Little"
        };
        private static readonly string[] PartNames = {
            "Proximal",
            "Intermediate",
            "Distal",
            "Distal"
        };
        
        private static readonly Vector4 RightColumn = new(0, 0, 0, 1);
        
        private Vector3[] _leftFingerLocalCoords = new Vector3[21];
        private Vector3[] _rightFingerLocalCoords = new Vector3[21];

        private Animator _animator;

        private Plane _handPlane = new();

        private Transform _leftHand;
        private Transform _rightHand;

        private readonly List<Vector3KalmanFilter> _leftKalmanFilters = new();
        private readonly List<Vector3KalmanFilter> _rightKalmanFilters = new();
        private readonly List<OneEuroFilter<Vector3>> _leftOneEuroFilters = new();
        private readonly List<OneEuroFilter<Vector3>> _rightOneEuroFilters = new();

        private const double KalmanTimeInterval = 0.45;
        private const double KalmanNoise = 0.4;
        private const float MinCutOffValue = 1.5f;
        private const float DCutOffValue = 1f;
        private const float Beta = 0.1f;
        private const float Frequency = 90f;

        private float _leftAngVelocity;
        private float _rightAngVelocity;

        private const string Left = "Left";
        private const string Right = "Right";

        private Transform _chest;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _chest = _animator.GetBoneTransform(HumanBodyBones.Chest);

            InitializeFingerLocalCoord(ref _leftFingerLocalCoords, Left);
            InitializeFingerLocalCoord(ref _rightFingerLocalCoords, Right);
            for (var i = 0; i < 21; ++i)
            {
                _leftOneEuroFilters.Add(new OneEuroFilter<Vector3>(Frequency, MinCutOffValue, Beta, DCutOffValue));
                _rightOneEuroFilters.Add(new OneEuroFilter<Vector3>(Frequency, MinCutOffValue, Beta, DCutOffValue));
                _leftKalmanFilters.Add( new Vector3KalmanFilter(KalmanTimeInterval, KalmanNoise));
                _rightKalmanFilters.Add( new Vector3KalmanFilter(KalmanTimeInterval, KalmanNoise));
            }
        }


        private void InitializeFingerLocalCoord(ref Vector3[] fingerLocalCoords, string side)
        {
            var index = 0;
            foreach (var fingerName in FingerNames)
            {
                foreach (var partName in PartNames)
                {
                    var boneTransform = _animator.GetBoneTransform(Enum.Parse<HumanBodyBones>(side + fingerName + partName));
                    if (!ReferenceEquals(boneTransform, null))
                    {
                        // fingerLocalCoords[index + 1] = boneTransform.position - boneTransform.parent.position;
                        fingerLocalCoords[index + 1] = boneTransform.localPosition;
                        ++index;
                    }
                }
            }
        }

        public void Solve(HandsData data)
        {
            if (data.LeftPosList is {Count: > 20})
            {
                data.LeftPosList = data.LeftPosList.Select((point, index) =>
                {
                    // point.Point = _leftHand.position + Vector3.Scale(point.Point - data.LeftPosList[0].Point, Vector3.one * 0.01f);
                    point.Point = _leftKalmanFilters[index].CorrectAndPredict(point.Point);
                    point.Point = _leftOneEuroFilters[index].Filter(point.Point);
                    // point.Point = _leftHand.TransformPoint(point.Point);
                    leftDrawer[index].position = point.Point;
                    return point;
                }).ToList();
                SetHandRotation(_leftHand, data.LeftPosList, false);
                FingerSolve(_leftHand, data.LeftPosList, false);
            }
            else
            {
                _leftHand.localRotation = Quaternion.identity;
            }

            if (data.RightPosList is {Count: > 20})
            {
                data.RightPosList = data.RightPosList.Select((point, index) =>
                {
                    // point.Point = _rightHand.position + Vector3.Scale(point.Point - data.RightPosList[0].Point, Vector3.one * 0.01f);
                    point.Point = _rightKalmanFilters[index].CorrectAndPredict(point.Point);
                    point.Point = _rightOneEuroFilters[index].Filter(point.Point);
                    // point.Point = _rightHand.TransformPoint(point.Point);
                    rightDrawer[index].position = point.Point;
                    return point;
                }).ToList();
                SetHandRotation(_rightHand, data.RightPosList, true);
                FingerSolve(_rightHand, data.RightPosList, true);
            }
            else
            {
                _rightHand.localRotation = Quaternion.identity;
            }
        }

        private void FingerSolve(Transform hand, List<PointData> points, bool rightHand)
        {
            if (_rightFingerLocalCoords == null || _leftFingerLocalCoords == null)
            {
                return;
            }

            // points = points.Select(point => point.ApplyRotation(Quaternion.Euler(0, 180, 0))).ToList();

            var lowerArm = hand.parent;
            var upperArm = lowerArm.parent;

            
            var quaternion1 = upperArm.rotation;
            var quaternion2 = Quaternion.Inverse(lowerArm.localRotation) * quaternion1;
            var quaternion12 = Quaternion.Inverse(hand.localRotation) * quaternion2;
            var localCoords = rightHand ? _rightFingerLocalCoords : _leftFingerLocalCoords;
            var str = rightHand ? Right : Left;
            for (var i = 0; i < FingerNames.Length; ++i)
            {
                var quaternion13 = quaternion12;
                for(var j = 0; j < PartNames.Length - 1; ++j)
                {
                    var index3 = i * 4 + 1 + j;
                    var normalized14 = localCoords[index3 + 1].normalized; // 14
                    var quaternion14 = quaternion13;
                    var vector3_1 = points[index3 + 1].Point - points[index3].Point; // 3_1
                    var normalized15 = vector3_1.normalized;
                    var vector3_3 = quaternion14 * normalized15; // 3_3
                    var vector3_2 = Vector3.Cross(localCoords[index3 + 1], Vector3.down);
                    var normalized16 = vector3_2.normalized; // 16
                    var vector3_4 = Vector3.ProjectOnPlane(vector3_3, normalized16);
                    var normalized17 = vector3_4.normalized; // 17
                    var rotation4 = Quaternion.FromToRotation(normalized14, normalized17); // rotation4
                    var quaternion15 = Quaternion.FromToRotation(rotation4 * normalized14, vector3_3) * rotation4; // quaternion15
                    _animator.GetBoneTransform(Enum.Parse<HumanBodyBones>(str + FingerNames[i] + PartNames[j])).localRotation = quaternion15;
                    quaternion13 = Quaternion.Inverse(quaternion15) * quaternion13;
                }
            }
        }

        private void SetHandRotation(Transform hand, IReadOnlyList<PointData> points, bool rightHand)
        {
            var palm0 = points[0].Point;
            var palm1 = points[5].Point;
            var palm2 = points[17].Point;
            var palm3 = points[9].Point;
            
            _handPlane.Set3Points(palm0, palm1, palm2);
            var rightDir = palm3 - palm0;
            var upDir = rightHand ? _handPlane.normal : _handPlane.flipped.normal;
            var forwardDir = Vector3.Cross(rightDir, upDir);
            var handMatrix = new Matrix4x4(rightHand ? rightDir : -rightDir, upDir, rightHand ? forwardDir : -forwardDir, RightColumn);
            
            // lower arm
            var dir = hand.localPosition;
            var lowerArm = hand.transform.parent;
            var angle = Vector3.Angle(lowerArm.up, handMatrix.rotation * Vector3.up);
            var lowerArmRot = Quaternion.AngleAxis(rightHand ? -angle : angle, dir);
            var rotation = lowerArm.rotation;
            lowerArmRot = rotation * lowerArmRot;
            // rotation = Quaternion.Slerp(rotation, lowerArmRot, Smooth * Time.deltaTime);
            lowerArm.rotation = lowerArmRot;
            hand.rotation = handMatrix.rotation;
        }

        private const float Smooth = 10f;
    }
}
