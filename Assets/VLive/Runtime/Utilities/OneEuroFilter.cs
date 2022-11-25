using System;
using UnityEngine;
namespace VLive.Runtime.Utilities
{
    /* 
     * OneEuroFilter.cs
     * Author: Dario Mazzanti (dario.mazzanti@iit.it), 2016
     * 
     * This Unity C# utility is based on the C++ implementation of the OneEuroFilter algorithm by Nicolas Roussel (http://www.lifl.fr/~casiez/1euro/OneEuroFilter.cc)
     * More info on the 1€ filter by Géry Casiez at http://www.lifl.fr/~casiez/1euro/
     *
     */

    internal class LowPassFilter
    {
        private float _y, _a, _s;
        private bool _initialized;

        public void SetAlpha(float alpha)
        {
            if (alpha is <= 0.0f or > 1.0f)
            {
                Debug.LogError("alpha should be in (0.0., 1.0]");
                return;
            }

            _a = alpha;
        }

        public LowPassFilter(float alpha, float initVal = 0.0f)
        {
            _y = _s = initVal;
            SetAlpha(alpha);
            _initialized = false;
        }

        private float Filter(float value)
        {
            float result;
            if (_initialized)
                result = _a * value + (1.0f - _a) * _s;
            else
            {
                result = value;
                _initialized = true;
            }

            _y = value;
            _s = result;
            return result;
        }

        public float FilterWithAlpha(float value, float alpha)
        {
            SetAlpha(alpha);
            return Filter(value);
        }

        public bool HasLastRawValue()
        {
            return _initialized;
        }

        public float LastRawValue()
        {
            return _y;
        }
    };

// -----------------------------------------------------------------

    public class OneEuroFilter
    {
        private float _freq;
        private float _minCutOff;
        private float _beta;
        private float _dCutOff;
        private readonly LowPassFilter _x;
        private readonly LowPassFilter _dx;
        private float _lastTime;

        // currValue contains the latest value which have been succesfully filtered
        // prevValue contains the previous filtered value
        public float CurrValue { get; protected set; }
        public float PrevValue { get; protected set; }

        private float Alpha(float cutoff)
        {
            var te = 1.0f / _freq;
            var tau = 1.0f / (2.0f * Mathf.PI * cutoff);
            return 1.0f / (1.0f + tau / te);
        }

        private void SetFrequency(float f)
        {
            if (f <= 0.0f)
            {
                Debug.LogError("freq should be > 0");
                return;
            }

            _freq = f;
        }

        private void SetMinCutoff(float mc)
        {
            if (mc <= 0.0f)
            {
                Debug.LogError("mincutoff should be > 0");
                return;
            }

            _minCutOff = mc;
        }

        private void SetBeta(float b)
        {
            _beta = b;
        }

        private void SetDerivateCutoff(float dc)
        {
            if (dc <= 0.0f)
            {
                Debug.LogError("dcutoff should be > 0");
                return;
            }

            _dCutOff = dc;
        }

        public OneEuroFilter(float freq, float mincutoff = 1.0f, float beta = 0.0f, float dcutoff = 1.0f)
        {
            SetFrequency(freq);
            SetMinCutoff(mincutoff);
            SetBeta(beta);
            SetDerivateCutoff(dcutoff);
            _x = new LowPassFilter(Alpha(this._minCutOff));
            _dx = new LowPassFilter(Alpha(this._dCutOff));
            _lastTime = -1.0f;

            CurrValue = 0.0f;
            PrevValue = CurrValue;
        }

        public void UpdateParams(float freq, float mincutoff = 1.0f, float beta = 0.0f, float dcutoff = 1.0f)
        {
            SetFrequency(freq);
            SetMinCutoff(mincutoff);
            SetBeta(beta);
            SetDerivateCutoff(dcutoff);
            _x.SetAlpha(Alpha(this._minCutOff));
            _dx.SetAlpha(Alpha(this._dCutOff));
        }

        public float Filter(float value, float timestamp = -1.0f)
        {
            PrevValue = CurrValue;

            // update the sampling frequency based on timestamps
            if (Math.Abs(_lastTime - (-1.0f)) > float.Epsilon && Math.Abs(timestamp - (-1.0f)) > float.Epsilon)
                _freq = 1.0f / (timestamp - _lastTime);
            _lastTime = timestamp;
            // estimate the current variation per second 
            var dvalue = _x.HasLastRawValue() ? (value - _x.LastRawValue()) * _freq : 0.0f; // FIXME: 0.0 or value? 
            var edvalue = _dx.FilterWithAlpha(dvalue, Alpha(_dCutOff));
            // use it to update the cutoff frequency
            var cutoff = _minCutOff + _beta * Mathf.Abs(edvalue);
            // filter the given value
            CurrValue = _x.FilterWithAlpha(value, Alpha(cutoff));

            return CurrValue;
        }
    }

    public class Vector3OneEuroFilter
    {
        private readonly OneEuroFilter[] _oneEuroFilters;

        // filter parameters
        public float Freq
        {
            get;
        }
        public float MinCutOff
        {
            get;
        }
        public float Beta
        {
            get;
        }
        public float DCutOff
        {
            get;
        }

        // currValue contains the latest value which have been succesfully filtered
        // prevValue contains the previous filtered value
        public Vector3 CurrValue
        {
            get;
            private set;
        }
        public Vector3 PrevValue
        {
            get;
            private set;
        }
        
