using System;
using System.Runtime.CompilerServices;

namespace UnitTestProject
{
    public static class TupleHelper
    {
        public static object[] ToArray(this ITuple tuple)
        {
            var array = new object[tuple.Length];
            for (int i = 0; i < tuple.Length; i++)
            {
                array[i] = tuple[i];
            }
            return array;
        }
    }
}