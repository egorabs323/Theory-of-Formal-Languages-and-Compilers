using System;

namespace YourNamespace
{
    public class ErrorEntry
    {
        public string ErrorFragment { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }

        public int Line { get; set; }
        public int Column { get; set; }

        public ErrorEntry(string errorFragment, string location, string description, int line, int column)
        {
            ErrorFragment = errorFragment;
            Location = location;
            Description = description;
            Line = line;
            Column = column;
        }
    }

    public class LexemeEntry
    {
        public int Code { get; set; }
        public string TokenType { get; set; }
        public string Value { get; set; }
        public string Location { get; set; }

        public LexemeEntry(int code, string tokenType, string value, string location)
        {
            Code = code;
            TokenType = tokenType;
            Value = value;
            Location = location;
        }
    }
}