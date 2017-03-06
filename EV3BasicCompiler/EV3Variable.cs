using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Statements;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.SmallBasic.Expressions;

namespace EV3BasicCompiler
{
    public class EV3Variable : IEV3Variable
    {

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
                Ev3Name = $"V{Name.ToUpper()}";
            }
        }
        public string Ev3Name { get; private set; }

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

        public void CompileCodeForDeclaration(TextWriter writer)
        {
            switch (Type)
            {
                case EV3Type.Int8:
                    writer.WriteLine($"DATA8 {Ev3Name}");
                    break;
                case EV3Type.Int16:
                    writer.WriteLine($"DATA16 {Ev3Name}");
                    break;
                case EV3Type.Int32:
                    writer.WriteLine($"DATA32 {Ev3Name}");
                    break;
                case EV3Type.Float:
                    writer.WriteLine($"DATAF {Ev3Name}");
                    break;
                case EV3Type.String:
                    writer.WriteLine($"DATAS {Ev3Name} 252");
                    break;
                case EV3Type.Int8Array:
                case EV3Type.Int16Array:
                case EV3Type.Int32Array:
                case EV3Type.FloatArray:
                case EV3Type.StringArray:
                    writer.WriteLine($"ARRAY16 {Ev3Name} {Math.Max(MaxIndex, 2)}");
                    break;
            }
        }

        public void CompileCodeForInitialization(TextWriter writer)
        {
            switch (Type)
            {
                case EV3Type.Int8:
                    writer.WriteLine($"    MOVE8_8 0 {Ev3Name}");
                    break;
                case EV3Type.Int16:
                    writer.WriteLine($"    MOVE16_16 0 {Ev3Name}");
                    break;
                case EV3Type.Int32:
                    writer.WriteLine($"    MOVE32_32 0 {Ev3Name}");
                    break;
                case EV3Type.Float:
                    writer.WriteLine($"    MOVEF_F 0.0 {Ev3Name}");
                    break;
                case EV3Type.String:
                    writer.WriteLine($"    STRINGS DUPLICATE '' {Ev3Name}");
                    break;
                case EV3Type.Int8Array:
                case EV3Type.Int16Array:
                case EV3Type.Int32Array:
                case EV3Type.FloatArray:
                    writer.WriteLine($"    CALL ARRAYCREATE_FLOAT {Ev3Name}");
                    break;
                case EV3Type.StringArray:
                    writer.WriteLine($"    CALL ARRAYCREATE_STRING {Ev3Name}");
                    break;
            }
        }

    }
}
