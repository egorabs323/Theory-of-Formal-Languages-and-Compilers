using System;
using System.Collections.Generic;
using System.Globalization;

namespace YourNamespace
{
    public class SemanticAnalyzer
    {
        private readonly Dictionary<string, SymbolInfo> symbols = new(StringComparer.Ordinal);
        private readonly List<SemanticError> errors = new();

        public SemanticAnalysisResult Analyze(ProgramNode program)
        {
            errors.Clear();
            symbols.Clear();

            var validDeclarations = new List<FinalDoubleDeclarationNode>();
            var declarations = program?.Declarations ?? Array.Empty<FinalDoubleDeclarationNode>();

            foreach (var declaration in declarations)
            {
                var errorCountBefore = errors.Count;

                if (symbols.TryGetValue(declaration.Name, out var existing))
                {
                    errors.Add(new SemanticError(
                        declaration.Name,
                        declaration.NameLine,
                        declaration.NameColumn,
                        $"Ошибка: идентификатор \"{declaration.Name}\" уже объявлен ранее (строка {existing.Line})"));
                    continue;
                }

                var expressionErrorCountBefore = errors.Count;
                var expressionInfo = EvaluateExpression(declaration.Value);

                if (errors.Count != expressionErrorCountBefore)
                {
                    continue;
                }

                if (expressionInfo.Type != SemanticValueType.Double)
                {
                    errors.Add(new SemanticError(
                        GetExpressionFragment(declaration.Value),
                        declaration.Value.Line,
                        declaration.Value.Column,
                        $"Ошибка: тип значения \"{expressionInfo.TypeName}\" несовместим с объявленным типом \"double\""));
                }

                if (errors.Count != errorCountBefore)
                {
                    continue;
                }

                symbols[declaration.Name] = new SymbolInfo(
                    declaration.Name,
                    declaration.Type.Name,
                    declaration.NameLine,
                    declaration.NameColumn,
                    expressionInfo.HasValue ? expressionInfo.Value : (double?)null);

                validDeclarations.Add(declaration);
            }

            return new SemanticAnalysisResult(
                new ProgramNode(validDeclarations),
                errors.ToArray());
        }

        private ExpressionInfo EvaluateExpression(ExpressionNode node)
        {
            switch (node)
            {
                case NumberLiteralNode literal:
                    return EvaluateNumberLiteral(literal);

                case IdentifierExpressionNode identifier:
                    return EvaluateIdentifier(identifier);

                case UnaryExpressionNode unary:
                    return EvaluateUnary(unary);

                case BinaryExpressionNode binary:
                    return EvaluateBinary(binary);

                default:
                    return ExpressionInfo.Unknown();
            }
        }

        private ExpressionInfo EvaluateNumberLiteral(NumberLiteralNode literal)
        {
            var type = DetermineLiteralType(literal.Value);
            var normalized = NormalizeNumericLiteral(literal.Value);

            if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericValue) ||
                double.IsInfinity(numericValue) ||
                double.IsNaN(numericValue))
            {
                errors.Add(new SemanticError(
                    literal.Value,
                    literal.Line,
                    literal.Column,
                    $"Ошибка: значение \"{literal.Value}\" выходит за допустимые пределы типа double"));
                return ExpressionInfo.Unknown();
            }

