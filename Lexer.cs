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
    {///
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
            else if (currentChar == '-' || currentChar == '+')
            {
                tokens.Add(new Token { Type = TokenType.Operator, Value = currentChar.ToString(), Line = line, Column = column, Length = 1 });
                Advance();
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

        bool hasDigits = false;
        while (position < input.Length && char.IsDigit(input[position]))
        {
            position++;
            column++;
            hasDigits = true;
        }
        if (position < input.Length && input[position] == '.')
        {
            position++;
            column++;

            while (position < input.Length && char.IsDigit(input[position]))
            {
                position++;
                column++;
                hasDigits = true;
            }
        }

        if (position < input.Length && (input[position] == 'e' || input[position] == 'E'))
        {
            position++;
            column++;

            if (position < input.Length && (input[position] == '+' || input[position] == '-'))
            {
                position++;
                column++;
            }
            if (position >= input.Length || !char.IsDigit(input[position]))
            {
                return new Token { Type = TokenType.Error, Value = "e", Line = startLine, Column = startCol + (position - start - 1), Length = 1 };
            }

            while (position < input.Length && char.IsDigit(input[position]))
            {
                position++;
                column++;
            }
        }

        if (position < input.Length && (input[position] == 'f' || input[position] == 'F' || input[position] == 'd' || input[position] == 'D'))
        {
            position++;
            column++;
        }
        if (!hasDigits)
        {
            return new Token { Type = TokenType.Error, Value = input.Substring(start, 1), Line = startLine, Column = startCol, Length = 1 };
        }

        string value = input.Substring(start, position - start);
        return new Token { Type = TokenType.NumberLiteral, Value = value, Line = startLine, Column = startCol, Length = position - start };
    }

    private bool IsKeyword(string word)
    {
        return word == "double" ||
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