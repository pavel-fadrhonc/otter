using System;

namespace LostPolygon.SpriteSharp.Utility.Internal {
    internal class Tuple<T1, T2> {
        public Tuple(T1 first, T2 second) {
            First = first;
            Second = second;
        }

        public T1 First { get; private set; }
        public T2 Second { get; private set; }

        public override string ToString() {
            return String.Format("{{ First: '{0}', Second: '{1}' }}", First, Second);
        }
    }

    internal static class Tuple {
        public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second) {
            Tuple<T1, T2> tuple = new Tuple<T1, T2>(first, second);
            return tuple;
        }
    }
}