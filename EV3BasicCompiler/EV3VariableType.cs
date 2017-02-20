using System;
using System.Linq;

namespace EV3BasicCompiler
{
    public enum EV3VariableType : int
    {
        Unknown = -1,
        Int8 = 0,
        Int16 = 1,
        Int32 = 2,
        Float = 3,
        String = 4,
        Int8Array = 100,
        Int16Array = 101,
        Int32Array = 102,
        FloatArray = 103,
        StringArray = 104
    }

    public static class EV3VariableTypeExtensions
    {
        public static bool IsArray(this EV3VariableType type)
        {
            return (int)type >= (int)EV3VariableType.Int8Array;
        }

        public static EV3VariableType BaseType(this EV3VariableType type)
        {
            if (type.IsArray())
                return (EV3VariableType)((int)type - (int)EV3VariableType.Int8Array);
            else
                return type;
        }

        public static EV3VariableType ConvertToArray(this EV3VariableType type)
        {
            if (!type.IsArray())
                return (EV3VariableType)((int)type + (int)EV3VariableType.Int8Array);
            else
                return type;
        }
    }
}
