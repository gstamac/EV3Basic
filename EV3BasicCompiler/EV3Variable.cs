using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SmallBasic;

namespace EV3BasicCompiler
{
    public class EV3Variable
    {
        private string ev3Name = "";

        public EV3Variable(string name, TokenInfo tokenInfo)
        {
            Name = name;
            Type = EV3Type.Unknown;
            TokenInfo = tokenInfo;
        }

        private string name;
        public string Name
        {
            get { return name; }
            private set
            {
                name = value;
                ev3Name = $"V{Name.ToUpper()}";
            }
        }

        public TokenInfo TokenInfo { get; set; }
        public EV3Type Type { get; set; }
        public int MaxIndex { get; private set; }
        public string Comment { get; set; }

        private void UpdateVariableType(EV3Type type)
        {
            if ((int)type > (int)Type)
                Type = type;
        }

        public void UpdateMaxIndex(int index)
        {
            if (MaxIndex < index)
                MaxIndex = index;
        }
        public string GenerateDeclarationCode()
        {
            if (Type.IsArray())
                return $"ARRAY16 {ev3Name} {Math.Max(MaxIndex, 2)}";
            else
            {
                switch (Type)
                {
                    case EV3Type.Int8:
                        return $"DATA8 {ev3Name}";
                    case EV3Type.Int16:
                        return $"DATA16 {ev3Name}";
                    case EV3Type.Int32:
                        return $"DATA32 {ev3Name}";
                    case EV3Type.Float:
                        return $"DATAF {ev3Name}";
                    case EV3Type.String:
                        return $"DATAS {ev3Name} 252";
                }
            }
            return "";
        }

        public string GenerateInitializationCode()
        {
            if (Type.IsArray())
                switch (Type)
                {
                    case EV3Type.Int8:
                    case EV3Type.Int16:
                    case EV3Type.Int32:
                    case EV3Type.FloatArray:
                        return $"CALL ARRAYCREATE_FLOAT {ev3Name}";
                    case EV3Type.StringArray:
                        return $"CALL ARRAYCREATE_STRING {ev3Name}";
                }
            else
            {
                switch (Type)
                {
                    case EV3Type.Int8:
                        return $"MOVE8_8 0 {ev3Name}";
                    case EV3Type.Int16:
                        return $"MOVE16_16 0 {ev3Name}";
                    case EV3Type.Int32:
                        return $"MOVE32_32 0 {ev3Name}";
                    case EV3Type.Float:
                        return $"MOVEF_F 0.0 {ev3Name}";
                    case EV3Type.String:
                        return $"STRINGS DUPLICATE '' {ev3Name}";
                }
            }
            return "";
        }
    }
}
