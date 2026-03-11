using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YourNamespace;

public class Lexer
{
    private readonly string input;
    private int position;
    private int line;
    private int column;

    public Lexer(string input)
    {
        this.input = input ?? "";
        this.position = 0;
        this.line = 1;
        this.column = 1;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (position < input.Length)
        {
            char currentChar = input[position];

            if (char.IsWhiteSpace(currentChar))
            {
                tokens.Add(ReadWhitespace());
            }
            else if (char.IsLetter(currentChar) || currentChar == '_')
            {
                tokens.Add(ReadIdentifierOrKeyword());
            }
            else if (char.IsDigit(currentChar) || currentChar == '.')
            {
                tokens.Add(ReadNumberLiteral());
            }
            else if (currentChar == '=')
            {
                tokens.Add(new Token { Type = TokenType.Operator, Value = "=", Line = line, Column = column, Length = 1 });
                Advance();
            }
            else if (currentChar == ';')
            {
                tokens.Add(new Token { Type = TokenType.Separator, Value = ";", Line = line, Column = column, Length = 1 });
                Advance();
            }
            else
            {
                tokens.Add(new Token { Type = TokenType.Error, Value = currentChar.ToString(), Line = line, Column = column, Length = 1 });
                Advance();
            }
        }
        return tokens;
    }

    private Token ReadWhitespace()
    {
        int startLine = line;
        int startCol = column;
        int start = position;
        while (position < input.Length && char.IsWhiteSpace(input[position]))
        {
            char c = input[position];
            if (c == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
            position++;
        }
        int length = position - start;
        string value = input.Substring(start, length);
        return new Token { Type = TokenType.Whitespace, Value = value, Line = startLine, Column = startCol, Length = length };
    }

    private Token ReadIdentifierOrKeyword()
    {
        int startLine = line;
        int startCol = column;
        int start = position;
        while (position < input.Length && (char.IsLetterOrDigit(input[position]) || input[position] == '_'))
        {
            position++;
            column++;
        }
        int length = position - start;
        string value = input.Substring(start, length);
        TokenType type = IsKeyword(value) ? TokenType.Keyword : TokenType.Identifier;
        return new Token { Type = type, Value = value, Line = startLine, Column = startCol, Length = length };
    }

    private Token ReadNumberLiteral()
    {
        int startLine = line;
        int startCol = column;
        int start = position;
        var regex = new Regex(@"^\d*\.?\d+(?:[eE][+-]?\d+)?[fFdD]?");

        var remaining = input.Substring(position);
        var match = regex.Match(remaining);

        if (match.Success)
        {
            string numberStr = match.Value;
            int len = numberStr.Length;
            position += len;
            column += len;
            return new Token { Type = TokenType.NumberLiteral, Value = numberStr, Line = startLine, Column = startCol, Length = len };
        }
        string errorChar = input.Substring(position, 1);
        position++;
        column++;
        return new Token { Type = TokenType.Error, Value = errorChar, Line = startLine, Column = startCol, Length = 1 };
    }

    private bool IsKeyword(string word)
    {
        return word == "double" ||
               word == "float" ||
               word == "final";  
    }

    private void Advance()
    {
        if (input[position] == '\n')
        {
            line++;
            column = 1;
        }
        else
        {
            column++;
        }
        position++;
    }
}