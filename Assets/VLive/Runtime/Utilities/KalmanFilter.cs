using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VLive.Runtime.Extensions;
namespace VLive.Runtime.Utilities
{
    
    public class ConstantVelocity3DModel : ICloneable
    {
        public const int Dimension = 6;
        public Vector3 Position;
        public Vector3 Velocity;

        public ConstantVelocity3DModel()
        {
            Position = Vector3.zero;
            Velocity = Vector3.zero;
        }

        public static double[,] GetTransitionMatrix(double timeInterval = 1.0)
        {
            return new[,]
            {
                {
                    1.0,
                    timeInterval,
                    0.0,
                    0.0,
                    0.0,
                    0.0
                },
                {
                    0.0,
                    1.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0
                },
                {
                    0.0,
                    0.0,
                    1.0,
                    timeInterval,
                    0.0,
                    0.0
                },
                {
                    0.0,
                    0.0,
                    0.0,
                    1.0,
                    0.0,
                    0.0
                },
                {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    1.0,
                    timeInterval
                },
                {
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    1.0
                }
            };
        }

        public static double[,] GetPositionMeasurementMatrix() => new[,]
        {
            {
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0
            },
            {
                0.0,
                0.0,
                1.0,
                0.0,
                0.0,
                0.0
            },
            {
                0.0,
                0.0,
                0.0,
                0.0,
                1.0,
                0.0
            }
        };

        public static double[,] GetProcessNoise(double accelerationNoise, double timeInterval = 1.0)
        {
            var numArray1 = new double[6, 3];
            numArray1[0, 0] = timeInterval * timeInterval / 2.0;
            numArray1[1, 0] = timeInterval;
            numArray1[2, 1] = timeInterval * timeInterval / 2.0;
            numArray1[3, 1] = timeInterval;
            numArray1[4, 2] = timeInterval * timeInterval / 2.0;
            numArray1[5, 2] = timeInterval;
            var b = MatrixFunctions.Diagonal(numArray1.ColumnCount(), accelerationNoise);
            return numArray1.Multiply(b).Multiply(numArray1.Transpose());
        }

        public static ConstantVelocity3DModel FromArray(double[] arr) => new()
        {
            Position = new Vector3((float)arr[0], (float)arr[2], (float)arr[4]),
            Velocity = new Vector3((float)arr[1], (float)arr[3], (float)arr[5])
        };

        public static double[] ToArray(ConstantVelocity3DModel modelState) => new double[]
        {
            modelState.Position.x,
            modelState.Velocity.x,
            modelState.Position.y,
            modelState.Velocity.y,
            modelState.Position.z,
            modelState.Velocity.z
        };

        public object Clone() => new ConstantVelocity3DModel()
        {
            Position = Position,
            Velocity = Velocity
        };
    }

    public abstract class BaseKalmanFilter<TState, TMeasurement>
    {
        public double[,] ResidualCovariance { get; protected set; }

        public double[,] ResidualCovarianceInv { get; protected set; }

        public double[,] KalmanGain { get; protected set; }

        public double[,] EstimateCovariance { get; protected set; }

        public double[,] TransitionMatrix { get; set; }

        public double[,] ControlMatrix { get; set; }

        public double[,] MeasurementMatrix { get; set; }

        public double[,] ProcessNoise { get; set; }

        public double[,] MeasurementNoise { get; set; }

        public int StateVectorDimension { get; private set; }

        public int MeasurementVectorDimension { get; private set; }

        public int ControlVectorDimension { get; private set; }

        public TState State => _stateConvertBackFunc(StateArray);

        private Func<TState, double[]> _stateConvertFunc;
        private Func<double[], TState> _stateConvertBackFunc;
        private Func<TMeasurement, double[]> _measurementConvertFunc;

        private const double GateThreshold = 9.21034037197618;

        protected double[] StateArray;

        protected BaseKalmanFilter(
            TState initialState,
            double[,] initialStateError,
            int measurementVectorDimension,
            int controlVectorDimension,
            Func<TState, double[]> stateConvertFunc,
            Func<double[], TState> stateConvertBackFunc,
            Func<TMeasurement, double[]> measurementConvertFunc
        )
        {
            var numArray = stateConvertFunc(initialState);
            StateVectorDimension = numArray.Length;
            MeasurementVectorDimension = measurementVectorDimension;
            ControlVectorDimension = controlVectorDimension;
            StateArray = numArray;
            EstimateCovariance = initialStateError;
            _stateConvertFunc = stateConvertFunc;
            _stateConvertBackFunc = stateConvertBackFunc;
            _measurementConvertFunc = measurementConvertFunc;
        }

