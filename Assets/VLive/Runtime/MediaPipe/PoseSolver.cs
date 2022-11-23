using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VLive.Runtime.Utilities;
namespace VLive.Runtime.MediaPipe
{

    [RequireComponent(typeof(Animator))]
    public class PoseSolver : MonoBehaviour, IPoseSolver
    {
        private Animator _animator;
        private readonly Dictionary<int, JointPoint> _jointPointDict = new();
        private readonly List<JointKalmanFilter> _kalmanFilters = new();
        private readonly List<OneEuroFilter<Vector3>> _oneEuroFilters = new();
        
        private const double KalmanTimeInterval = 0.45;
        private const double KalmanNoise = 0.4;
        private const float MinCutOffValue = 1.5f;
        private const float DCutOffValue = 1f;
        private const float Beta = 0.1f;
        private const float Frequency = 90f;
        
        private Vector3 _leftUpperForward;
        private Vector3 _leftLowerForward;
        private Vector3 _rightUpperForward;
        private Vector3 _rightLowerForward;
        private const string LeftString = "Left";

        private Plane _headPlane;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            InitJointPointDict();
            SetHierarchy();
            SetRotationInfo();
        }

        private void InitJointPointDict()
        {
            foreach (JointIndex jointIndex in Enum.GetValues(typeof(JointIndex)))
            {
                var id = jointIndex.Int();
                var jointPoint = new JointPoint();
                var humanBodyBones = jointIndex.ToHumanBodyBones();
                if (humanBodyBones != HumanBodyBones.LastBone)
                {
                    jointPoint.Transform = _animator.GetBoneTransform(humanBodyBones);
                }

                // if (jointIndex == JointIndex.Nose)
                // {
                //     jointPoint.Transform = nose;
                // }

                _jointPointDict.Add(id, jointPoint);
                _kalmanFilters.Add(new JointKalmanFilter(KalmanTimeInterval, KalmanNoise));
                _oneEuroFilters.Add(new OneEuroFilter<Vector3>(Frequency, MinCutOffValue, Beta, DCutOffValue));
            }
        }
        
        private void SetHierarchy()
        {
            // Arms
            _jointPointDict[JointIndex.RightUpperArm.Int()].Child = _jointPointDict[JointIndex.RightLowerArm.Int()];
            _jointPointDict[JointIndex.RightLowerArm.Int()].Child = _jointPointDict[JointIndex.RightHand.Int()];
            _jointPointDict[JointIndex.RightShoulder.Int()].Child = _jointPointDict[JointIndex.RightUpperArm.Int()];
            _jointPointDict[JointIndex.LeftUpperArm.Int()].Child = _jointPointDict[JointIndex.LeftLowerArm.Int()];
            _jointPointDict[JointIndex.LeftLowerArm.Int()].Child = _jointPointDict[JointIndex.LeftHand.Int()];
            _jointPointDict[JointIndex.LeftShoulder.Int()].Child = _jointPointDict[JointIndex.LeftUpperArm.Int()];

            // Legs
            _jointPointDict[JointIndex.RightThigh.Int()].Child = _jointPointDict[JointIndex.RightShin.Int()];
            _jointPointDict[JointIndex.RightShin.Int()].Child = _jointPointDict[JointIndex.RightFoot.Int()];
            _jointPointDict[JointIndex.RightFoot.Int()].Child = _jointPointDict[JointIndex.RightToe.Int()];
            _jointPointDict[JointIndex.RightFoot.Int()].Parent = _jointPointDict[JointIndex.RightShin.Int()];
            _jointPointDict[JointIndex.LeftThigh.Int()].Child = _jointPointDict[JointIndex.LeftShin.Int()];
            _jointPointDict[JointIndex.LeftShin.Int()].Child = _jointPointDict[JointIndex.LeftFoot.Int()];
            _jointPointDict[JointIndex.LeftFoot.Int()].Child = _jointPointDict[JointIndex.LeftToe.Int()];
            _jointPointDict[JointIndex.LeftFoot.Int()].Parent = _jointPointDict[JointIndex.LeftShin.Int()];

            // Chest
            _jointPointDict[JointIndex.Spine.Int()].Child = _jointPointDict[JointIndex.Chest.Int()];
            _jointPointDict[JointIndex.Chest.Int()].Child = _jointPointDict[JointIndex.Neck.Int()];

            // head
            _jointPointDict[JointIndex.Neck.Int()].Child = _jointPointDict[JointIndex.Head.Int()];
        }

