using Microsoft.SmallBasic;
using Microsoft.SmallBasic.Expressions;
using Microsoft.SmallBasic.Statements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SBExpression = Microsoft.SmallBasic.Expressions.Expression;
using System;
using System.Globalization;
using EV3BasicCompiler.Compilers;
using EV3BasicCompiler;
using System.Windows;

namespace EV3BasicCompiler
{
    public class EV3Variables
    {
        private const int MAX_TEMP_VARIABLE_INDEX = 255;

        private Parser parser;
        private Dictionary<string, EV3Variable> variables;
        private readonly Dictionary<EV3Type, List<int>> tempVariablesCurrent;
        private readonly Dictionary<EV3Type, int> tempVariablesMax;

        public EV3Variables(Parser parser)
        {
            this.parser = parser;

            variables = new Dictionary<string, EV3Variable>();

            tempVariablesCurrent = new Dictionary<EV3Type, List<int>>();
            tempVariablesMax = new Dictionary<EV3Type, int>();
            Clear();
        }

        public void Clear()
        {
            variables.Clear();

            tempVariablesCurrent.Clear();
            tempVariablesMax.Clear();
            foreach (EV3Type type in Enum.GetValues(typeof(EV3Type)))
            {
                tempVariablesCurrent[type] = new List<int>();
                tempVariablesMax[type] = -1;
            }
        }

        public void Process()
        {
            variables = parser.SymbolTable.InitializedVariables
                .ToDictionary(v => v.Key, v => new EV3Variable(v.Key, v.Value));
        }

        public void CompileCodeForVariableDeclarations(TextWriter writer)
        {
            foreach (EV3Variable variable in variables.Values.Where(v => !v.IsConstant))
            {
                variable.CompileCodeForDeclaration(writer);
            }
        }

        public void CompileCodeForVariableInitializations(TextWriter writer)
        {
            foreach (EV3Variable variable in variables.Values.Where(v => !v.IsConstant))
            {
                variable.CompileCodeForInitialization(writer);
            }
        }

        public void CompileCodeForTempVariableDeclarationsFloat(TextWriter writer)
        {
            for (int i = 0; i <= tempVariablesMax[EV3Type.Float]; i++)
            {
                writer.WriteLine($"    DATAF F{i}");
            }
        }

        public void CompileCodeForTempVariableDeclarationsString(TextWriter writer)
        {
            for (int i = 0; i <= tempVariablesMax[EV3Type.String]; i++)
            {
                writer.WriteLine($"    DATAS S{i} 252");
            }
        }

        public TempVariableCreator UseTempVariables()
        {
            return new TempVariableCreator(t => GetTempVariableId(t), t => RemoveTempVariable(t));
        }

        public class TempVariableCreator : IDisposable
        {
            private Func<EV3Type, int> tempIdGenerator;
            private Action<IEV3Variable> tempRemover;
            private List<IEV3Variable> createdTemps;

            public TempVariableCreator(Func<EV3Type, int> tempIdGenerator, Action<IEV3Variable> tempRemover)
            {
                this.tempIdGenerator = tempIdGenerator;
                this.tempRemover = tempRemover;
                createdTemps = new List<IEV3Variable>();
            }

            public IEV3Variable CreateVariable(EV3Type type)
            {
                TempEV3Variable tempEV3Variable = new TempEV3Variable(type, () => tempIdGenerator(type));
                createdTemps.Add(tempEV3Variable);
                return tempEV3Variable;
            }

            public void Dispose()
            {
                foreach (IEV3Variable eV3Variable in createdTemps)
                {
                    tempRemover(eV3Variable);
                }
            }
        }

        class TempEV3Variable : IEV3Variable
        {
            private Func<int> tempIdGenerator;
            private string ev3Name = null;

            public TempEV3Variable(EV3Type type, Func<int> tempIdGenerator)
            {
                Type = type;
                this.tempIdGenerator = tempIdGenerator;
            }

            public string Ev3Name
            {
                get
                {
                    if (ev3Name == null)
                    {
                        Id = tempIdGenerator();
                        if (Type.IsArray())
                        {
                            if (Type == EV3Type.StringArray)
                                ev3Name = $"X{Id}";
                            else
                                ev3Name = $"A{Id}";
                        }
                        else
                        {
                            if (Type == EV3Type.String)
                                ev3Name = $"S{Id}";
                            else
                                ev3Name = $"F{Id}";
                        }
                    }
                    return ev3Name;
                }
            }

            public EV3Type Type { get; private set; }
            public int Id { get; private set; } = -1;
        }

        private int GetTempVariableId(EV3Type type)
        {
            int firstAvailable = -1;
            for (int i = 0; i <= MAX_TEMP_VARIABLE_INDEX; i++)
            {
                if (!tempVariablesCurrent[type].Contains(i))
                {
                    firstAvailable = i;
                    break;
                }
            }
            tempVariablesCurrent[type].Add(firstAvailable);
            tempVariablesMax[type] = Math.Max(tempVariablesMax[type], firstAvailable);

            return firstAvailable;
        }

        private void RemoveTempVariable(IEV3Variable variable)
        {
            if (variable is TempEV3Variable)
                tempVariablesCurrent[variable.Type].Remove(((TempEV3Variable)variable).Id);
        }

        public EV3Variable FindVariable(string variableName)
        {
            if (!variables.ContainsKey(variableName)) return null;

            return variables[variableName];
        }

        private EV3Variable FindVariable(SBExpression expression)
        {
            return variables.Values.FirstOrDefault(v => expression.IsVariable(v));
        }

        public EV3Variable FindVariable(ForStatement statement)
        {
            return variables.Values.FirstOrDefault(v => statement.IsVariable(v));
        }
    }
}
