using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EV3BasicCompiler
{
    public class EV3SubDefinition
    {
        public bool Referenced { get; set; }
        public string Name { get; private set; }
        public List<EV3Type> ParameterTypes { get; private set; }
        public EV3Type ReturnType { get; private set; }
        public string Signature { get; private set; }
        public string Code { get; private set; }
        public List<string> References { get; private set; }

        public EV3SubDefinition(string name, string signature, string code)
        {
            Referenced = false;
            Name = name;
            Signature = signature;
            Code = code;
        }

        public void ParseParameters()
        {
            ParameterTypes = new List<EV3Type>();
            ReturnType = EV3Type.Void;

            Dictionary<string, EV3Type> parameters = new Dictionary<string, EV3Type>();

            MatchCollection matches = Regex.Matches(Code, "IN_([^ \t]+)[ \t]+([^ \t\n\r]+)[ \t\n\r/]", RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                parameters.Add(match.Groups[2].Value, ParseType(match.Groups[1].Value));
            }

            matches = Regex.Matches(Code, "OUT_([^ \t]+)[ \t]+", RegexOptions.Singleline);
            if (matches.Count == 1)
            {
                ReturnType = ParseType(matches[0].Groups[1].Value);
            }
            else
            {
                if (parameters.Count > 0 && parameters.Values.Last() == EV3Type.Int16
                    && Regex.IsMatch(Code, "(ARRAY_WRITE|ARRAY FILL|(ARRAY WRITE_CONTENT[ \t]+[^ \t]+)|(ARRAY COPY[ \t]+[^ \t]+))[ \t]+" + parameters.Keys.Last() + "[ \t\n\r/]+", RegexOptions.Singleline))
                {
                    ReturnType = EV3Type.FloatArray;
                    parameters.Remove(parameters.Keys.Last());
                }
            }
            foreach (string name in parameters.Where(kv => kv.Value == EV3Type.Int16).Select(kv => kv.Key).ToArray())
            {
                if (Regex.IsMatch(Code, "(ARRAY_READ|ARRAY COPY)[ \t]+" + name + "[ \t]+", RegexOptions.Singleline))
                {
                    parameters[name] = EV3Type.FloatArray;
                }
            }

            ParameterTypes = parameters.Values.Select(pt => NormalizeType(pt)).ToList();
            ReturnType = NormalizeType(ReturnType);
        }

        public void ParseParameterTypes(string value)
        {
            ParameterTypes = value.Select(c => EV3SubDefinition.ParseType(c.ToString())).ToList();
        }

        public void ParseReturnType(string value)
        {
            ReturnType = ParseType(value);
        }

        public static EV3Type ParseType(string typeString)
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

        private EV3Type NormalizeType(EV3Type pt)
        {
            switch (pt)
            {
                case EV3Type.Int8:
                case EV3Type.Int16:
                case EV3Type.Int32:
                    return EV3Type.Float;
                case EV3Type.Int8Array:
                case EV3Type.Int16Array:
                case EV3Type.Int32Array:
                    return EV3Type.FloatArray;
            }
            return pt;
        }

        public void ParseReferences()
        {
            MatchCollection matches = Regex.Matches(Code, "CALL[ \t]+([^ \t\n\r]+)[ \t\n\r/]", RegexOptions.Singleline);
            References = matches.OfType<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();
        }
    }
}