        public Vector3OneEuroFilter(float freq, float minCutOff = 1.0f, float beta = 0.0f, float dCutOff = 1.0f)
        {
            CurrValue = Vector3.zero;
            PrevValue = Vector3.zero;
            Freq = freq;
            MinCutOff = minCutOff;
            Beta = beta;
            DCutOff = dCutOff;
            _oneEuroFilters = new OneEuroFilter[3];
            for (var i = 0; i < _oneEuroFilters.Length; i++)
                _oneEuroFilters[i] = new OneEuroFilter(Freq, MinCutOff, Beta, DCutOff);
        }
        
        public Vector3 Filter(Vector3 value, float timestamp = -1.0f)
        {
            PrevValue = CurrValue;
            var output = Vector3.zero;
            var input = value;

            for (var i = 0; i < _oneEuroFilters.Length; i++)
                output[i] = _oneEuroFilters[i].Filter(input[i], timestamp);

            CurrValue = output;
            return CurrValue;
        }
    }


    // this class instantiates an array of OneEuroFilter objects to filter each component of Vector2, Vector3, Vector4 or Quaternion types
    public class OneEuroFilter<T> where T : struct
    {
        // containst the type of T
        private readonly Type _type;

        // the array of filters
        private readonly OneEuroFilter[] _oneEuroFilters;

        // filter parameters
        public float Freq { get; protected set; }
        public float MinCutOff { get; protected set; }
        public float Beta { get; protected set; }
        public float DCutOff { get; protected set; }

        // currValue contains the latest value which have been succesfully filtered
        // prevValue contains the previous filtered value
        public T CurrValue { get; protected set; }
        public T PrevValue { get; protected set; }

        // initialization of our filter(s)
        public OneEuroFilter(float freq, float minCutOff = 1.0f, float beta = 0.0f, float dCutOff = 1.0f)
        {
            _type = typeof(T);
            CurrValue = new T();
            PrevValue = new T();

            Freq = freq;
            MinCutOff = minCutOff;
            Beta = beta;
            DCutOff = dCutOff;

            if (_type == typeof(Vector2))
                _oneEuroFilters = new OneEuroFilter[2];

            else if (_type == typeof(Vector3))
                _oneEuroFilters = new OneEuroFilter[3];

            else if (_type == typeof(Vector4) || _type == typeof(Quaternion))
                _oneEuroFilters = new OneEuroFilter[4];
            else
            {
                Debug.LogError(_type + " is not a supported type");
                return;
            }

            for (var i = 0; i < _oneEuroFilters.Length; i++)
                _oneEuroFilters[i] = new OneEuroFilter(Freq, MinCutOff, Beta, DCutOff);
        }

        // updates the filter parameters
        public void UpdateParams(float freq, float minCutOff = 1.0f, float beta = 0.0f, float dCutOff = 1.0f)
        {
            Freq = freq;
            MinCutOff = minCutOff;
            Beta = beta;
            DCutOff = dCutOff;

            foreach (var filter in _oneEuroFilters)
                filter.UpdateParams(Freq, MinCutOff, Beta, DCutOff);
        }


        // filters the provided _value and returns the result.
        // Note: a timestamp can also be provided - will override filter frequency.
        public T Filter<TU>(TU value, float timestamp = -1.0f) where TU : struct
        {
            PrevValue = CurrValue;

            // if (typeof(TU) != _type)
            // {
            //     Debug.LogError("WARNING! " + typeof(TU) + " when " + _type +
            //                    " is expected!\nReturning previous filtered value");
            //     CurrValue = PrevValue;
            //
            //     return (T)Convert.ChangeType(CurrValue, typeof(T));
            // }

            if (_type == typeof(Vector2))
            {
                var output = Vector2.zero;
                var input = (Vector2)Convert.ChangeType(value, typeof(Vector2));

                for (var i = 0; i < _oneEuroFilters.Length; i++)
                    output[i] = _oneEuroFilters[i].Filter(input[i], timestamp);

                CurrValue = (T)Convert.ChangeType(output, typeof(T));
            }

            else if (_type == typeof(Vector3))
            {
                var output = Vector3.zero;
                var input = (Vector3)Convert.ChangeType(value, typeof(Vector3));

                for (var i = 0; i < _oneEuroFilters.Length; i++)
                    output[i] = _oneEuroFilters[i].Filter(input[i], timestamp);

                CurrValue = (T)Convert.ChangeType(output, typeof(T));
            }

            else if (_type == typeof(Vector4))
            {
                var output = Vector4.zero;
                var input = (Vector4)Convert.ChangeType(value, typeof(Vector4));

                for (var i = 0; i < _oneEuroFilters.Length; i++)
                    output[i] = _oneEuroFilters[i].Filter(input[i], timestamp);

                CurrValue = (T)Convert.ChangeType(output, typeof(T));
            }

            else
            {
                var output = Quaternion.identity;
                var input = (Quaternion)Convert.ChangeType(value, typeof(Quaternion));

                // Workaround that take into account that some input device sends
                // quaternion that represent only a half of all possible values.
                // this piece of code does not affect normal behaviour (when the
                // input use the full range of possible values).
                if (Vector4.SqrMagnitude(new Vector4(_oneEuroFilters[0].CurrValue, _oneEuroFilters[1].CurrValue,
                                             _oneEuroFilters[2].CurrValue, _oneEuroFilters[3].CurrValue).normalized
                                         - new Vector4(input[0], input[1], input[2], input[3]).normalized) > 2)
                {
                    input = new Quaternion(-input.x, -input.y, -input.z, -input.w);
                }

                for (var i = 0; i < _oneEuroFilters.Length; i++)
                    output[i] = _oneEuroFilters[i].Filter(input[i], timestamp);

                CurrValue = (T)Convert.ChangeType(output, typeof(T));
            }

            return (T)Convert.ChangeType(CurrValue, typeof(T));
        }
    }
}
