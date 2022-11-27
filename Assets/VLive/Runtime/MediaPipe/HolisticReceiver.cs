using System;
using Sirenix.OdinInspector;
using UniGLTF;
using UnityEngine;
namespace VLive.Runtime.MediaPipe
{
    public class HolisticReceiver : MonoBehaviour
    {
        private IPoseSolver _poseSolver;
        private IFaceSolver _faceSolver;
        private IHandsSolver _handsSolver;
        private GameObject _currModel;

        public void SetModel(GameObject model)
        {
            if (_currModel != null)
            {
                Destroy(_currModel);
            }
            _poseSolver = model.GetOrAddComponent<PoseSolver>();
            _faceSolver = model.GetOrAddComponent<FaceSolver>();
            _handsSolver = model.GetOrAddComponent<HandsSolver>();
            _currModel = model;
        }

        public void SolvePose(PoseData poseData)
        {
            _poseSolver?.Solve(poseData);
        }

        public void SolveFace(FaceData faceData)
        {
            _faceSolver?.Solve(faceData);
        }

        public void SolveHand(HandsData handsData)
        {
            _handsSolver?.Solve(handsData);
        }
    }
}