        private void SetRotationInfo()
        {
            var forward = TriangleNormal(_jointPointDict[JointIndex.Hip.Int()].Transform.position,
                _jointPointDict[JointIndex.LeftThigh.Int()].Transform.position,
                _jointPointDict[JointIndex.RightThigh.Int()].Transform.position);

            var _1 = _jointPointDict[JointIndex.Neck.Int()].Transform.position -
                     _jointPointDict[JointIndex.AbdomenUpper.Int()].Transform.position;

            foreach (var jointPoint in _jointPointDict.Values.Where(jointPoint => jointPoint.Transform != null))
            {
                jointPoint.InitRotation = jointPoint.Transform.rotation;
                jointPoint.InitLocalRotation = jointPoint.Transform.localRotation;
                var currPos = jointPoint.Transform.position;

                if (jointPoint.Parent != null && jointPoint.Parent.Transform != null &&
                    jointPoint.Child.Transform != null)
                {
                    var forward1 = jointPoint.Parent.Transform.position - currPos;
                    jointPoint.Inverse = GetInverse(currPos,
                        jointPoint.Child.Transform.position, forward1);
                    jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
                }
                else if (jointPoint.Child != null && jointPoint.Child.Transform != null)
                {
                    jointPoint.Inverse = GetInverse(currPos,
                        jointPoint.Child.Transform.position, forward);
                    jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
                }
            }

            var hip = _jointPointDict[JointIndex.Hip.Int()];
            hip.Inverse = Quaternion.Inverse(Quaternion.LookRotation(-forward));
            hip.InverseRotation = hip.Inverse * hip.InitRotation;

            var head = _jointPointDict[JointIndex.Head.Int()];
            var headPosition = head.Transform.position;

            var rightHand = _jointPointDict[JointIndex.RightHand.Int()];
            var leftHand = _jointPointDict[JointIndex.LeftHand.Int()];

            var rightHandPosition = rightHand.Transform.position;
            var leftHandPosition = leftHand.Transform.position;
            var rightHandUpWards = TriangleNormal(rightHandPosition,
                _jointPointDict[JointIndex.RightMid1.Int()].Transform.position,
                _jointPointDict[JointIndex.RightThumb2.Int()].Transform.position);
            var leftHandUpWards = TriangleNormal(leftHandPosition,
                _jointPointDict[JointIndex.LeftMid1.Int()].Transform.position,
                _jointPointDict[JointIndex.LeftThumb2.Int()].Transform.position);

            rightHand.InitRotation = rightHand.Transform.rotation;
            rightHand.Inverse = Quaternion.Inverse(Quaternion.LookRotation(
                rightHandPosition - _jointPointDict[JointIndex.RightMid1.Int()].Transform.position, rightHandUpWards));
            rightHand.InverseRotation = rightHand.Inverse * rightHand.InitRotation;

            leftHand.InitRotation = leftHand.Transform.rotation;
            leftHand.Inverse = Quaternion.Inverse(Quaternion.LookRotation(
                leftHandPosition - _jointPointDict[JointIndex.LeftMid1.Int()].Transform.position, leftHandUpWards));
            leftHand.InverseRotation = leftHand.Inverse * leftHand.InitRotation;


            // arm recalculate
            var rightLowerArm = _jointPointDict[JointIndex.RightLowerArm.Int()];
            var rightUpperArm = _jointPointDict[JointIndex.RightUpperArm.Int()];
            var leftLowerArm = _jointPointDict[JointIndex.LeftLowerArm.Int()];
            var leftUpperArm = _jointPointDict[JointIndex.LeftUpperArm.Int()];

            rightLowerArm.InitRotation = rightLowerArm.Transform.rotation;
            rightLowerArm.Inverse =
                Quaternion.Inverse(Quaternion.LookRotation(rightLowerArm.Transform.position - rightHandPosition,
                    rightHandUpWards));
            rightLowerArm.InverseRotation = rightLowerArm.Inverse * rightLowerArm.InitRotation;

            var rightArmUpWards = TriangleNormal(rightUpperArm.Transform.position,
                rightLowerArm.Transform.position,
                _jointPointDict[JointIndex.AbdomenUpper.Int()].Transform.position);
            rightUpperArm.InitRotation = rightUpperArm.Transform.rotation;
            rightUpperArm.Inverse =
                Quaternion.Inverse(Quaternion.LookRotation(rightUpperArm.Transform.position - rightHandPosition,
                    rightArmUpWards));
            rightUpperArm.InverseRotation = rightUpperArm.Inverse * rightLowerArm.InitRotation;

            leftLowerArm.InitRotation = leftLowerArm.Transform.rotation;
            leftLowerArm.Inverse =
                Quaternion.Inverse(Quaternion.LookRotation(leftLowerArm.Transform.position - leftHandPosition,
                    leftHandUpWards));
            leftLowerArm.InverseRotation = leftLowerArm.Inverse * leftLowerArm.InitRotation;

            var leftArmUpWards = TriangleNormal(leftUpperArm.Transform.position, leftLowerArm.Transform.position,
                _jointPointDict[JointIndex.AbdomenUpper.Int()].Transform.position);
            leftUpperArm.InitRotation = leftUpperArm.Transform.rotation;
            leftUpperArm.Inverse =
                Quaternion.Inverse(Quaternion.LookRotation(leftUpperArm.Transform.position - leftHandPosition,
                    leftArmUpWards));
            leftUpperArm.InverseRotation = leftUpperArm.Inverse * leftUpperArm.InitRotation;
        }

