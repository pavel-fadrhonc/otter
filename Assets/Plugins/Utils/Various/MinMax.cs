﻿using UnityEngine;
using System.Collections;
using System;

namespace OakFramework2.Utils
{
    [Serializable]
    public class MinMax
    {
        public float Min;
        public float Max;

        public MinMax() { }

        public MinMax(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public MinMax(MinMax other)
        {
            Min = other.Min;
            Max = other.Max;
        }

        public static implicit operator Vector2(MinMax span)
        {
            return new Vector2(span.Min, span.Max);
        }

        public static MinMax operator *(MinMax span, float multiplier)
        {
            span.Min *= multiplier;
            span.Max *= multiplier;

            return span;
        }

        public float RandomValue()
        {
            return UnityEngine.Random.Range(Min, Max);
        }

        public float Lerp(float value)
        {
            return Mathf.Lerp(Min, Max, value);
        }
    }
}