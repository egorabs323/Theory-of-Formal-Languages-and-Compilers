using System;

namespace YourNamespace
{
    public enum TokenType
    {
        Keyword,
        Identifier,
        NumberLiteral,
        Operator,
        Separator,
        Whitespace,
        Error
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public int Length { get; set; }

        public override string ToString()
        {
            return $"[{Type}] '{Value}' @ Ln {Line}, Col {Column}";
        }
    }
}