        private Vector3 TriangleNormalNow(JointIndex index1, JointIndex index2, JointIndex index3) =>
            TriangleNormalNow(index1.Int(), index2.Int(), index3.Int());

        private Vector3 TriangleNormalPos(JointIndex index1, JointIndex index2, JointIndex index3) =>
            TriangleNormalPos(index1.Int(), index2.Int(), index3.Int());

        private Vector3 TriangleNormalPos(int id1, int id2, int id3) => TriangleNormal(
            _jointPointDict[id1].Pos3D, _jointPointDict[id2].Pos3D,
            _jointPointDict[id3].Pos3D);

        private Vector3 TriangleNormalNow(int id1, int id2, int id3) => TriangleNormal(
            _jointPointDict[id1].Now3D, _jointPointDict[id2].Now3D,
            _jointPointDict[id3].Now3D);

        private Vector3 Dir(JointIndex index1, JointIndex index2) => Dir(index1.Int(), index2.Int());

        private Vector3 Dir(int id1, int id2)
        {
            return _jointPointDict[id1].Pos3D - _jointPointDict[id2].Pos3D;
        }

        private void LookAt(JointIndex index, JointIndex childIndex, Vector3 upWords) =>
            LookAt(index.Int(), childIndex.Int(), upWords);

        private void LookAt(int id, int childId, Vector3 upWords)
        {
            var curr = _jointPointDict[id];
            if (ReferenceEquals(curr.Transform, null))
                return;
            var child = _jointPointDict[childId];
            if (!child.Enabled)
            {
                // curr.Transform.rotation = curr.InitRotation;
                return;
            }

            curr.Transform.rotation = Quaternion.LookRotation(curr.Pos3D - child.Pos3D, upWords) * curr.InverseRotation;
        }
        
        public static Quaternion GetInverse(Vector3 p1, Vector3 p2, Vector3 forward)
        {
            return Quaternion.Inverse(Quaternion.LookRotation(p1 - p2, forward));
        }
        
        public static Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            var d1 = a - b;
            var d2 = a - c;

            var dd = Vector3.Cross(d1, d2);
            dd.Normalize();

