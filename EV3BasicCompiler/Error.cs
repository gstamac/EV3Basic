using System;
using System.Linq;

namespace EV3BasicCompiler
{
    public class Error
    {
        public Error(string message, int line, int column)
        {
            Message = message;
            Line = line;
            Column = column;
        }

        public string Message { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public override string ToString()
        {
            return $"({Line}, {Column}): {Message}";
        }
    }
}
