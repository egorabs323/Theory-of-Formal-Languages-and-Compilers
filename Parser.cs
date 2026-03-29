using System;
using System.Collections.Generic;
using System.Linq;
using YourNamespace;

public class Parser
{
    private readonly List<Token> tokens;
    private int position;
    private readonly List<ParserSyntaxError> errors;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens.ToList();
        this.position = 0;
        this.errors = new List<ParserSyntaxError>();
    }

    public ParseResult Parse()
    {
        errors.Clear();
        position = 0;

        if (tokens.Count == 0)
        {
            errors.Add(new ParserSyntaxError("Пустой вход", 1, 1));
            return new ParseResult { Success = false, Errors = errors };
        }

        ExpectKeyword("final");
        ExpectWhitespace();
        ExpectKeyword("double");
        ExpectWhitespace();
        ExpectIdentifier();
        SkipWhitespace();
        ExpectOperator("=");
        SkipWhitespace();
        ExpectNumber();
        ExpectSeparator(";");

        if (position < tokens.Count)
        {
            var extra = tokens[position];
            errors.Add(new ParserSyntaxError(
                $"Лишний токен '{extra.Value}' после завершения объявления",
                extra.Line, extra.Column));
        }

        return new ParseResult { Success = errors.Count == 0, Errors = errors };
    }

    private void ExpectKeyword(string expected)
    {
        var token = Current();
        if (token.Type == TokenType.Keyword && token.Value == expected)
        {
            Advance();
        }
        else
        {
            errors.Add(new ParserSyntaxError(
                $"Ожидается '{expected}', найдено '{token.Value}'",
                token.Line, token.Column));
            Advance();
        }
    }

    private void ExpectWhitespace()
    {
        var token = Current();
        if (token.Type == TokenType.Whitespace)
        {
            Advance();
        }
        else
        {
            errors.Add(new ParserSyntaxError(
                $"Ожидается пробел после '{tokens[position > 0 ? position - 1 : 0].Value}'",
                token.Line, token.Column));
        }
    }

    private void SkipWhitespace()
    {
        while (position < tokens.Count && Current().Type == TokenType.Whitespace)
        {
            Advance();
        }
    }

    private void ExpectIdentifier()
    {
        var token = Current();
        if (token.Type == TokenType.Identifier)
        {
            Advance();
        }
        else
        {
            errors.Add(new ParserSyntaxError(
                $"Ожидается имя константы (идентификатор), найдено '{token.Value}'",
                token.Line, token.Column));
            Advance();
        }
    }

    private void ExpectOperator(string expected)
    {
        var token = Current();
        if (token.Type == TokenType.Operator && token.Value == expected)
        {
            Advance();
        }
        else
        {
            errors.Add(new ParserSyntaxError(
                $"Ожидается оператор '{expected}', найдено '{token.Value}'",
                token.Line, token.Column));
            Advance();
        }
    }

    private void ExpectNumber()
    {
        var token = Current();

        if (token.Type == TokenType.Operator && (token.Value == "+" || token.Value == "-"))
        {
            Advance();
            token = Current();
        }

        if (token.Type == TokenType.NumberLiteral)
        {
            if (!token.Value.Contains("."))
            {
                errors.Add(new ParserSyntaxError(
                    "Ожидается вещественное число (с точкой), найдено целое число",
                    token.Line, token.Column));
            }
            Advance();
        }
        else
        {
            errors.Add(new ParserSyntaxError(
                $"Ожидается числовой литерал, найдено '{token.Value}'",
                token.Line, token.Column));
            Advance();
        }
    }

    private void ExpectSeparator(string expected)
    {
        var token = Current();
        if (token.Type == TokenType.Separator && token.Value == expected)
        {
            Advance();
        }
        else
        {
            errors.Add(new ParserSyntaxError(
                $"Ожидается '{expected}', найдено '{token.Value}'",
                token.Line, token.Column));
            Advance();
        }
    }

    private Token Current()
    {
        return position < tokens.Count ? tokens[position] :
               new Token
               {
                   Type = TokenType.Error,
                   Value = "EOF",
                   Line = tokens.LastOrDefault()?.Line ?? 1,
                   Column = tokens.LastOrDefault()?.Column ?? 1
               };
    }

    private void Advance()
    {
        if (position < tokens.Count) position++;
    }/////
}

public class ParseResult
{
    public bool Success { get; set; }
    public List<ParserSyntaxError> Errors { get; set; } = new();
}

public class ParserSyntaxError
{
    public string Message { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }

    public ParserSyntaxError(string message, int line, int column)
    {
        Message = message;
        Line = line;
        Column = column;
    }
}