            return dd;
        }

        public void Solve(PoseData data)
        {
            if (data.NewPosList is { Count: < 24 })
            {
                return;
            }
            
            var forward = TriangleNormal(data.HipPoint, data.NewPosList[23].Point,
                                              data.NewPosList[24].Point);
            SetArmJoints(data);
            SetLegJoints(data);
            SolveHead(data);
            SetBodyJoints(data);
            UpdateKalmanFilter();
            UpdateOneEuroFilter();
            UpdateJoints(data, forward);
        }

        private void UpdateKalmanFilter()
        {
            for (var i = 0; i < _jointPointDict.Values.Count; i++)
            {
                var jointPoint = _jointPointDict.Values.ElementAt(i);
                jointPoint.Now3D = _kalmanFilters[i].CorrectAndPredict(jointPoint);
            }
        }

        private void UpdateOneEuroFilter()
        {
            var index = 0;
            foreach (var joint in _jointPointDict.Values)
            {
                joint.Pos3D = _oneEuroFilters[index].Filter(joint.Now3D);
                index++;
            }
        }
        
        private void UpdateJoints(PoseData poseData, Vector3 forward)
        {
            var hip = _jointPointDict[JointIndex.Hip.Int()];
            var forwardUpperVec =
                TriangleNormalPos(JointIndex.AbdomenUpper, JointIndex.RightUpperArm, JointIndex.LeftUpperArm);
            var forwardLowerVec =
                TriangleNormalPos(JointIndex.AbdomenUpper, JointIndex.LeftThigh, JointIndex.RightThigh);

            var upperVec = Dir(JointIndex.Neck, JointIndex.AbdomenUpper);
            var rightUpperVec = Vector3.Cross(upperVec, forwardUpperVec);
            var downVec = Dir(JointIndex.Crotch, JointIndex.AbdomenUpper);
            var rightLowerVec = Vector3.Cross(forwardLowerVec, downVec);
            var rightAngle = Vector3.Angle(rightUpperVec, rightLowerVec);
            var bodyAngle = Vector3.Angle(upperVec, downVec);
            // hip.Transform.rotation = Quaternion.LookRotation(forward, -downVec) * hip.InverseRotation;

            // rightAngle < 100.0f

            var chest = _jointPointDict[JointIndex.Chest.Int()];
            if (chest.Enabled)
            {
                LookAt(JointIndex.Spine, JointIndex.Chest, forwardUpperVec);
                LookAt(JointIndex.Chest, JointIndex.Neck, forwardUpperVec);
            }

            UpdateArmJoints(JointIndex.LeftUpperArm, JointIndex.LeftLowerArm, JointIndex.LeftHand,
                            JointIndex.LeftShoulder, JointIndex.LeftThumb2, JointIndex.LeftMid1, forwardUpperVec,
                            ref _leftUpperForward, ref _leftLowerForward);
            UpdateArmJoints(JointIndex.RightUpperArm, JointIndex.RightLowerArm, JointIndex.RightHand,
                            JointIndex.RightShoulder, JointIndex.RightThumb2, JointIndex.RightMid1, forwardUpperVec,
                            ref _rightUpperForward, ref _rightLowerForward);
            

            UpdateLegJoints(JointIndex.LeftThigh, JointIndex.LeftShin, JointIndex.LeftFoot, JointIndex.LeftToe,
                rightLowerVec, forwardLowerVec);
            UpdateLegJoints(JointIndex.RightThigh, JointIndex.RightShin, JointIndex.RightFoot, JointIndex.RightToe,
                rightLowerVec, forwardLowerVec);
            
            UpdateHeadJoints();
        }

        private void UpdateHeadJoints()
        {
            var lEar = _jointPointDict[JointIndex.LeftEar.Int()];
            var rEar = _jointPointDict[JointIndex.RightEar.Int()];
            var nose = _jointPointDict[JointIndex.Nose.Int()];
            var head = _jointPointDict[JointIndex.Head.Int()];
            _headPlane.Set3Points(nose.Pos3D, rEar.Pos3D, lEar.Pos3D);
            var forward = _headPlane.normal;
            var right = lEar.Pos3D - rEar.Pos3D;
            var up = Vector3.Cross(forward, right);

            var position = head.Transform.position;
            Debug.DrawRay(position, forward, Color.blue, 0.5f);
            Debug.DrawRay(position, up, Color.green, 0.5f);
            Debug.DrawRay(position, right, Color.red, 0.5f);
            
        }
        
        private void UpdateArmJoints(JointIndex upper, JointIndex lower, JointIndex hand, JointIndex thumb,
            JointIndex mid, JointIndex shoulder, Vector3 forward, ref Vector3 upperForward, ref Vector3 lowerForward)
        {
            if (_jointPointDict[hand.Int()].Enabled)
            {
                // var isLeft = upper.ToString().StartsWith(LeftString);
                if (_jointPointDict[shoulder.Int()].Enabled)
                {
                    LookAt(shoulder, upper, forward);
                }

                var normal = TriangleNormalPos(hand, mid, thumb);
                if (GetVectorAngle(upper, lower, JointIndex.AbdomenUpper) > 5.0f)
                {
                    upperForward = TriangleNormalPos(upper, lower, JointIndex.AbdomenUpper);
                }

                LookAt(upper, lower, upperForward);
                var vectorAngle = GetVectorAngle(lower, hand, upper);
                if (vectorAngle > 5.0f)
                {
                    lowerForward = TriangleNormalPos(lower, hand, upper);
                }

                switch (vectorAngle)
                {
                    case < 20.0f:
                        LookAt(lower, hand, normal);
                        break;
                    case < 90.0f:
                    {
                        var ratio = (vectorAngle - 20.0f) / 70.0f;
                        LookAt(lower, hand, normal * (1f - ratio) + lowerForward * ratio);
                        break;
                    }
                    default:
                    {
                        // if (isLeft)
                        // {
                        //     LookAt(lower, hand, lowerForward);
                        // }
                        // else
                        // {
                        //     LookAt(lower, hand, normal * 0.5f + lowerForward * 0.5f);
                        // }
                        LookAt(lower, hand, lowerForward);
                        break;
                    }
                }

                // LookAt(hand, mid, normal);
            }
            // else
            // {
            //     LookAt(upper, lower, forward);
            //     LookAt(lower, hand, forward);
            // }
        }

        private void UpdateLegJoints(JointIndex thigh, JointIndex shin, JointIndex foot, JointIndex toe,
            Vector3 rightLowerVec, Vector3 forwardLowerVec)
        {
            var thighPos = _jointPointDict[thigh.Int()].Pos3D;
            var shinPos = _jointPointDict[shin.Int()].Pos3D;
            var num1 = -(rightLowerVec.x * thighPos.x +
                         rightLowerVec.y * thighPos.y +
                         rightLowerVec.z * thighPos.z);
            var num2 =
                (-(rightLowerVec.x * shinPos.x +
                   rightLowerVec.y * shinPos.y +
                   rightLowerVec.z * shinPos.z + num1) /
                 (rightLowerVec.x * rightLowerVec.x +
                  rightLowerVec.y * rightLowerVec.y +
                  rightLowerVec.z * rightLowerVec.z));
            var upWords = Vector3.Cross(shinPos + num2 * rightLowerVec - thighPos, rightLowerVec);
            LookAt(thigh, shin, upWords);

            var vectorAngle = GetVectorAngle(shin, foot, thigh);
            switch (vectorAngle)
            {
                case < 20.0f:
                    LookAt(shin, foot, forwardLowerVec);
                    break;
                case >= 20.0f and < 40.0f:
                {
                    var ratio = (vectorAngle - 20.0f) / 20.0f;
                    var upWords2 = forwardLowerVec * (1f - ratio) + (Dir(shin, foot) + Dir(shin, thigh)) * ratio;
                    LookAt(shin, foot, upWords2);
                    break;
                }
                // default:
                //     LookAt(shin, foot, Dir(shin, foot) + Dir(shin, thigh));
                //     break;
            }

            // LookAt(foot, toe, Dir(shin, foot));
        }

        private float GetVectorAngle(JointIndex index1, JointIndex index2, JointIndex index3) =>
            Vector3.Angle(Dir(index1, index2), Dir(index3, index1));
        
        private void SetArmJoints(PoseData poseData)
        {
            _jointPointDict[JointIndex.LeftUpperArm.Int()].SetNow3D(poseData.NewPosList[11]);
            _jointPointDict[JointIndex.LeftLowerArm.Int()].SetNow3D(poseData.NewPosList[13]);
            _jointPointDict[JointIndex.LeftHand.Int()].SetNow3D(poseData.NewPosList[15]);
            _jointPointDict[JointIndex.LeftThumb2.Int()].SetNow3D(poseData.NewPosList[21]);
            _jointPointDict[JointIndex.LeftMid1.Int()].SetNow3D(poseData.NewPosList[19]);


            _jointPointDict[JointIndex.RightUpperArm.Int()].SetNow3D(poseData.NewPosList[12]);
            _jointPointDict[JointIndex.RightLowerArm.Int()].SetNow3D(poseData.NewPosList[14]);
            _jointPointDict[JointIndex.RightHand.Int()].SetNow3D(poseData.NewPosList[16]);
            _jointPointDict[JointIndex.RightThumb2.Int()].SetNow3D(poseData.NewPosList[22]);
            _jointPointDict[JointIndex.RightMid1.Int()].SetNow3D(poseData.NewPosList[20]);
        }

        private void SetLegJoints(PoseData poseData)
        {
            _jointPointDict[JointIndex.LeftThigh.Int()].SetNow3D(poseData.NewPosList[23]);
            _jointPointDict[JointIndex.LeftShin.Int()].SetNow3D(poseData.NewPosList[25]);
            _jointPointDict[JointIndex.LeftFoot.Int()].SetNow3D(poseData.NewPosList[27]);
            _jointPointDict[JointIndex.LeftToe.Int()].SetNow3D(poseData.NewPosList[31]);
            _jointPointDict[JointIndex.RightThigh.Int()].SetNow3D(poseData.NewPosList[24]);
            _jointPointDict[JointIndex.RightShin.Int()].SetNow3D(poseData.NewPosList[26]);
            _jointPointDict[JointIndex.RightFoot.Int()].SetNow3D(poseData.NewPosList[28]);
            _jointPointDict[JointIndex.RightToe.Int()].SetNow3D(poseData.NewPosList[32]);
        }

        private void SetBodyJoints(PoseData poseData)
        {
            // hip = 14
            // neck = 15
            // spine = 16
            // head = 17

            var abdomenUpper = _jointPointDict[JointIndex.AbdomenUpper.Int()];
            abdomenUpper.Now3D = poseData.HipPoint;
            var hip = _jointPointDict[JointIndex.Hip.Int()];
            var crotch = _jointPointDict[JointIndex.Crotch.Int()];
            crotch.Now3D = (_jointPointDict[JointIndex.RightThigh.Int()].Now3D +
                            _jointPointDict[JointIndex.LeftThigh.Int()].Now3D) * 0.5f;
            crotch.Enabled = true;
            hip.Now3D = (abdomenUpper.Now3D + crotch.Now3D) * 0.5f;
            
            hip.Enabled = true;
            var forward = TriangleNormalNow(JointIndex.Hip, JointIndex.LeftThigh, JointIndex.RightThigh);
            var hipToSpine = abdomenUpper.Now3D - crotch.Now3D;

            // TODO : Shoulder?
            _jointPointDict[JointIndex.RightShoulder.Int()].Now3D = (_jointPointDict[JointIndex.Neck.Int()].Now3D +
                                                                     _jointPointDict[JointIndex.RightUpperArm.Int()]
                                                                         .Now3D) * 0.5f;
            _jointPointDict[JointIndex.RightShoulder.Int()].Enabled = false;
            _jointPointDict[JointIndex.LeftShoulder.Int()].Now3D = (_jointPointDict[JointIndex.Neck.Int()].Now3D +
                                                                    _jointPointDict[JointIndex.LeftUpperArm.Int()]
                                                                        .Now3D) * 0.5f;
            _jointPointDict[JointIndex.LeftShoulder.Int()].Enabled = false;


            _jointPointDict[JointIndex.Spine.Int()].Now3D = _jointPointDict[JointIndex.AbdomenUpper.Int()].Now3D;
            _jointPointDict[JointIndex.Spine.Int()].Enabled = true;
            var spineToNeck = _jointPointDict[JointIndex.Neck.Int()].Now3D -
                              _jointPointDict[JointIndex.AbdomenUpper.Int()].Now3D;

            var chestDir = hipToSpine.normalized + spineToNeck.normalized * 2f;
            var chestDirNorm = chestDir.normalized;
            chestDir = spineToNeck * 0.5f;
            var chestDirDis = chestDir.magnitude;
            _jointPointDict[JointIndex.Chest.Int()].Now3D =
                _jointPointDict[JointIndex.AbdomenUpper.Int()].Now3D + chestDirNorm * chestDirDis;
            _jointPointDict[JointIndex.Chest.Int()].Enabled = true;
        }
        
        private void SolveHead(PoseData poseData)
        {
            _jointPointDict[JointIndex.LeftEar.Int()].SetNow3D(poseData.NewPosList[1]);
            _jointPointDict[JointIndex.RightEar.Int()].SetNow3D(poseData.NewPosList[4]);
            _jointPointDict[JointIndex.Nose.Int()].SetNow3D(poseData.NewPosList[0]);
            // _jointPointDict[JointIndex.Nose.Int()].Now3D.y = _jointPointDict[JointIndex.LeftEar.Int()].Now3D.y;
            _jointPointDict[JointIndex.Neck.Int()].Now3D = (_jointPointDict[JointIndex.RightUpperArm.Int()].Now3D +
                                                            _jointPointDict[JointIndex.LeftUpperArm.Int()].Now3D) *
                                                           0.5f;
            _jointPointDict[JointIndex.Neck.Int()].Enabled = true;
            _jointPointDict[JointIndex.Head.Int()].Now3D = (poseData.NewPosList[7].Point +
                                                            poseData.NewPosList[8].Point) * 0.5f;
            _jointPointDict[JointIndex.Head.Int()].Enabled = true;
        }
    }
}
