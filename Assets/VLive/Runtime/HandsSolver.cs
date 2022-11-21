using System;
using System.Collections.Generic;
using UnityEngine;
namespace VLive.Runtime
{
    public class HandsSolver : MonoBehaviour, IHandsSolver
    {
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
        
        private Vector3[] _leftFingerLocalCoords;
        private Vector3[] _rightFingerLocalCoords;

        private Animator _animator;

        private Plane _handPlane = new();

        private Transform _leftHand;
        private Transform _rightHand;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            InitializeFingerLocalCoord(ref _leftFingerLocalCoords, "Left");
            InitializeFingerLocalCoord(ref _rightFingerLocalCoords, "Right");

            _leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
        }

        private void InitializeFingerLocalCoord(ref Vector3[] fingerLocalCoords, string side)
        {
            fingerLocalCoords = new Vector3[21];
            var index = 1;
            foreach (var fingerName in FingerNames)
            {
                foreach (var partName in PartNames)
                {
                    var boneTransform = _animator.GetBoneTransform(Enum.Parse<HumanBodyBones>(side + fingerName + partName));
                    if (!ReferenceEquals(boneTransform, null))
                    {
                        fingerLocalCoords[index] = boneTransform.localPosition;
                        ++index;
                    }
                }
            }
        }

        public void Solve(HandsData data)
        {
            if (data.LeftPosList is {Count: > 20})
            {
                SetHandRotation(_leftHand, data.LeftPosList, false);
            }
            else
            {
                _leftHand.localRotation = Quaternion.identity;
            }

            if (data.RightPosList is {Count: > 20})
            {
                SetHandRotation(_rightHand, data.RightPosList, true);
            }
            else
            {
                _rightHand.localRotation = Quaternion.identity;
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
            var handMatrix = new Matrix4x4(rightHand ? -rightDir : rightDir, upDir, rightHand ? -forwardDir : forwardDir, RightColumn);
            hand.localRotation = handMatrix.rotation;
        }
    }
}
