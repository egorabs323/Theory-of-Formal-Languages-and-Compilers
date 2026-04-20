using System;
using System.Collections.Generic;
using System.Text;

namespace YourNamespace
{
    public abstract class AstNode
    {
        protected AstNode(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public int Line { get; }
        public int Column { get; }
    }

    public sealed class ProgramNode : AstNode
    {
        public ProgramNode(IReadOnlyList<FinalDoubleDeclarationNode> declarations)
            : base(1, 1)
        {
            Declarations = declarations ?? Array.Empty<FinalDoubleDeclarationNode>();
        }

        public IReadOnlyList<FinalDoubleDeclarationNode> Declarations { get; }
    }

    public sealed class FinalDoubleDeclarationNode : AstNode
    {
        public FinalDoubleDeclarationNode(
            int line,
            int column,
            string name,
            int nameLine,
            int nameColumn,
            TypeNode type,
            ExpressionNode value)
            : base(line, column)
        {
            Name = name ?? "";
            NameLine = nameLine;
            NameColumn = nameColumn;
            Type = type;
            Value = value;
        }

        public string Name { get; }
        public int NameLine { get; }
        public int NameColumn { get; }
        public TypeNode Type { get; }
        public ExpressionNode Value { get; }
    }

    public sealed class TypeNode : AstNode
    {
        public TypeNode(int line, int column, string name)
            : base(line, column)
        {
            Name = name ?? "";
        }

        public string Name { get; }
    }

    public abstract class ExpressionNode : AstNode
    {
        protected ExpressionNode(int line, int column)
            : base(line, column)
        {
        }
    }

    public sealed class NumberLiteralNode : ExpressionNode
    {
        public NumberLiteralNode(int line, int column, string value)
            : base(line, column)
        {
            Value = value ?? "";
        }

        public string Value { get; }
    }

    public sealed class IdentifierExpressionNode : ExpressionNode
    {
        public IdentifierExpressionNode(int line, int column, string name)
            : base(line, column)
        {
            Name = name ?? "";
        }

        public string Name { get; }
    }

    public sealed class UnaryExpressionNode : ExpressionNode
    {
        public UnaryExpressionNode(int line, int column, string @operator, ExpressionNode operand)
            : base(line, column)
        {
            Operator = @operator ?? "";
            Operand = operand;
        }

        public string Operator { get; }
        public ExpressionNode Operand { get; }
    }

    public sealed class BinaryExpressionNode : ExpressionNode
    {
        public BinaryExpressionNode(int line, int column, ExpressionNode left, string @operator, ExpressionNode right)
            : base(line, column)
        {
            Left = left;
            Operator = @operator ?? "";
            Right = right;
        }

        public ExpressionNode Left { get; }
        public string Operator { get; }
        public ExpressionNode Right { get; }
    }

    public static class AstTextFormatter
    {
        public static string Format(ProgramNode program)
        {
            var builder = new StringBuilder();
            WriteProgram(program ?? new ProgramNode(Array.Empty<FinalDoubleDeclarationNode>()), builder);
            return builder.ToString();
        }

        private static void WriteProgram(ProgramNode program, StringBuilder builder)
        {
            builder.AppendLine("ProgramNode");

            if (program.Declarations.Count == 0)
            {
                builder.AppendLine("\\- declarations: []");
                return;
            }

            for (var i = 0; i < program.Declarations.Count; i++)
            {
                WriteDeclaration(program.Declarations[i], builder, "", i == program.Declarations.Count - 1);
            }
        }

        private static void WriteDeclaration(FinalDoubleDeclarationNode node, StringBuilder builder, string prefix, bool isLast)
        {
            WriteLine(builder, prefix, isLast, "FinalDoubleDeclarationNode");
            var childPrefix = GetChildPrefix(prefix, isLast);

            WriteLine(builder, childPrefix, false, "modifiers: [\"final\"]");
            WriteNodeLine(builder, childPrefix, false, "type: TypeNode", node.Type, WriteType);
            WriteLine(builder, childPrefix, false, $"name: \"{node.Name}\"");
            WriteNodeLine(builder, childPrefix, true, $"value: {GetExpressionKind(node.Value)}", node.Value, WriteExpression);
        }

        private static void WriteType(TypeNode node, StringBuilder builder, string prefix)
        {
            WriteLine(builder, prefix, true, $"name: \"{node?.Name ?? ""}\"");
        }

        private static void WriteExpression(ExpressionNode node, StringBuilder builder, string prefix)
        {
            switch (node)
            {
                case NumberLiteralNode literal:
                    WriteLine(builder, prefix, true, $"value: {literal.Value}");
                    break;

                case IdentifierExpressionNode identifier:
                    WriteLine(builder, prefix, true, $"name: \"{identifier.Name}\"");
                    break;

                case UnaryExpressionNode unary:
                    WriteLine(builder, prefix, false, $"operator: \"{unary.Operator}\"");
                    WriteNodeLine(builder, prefix, true, $"operand: {GetExpressionKind(unary.Operand)}", unary.Operand, WriteExpression);
                    break;

                case BinaryExpressionNode binary:
                    WriteNodeLine(builder, prefix, false, $"left: {GetExpressionKind(binary.Left)}", binary.Left, WriteExpression);
                    WriteLine(builder, prefix, false, $"operator: \"{binary.Operator}\"");
                    WriteNodeLine(builder, prefix, true, $"right: {GetExpressionKind(binary.Right)}", binary.Right, WriteExpression);
                    break;

                default:
                    WriteLine(builder, prefix, true, "value: <unknown>");
                    break;
            }
        }

        private static void WriteNodeLine<TNode>(
            StringBuilder builder,
            string prefix,
            bool isLast,
            string label,
            TNode node,
            Action<TNode, StringBuilder, string> writer)
        {
            WriteLine(builder, prefix, isLast, label);
            writer(node, builder, GetChildPrefix(prefix, isLast));
        }

        private static void WriteLine(StringBuilder builder, string prefix, bool isLast, string text)
        {
            builder.Append(prefix);
            builder.Append(isLast ? "\\- " : "+- ");
            builder.AppendLine(text);
        }

        private static string GetChildPrefix(string prefix, bool isLast)
        {
            return prefix + (isLast ? "   " : "|  ");
        }

        private static string GetExpressionKind(ExpressionNode node)
        {
            return node switch
            {
                NumberLiteralNode => "NumberLiteralNode",
                IdentifierExpressionNode => "IdentifierNode",
                UnaryExpressionNode => "UnaryExpressionNode",
                BinaryExpressionNode => "BinaryExpressionNode",
                _ => "ExpressionNode"
            };
        }
    }
}