            return new ExpressionInfo(type, numericValue, hasValue: true);
        }

        private ExpressionInfo EvaluateIdentifier(IdentifierExpressionNode identifier)
        {
            if (!symbols.TryGetValue(identifier.Name, out var symbol))
            {
                // Правило проверки использования идентификаторов отключено для этого варианта.
                return new ExpressionInfo(
                    SemanticValueType.Double,
                    0d,
                    hasValue: false);
            }

            return new ExpressionInfo(
                SemanticValueType.Double,
                symbol.Value ?? 0d,
                hasValue: symbol.Value.HasValue);
        }

        private ExpressionInfo EvaluateUnary(UnaryExpressionNode unary)
        {
            var operand = EvaluateExpression(unary.Operand);
            if (!operand.HasValue)
            {
                return operand;
            }

            var value = unary.Operator == "-"
                ? -operand.Value
                : operand.Value;

            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                errors.Add(new SemanticError(
                    GetExpressionFragment(unary),
                    unary.Line,
                    unary.Column,
                    "Ошибка: значение выражения выходит за допустимые пределы типа double"));
                return ExpressionInfo.Unknown();
            }

            return new ExpressionInfo(operand.Type, value, hasValue: true);
        }

        private ExpressionInfo EvaluateBinary(BinaryExpressionNode binary)
        {
            var left = EvaluateExpression(binary.Left);
            var right = EvaluateExpression(binary.Right);
            var resultType = MergeTypes(left.Type, right.Type);

            if (!left.HasValue || !right.HasValue)
            {
                return new ExpressionInfo(resultType, 0d, hasValue: false);
            }

            var value = binary.Operator == "+"
                ? left.Value + right.Value
                : left.Value - right.Value;

            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                errors.Add(new SemanticError(
                    GetExpressionFragment(binary),
                    binary.Line,
                    binary.Column,
                    "Ошибка: значение выражения выходит за допустимые пределы типа double"));
                return ExpressionInfo.Unknown();
            }

            return new ExpressionInfo(resultType, value, hasValue: true);
        }

        private static SemanticValueType DetermineLiteralType(string literal)
        {
            if (string.IsNullOrWhiteSpace(literal))
            {
                return SemanticValueType.Unknown;
            }

            var trimmed = literal.Trim();
            var hasFraction = trimmed.Contains(".") || trimmed.Contains("e") || trimmed.Contains("E");
            return hasFraction ? SemanticValueType.Double : SemanticValueType.Integer;
        }

        private static string NormalizeNumericLiteral(string literal)
        {
            if (string.IsNullOrWhiteSpace(literal))
            {
                return "0";
            }

            var trimmed = literal.Trim();
            if (trimmed.EndsWith("d", StringComparison.OrdinalIgnoreCase) ||
                trimmed.EndsWith("f", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed[..^1];
            }

            return trimmed;
        }

        private static SemanticValueType MergeTypes(SemanticValueType left, SemanticValueType right)
        {
            if (left == SemanticValueType.Unknown || right == SemanticValueType.Unknown)
            {
                return SemanticValueType.Unknown;
            }

            if (left == SemanticValueType.Double || right == SemanticValueType.Double)
            {
                return SemanticValueType.Double;
            }

            return SemanticValueType.Integer;
        }

        private static string GetExpressionFragment(ExpressionNode node)
        {
            return node switch
            {
                NumberLiteralNode literal => literal.Value,
                IdentifierExpressionNode identifier => identifier.Name,
                UnaryExpressionNode unary => unary.Operator + GetExpressionFragment(unary.Operand),
                BinaryExpressionNode binary => $"{GetExpressionFragment(binary.Left)} {binary.Operator} {GetExpressionFragment(binary.Right)}",
                _ => ""
            };
        }

        private sealed class SymbolInfo
        {
            public SymbolInfo(string name, string typeName, int line, int column, double? value)
            {
                Name = name;
                TypeName = typeName;
                Line = line;
                Column = column;
                Value = value;
            }

            public string Name { get; }
            public string TypeName { get; }
            public int Line { get; }
            public int Column { get; }
            public double? Value { get; }
        }

        private readonly struct ExpressionInfo
        {
            public ExpressionInfo(SemanticValueType type, double value, bool hasValue)
            {
                Type = type;
                Value = value;
                HasValue = hasValue;
            }

            public SemanticValueType Type { get; }
            public double Value { get; }
            public bool HasValue { get; }
            public string TypeName => Type switch
            {
                SemanticValueType.Double => "double",
                SemanticValueType.Integer => "int",
                _ => "unknown"
            };

            public static ExpressionInfo Unknown()
            {
                return new ExpressionInfo(SemanticValueType.Unknown, 0d, hasValue: false);
            }
        }
    }

    public enum SemanticValueType
    {
        Unknown,
        Integer,
        Double
    }

    public class SemanticError
    {
        public SemanticError(string fragment, int line, int column, string message)
        {
            Fragment = fragment;
            Line = line;
            Column = column;
            Message = message;
        }

        public string Fragment { get; }
        public int Line { get; }
        public int Column { get; }
        public string Message { get; }
    }

    public class SemanticAnalysisResult
    {
        public SemanticAnalysisResult(ProgramNode validAst, IReadOnlyList<SemanticError> errors)
        {
            ValidAst = validAst;
            Errors = errors ?? Array.Empty<SemanticError>();
        }

        public ProgramNode ValidAst { get; }
        public IReadOnlyList<SemanticError> Errors { get; }
    }
}
