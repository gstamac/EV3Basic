using System;
using System.Linq;

namespace EV3BasicCompiler
{
    public enum EV3Type : int
    {
        Unknown = -1,
        Void = 0,
        Int8 = 1,
        Int16 = 2,
        Int32 = 3,
        Float = 4,
        String = 5,
        Int8Array = 101,
        Int16Array = 102,
        Int32Array = 103,
        FloatArray = 104,
        StringArray = 105
    }

    public static class EV3VariableTypeExtensions
    {
        private const int ARRAY_OFFSET = (int)EV3Type.Int8Array - (int)EV3Type.Int8;

        public static bool IsArray(this EV3Type type)
        {
            return (int)type >= (int)EV3Type.Int8Array;
        }

        public static bool IsNumber(this EV3Type type)
        {
            int typeId = (int)type;
            return (int)EV3Type.Int8 <= typeId && typeId <= (int)EV3Type.Float;
        }

        public static bool IsArrayOf(this EV3Type type, EV3Type baseType)
        {
            return type.IsArray() && type.BaseType() == baseType;
        }

        public static EV3Type BaseType(this EV3Type type)
        {
            if (type.IsArray())
                return (EV3Type)((int)type - ARRAY_OFFSET);
            else
                return type;
        }

        public static EV3Type ConvertToArray(this EV3Type type)
        {
            if (!type.IsArray())
                return (EV3Type)((int)type + ARRAY_OFFSET);
            else
                return type;
        }
    }
}
