using System;
using System.Collections.Generic;
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
    }
    public class EV3Variable
    {
        public EV3Variable(string name)
        {
            Name = name;
            Type = EV3VariableType.Unknown;
            IsArray = false;
            References = new List<EV3Variable>();
        }

        public string Name { get; private set; }
        public EV3VariableType Type { get; set; }
        public bool IsArray { get; set; }
        public int MaxIndex { get; private set; }
        public string Comment { get; set; }
        public List<EV3Variable> References { get; private set; }

        public void UpdateVariableType(EV3VariableType type)
        {
            if ((int)type > (int)Type)
                Type = type;
        }

        public void UpdateVariableTypeFromReferences()
        {
            UpdateVariableTypeFromReferences(new List<EV3Variable>());
        }

        public void UpdateVariableTypeFromReferences(List<EV3Variable> trail)
        {
            List<EV3Variable> myTrail = new List<EV3Variable>(trail);
            myTrail.Add(this);

            foreach (EV3Variable reference in References.Except(myTrail))
            {
                reference.UpdateVariableTypeFromReferences(myTrail);
                UpdateVariableType(reference.Type);
            }
        }

        public void UpdateMaxIndex(int index)
        {
            if (MaxIndex < index)
                MaxIndex = index;
        }
        public string GenerateCode()
        {
            if (IsArray)
                return $"ARRAY16 {Name} {MaxIndex}";
            else
            {
                switch (Type)
                {
                    case EV3VariableType.Unknown:
                        return $"DATAS {Name} 252 // UNKNOWN";
                    case EV3VariableType.Int8:
                        return $"DATA8 {Name}";
                    case EV3VariableType.Int16:
                        return $"DATA16 {Name}";
                    case EV3VariableType.Int32:
                        return $"DATA32 {Name}";
                    case EV3VariableType.Float:
                        return $"DATAF {Name}";
                    case EV3VariableType.String:
                        return $"DATAS {Name} 252";
                }
            }
            throw new Exception("This should not happen!");
        }
    }
}