        public void Predict() => Predict(null);

        public void Predict(double[] controlVector)
        {
            CheckPrerequisites();
            PredictInternal(controlVector);
        }

        public void Correct(TMeasurement measurement)
        {
            CheckPrerequisites();
            CorrectInternal(_measurementConvertFunc(measurement));
        }

        public double[] CalculateDelta(TMeasurement measurement)
        {
            CheckPrerequisites();
            return CalculateDelta(_measurementConvertFunc(measurement));
        }

        internal double[] CalculateDelta(double[] measurement)
        {
            var b = MeasurementMatrix.Multiply(StateArray);
            return measurement.Subtract(b);
        }

        protected abstract void CorrectInternal(double[] measurement);

        protected abstract void PredictInternal(double[] controlVector);

        protected void CheckPrerequisites()
        {
            if (TransitionMatrix == null)
                throw new Exception("Transition matrix cannot be null!");
            if (TransitionMatrix.RowCount() != StateVectorDimension ||
                TransitionMatrix.ColumnCount() != StateVectorDimension)
                throw new Exception("Transition matrix dimensions are not valid!");
            if (ControlMatrix == null && ControlVectorDimension != 0)
                throw new Exception("Control matrix can be null only if control vector dimension is set to 0!");
            if (ControlMatrix != null && (ControlMatrix.RowCount() != StateVectorDimension ||
                                               ControlMatrix.ColumnCount() != ControlVectorDimension))
                throw new Exception("Control matrix dimensions are not valid!");
            if (MeasurementMatrix == null)
                throw new Exception("Measurement matrix cannot be null!");
            if (MeasurementMatrix.RowCount() != MeasurementVectorDimension ||
                MeasurementMatrix.ColumnCount() != StateVectorDimension)
                throw new Exception("Measurement matrix dimesnions are not valid!");
            if (ProcessNoise == null)
                throw new Exception("Process noise covariance matrix cannot be null!");
            if (ProcessNoise.RowCount() != StateVectorDimension ||
                ProcessNoise.ColumnCount() != StateVectorDimension)
                throw new Exception("Process noise covariance matrix dimensions are not valid!");
            if (MeasurementNoise == null)
                throw new Exception("Measurement noise covariance matrix cannot be null!");
            if (MeasurementNoise.RowCount() != MeasurementVectorDimension ||
                MeasurementNoise.ColumnCount() != MeasurementVectorDimension)
                throw new Exception("Measurement noise covariance matrix dimensions are not valid!");
        }
    }

    public class DiscreteKalmanFilter<TState, TMeasurement> : BaseKalmanFilter<TState, TMeasurement>
    {
        public DiscreteKalmanFilter(
            TState initialState,
            double[,] initialStateError,
            int measurementVectorDimension,
            int controlVectorDimension,
            Func<TState, double[]> stateConvertFunc,
            Func<double[], TState> stateConvertBackFunc,
            Func<TMeasurement, double[]> measurementConvertFunc)
            : base(initialState, initialStateError, measurementVectorDimension, controlVectorDimension,
                stateConvertFunc, stateConvertBackFunc, measurementConvertFunc)
        {
        }

        protected override void PredictInternal(double[] controlVector)
        {
            StateArray = TransitionMatrix.Multiply(StateArray);
            if (controlVector != null)
                StateArray = StateArray.Add(ControlMatrix.Multiply(controlVector));
            EstimateCovariance =
                TransitionMatrix.Multiply(EstimateCovariance)
                    .Multiply(TransitionMatrix.Transpose()).Add(ProcessNoise);
            var b = MeasurementMatrix.Transpose();
            ResidualCovariance = MeasurementMatrix.Multiply(EstimateCovariance).Multiply(b).Add(MeasurementNoise);
            ResidualCovarianceInv = ResidualCovariance.Inverse();
            KalmanGain = EstimateCovariance.Multiply(b).Multiply(ResidualCovarianceInv);
        }

        protected override void CorrectInternal(double[] measurement) => Correct(CalculateDelta(measurement));

        private void Correct(double[] innovationVector)
        {
            if (innovationVector.Length != MeasurementVectorDimension)
                throw new Exception(
                    "PredicitionError error vector (innovation vector) must have the same length as measurement.");
            StateArray = StateArray.Add(KalmanGain.Multiply(innovationVector));
            EstimateCovariance = MatrixFunctions.Identity(StateVectorDimension)
                .Subtract(KalmanGain.Multiply(MeasurementMatrix))
                .Multiply(EstimateCovariance.Transpose());
        }
    }
}
