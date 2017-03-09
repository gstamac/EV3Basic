using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using EV3BasicCompiler.Compilers;
using System;

namespace EV3BasicCompiler
{
    public abstract class EV3SubDefinitionBase
    {
        public string Name { get; }
        public string Signature { get; }
        public virtual string Code { get; private set; }
        public List<EV3Type> ParameterTypes { get; protected set; }
        public EV3Type ReturnType { get; protected set; }

        protected EV3SubDefinitionBase(string name, string signature, string code)
        {
            Name = name;
            Signature = signature;
            Code = code;
        }

        public abstract string Compile(TextWriter writer, EV3CompilerContext context, string[] arguments, string result);

        protected static EV3Type ParseType(string typeString)
        {
            switch (typeString)
            {
                case "8":
                    return EV3Type.Int8;
                case "16":
                    return EV3Type.Int16;
                case "32":
                    return EV3Type.Int32;
                case "F":
                    return EV3Type.Float;
                case "S":
                    return EV3Type.String;
                case "A":
                    return EV3Type.FloatArray;
                case "X":
                    return EV3Type.StringArray;
                case "V":
                    return EV3Type.Void;
            }
            return EV3Type.Unknown;
        }
    }
}
