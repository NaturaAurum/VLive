using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace VLive.Runtime
{
    public class HolisticReceiver : MonoBehaviour
    {
        private IPoseSolver _poseSolver;
        private IFaceSolver _faceSolver;
        private IHandSolver _handSolver;

        [SerializeField]
        private GameObject[] models;

        private int _currIndex = 0;

        [Button]
        public void ChangeModel(int index)
        {
            if (index >= models.Length)
            {
                return;
            }
            
            models[_currIndex].SetActive(false);
            var model = models[index];
            model.SetActive(true);
            _poseSolver = model.GetComponent<IPoseSolver>();
            _faceSolver = model.GetComponent<IFaceSolver>();
            _handSolver = model.GetComponent<IHandSolver>();

            _currIndex = index;
        }

        private void Start()
        {
            foreach (var model in models)
            {
                model.SetActive(false);
            }
            ChangeModel(0);
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
            _handSolver?.Solve(handsData);
        }
    }
}
