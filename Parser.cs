using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YourNamespace
{
    public class Parser
    {
        private sealed class ExpectedSymbol
        {
            public string DisplayName { get; }
            public bool AllowLexerError { get; }
            private readonly Func<Token, bool> matcher;

            public ExpectedSymbol(Func<Token, bool> matcher, string displayName, bool allowLexerError = false)
            {
                this.matcher = matcher;
                DisplayName = displayName;
                AllowLexerError = allowLexerError;
            }

            public bool Matches(Token token)
            {
                return matcher(token);
            }
        }

        private enum RecoveryActionKind
        {
            InsertMissing,
            DeleteUnexpected,
            ReportLexerError
        }

        private sealed class RecoveryAction
        {
            public RecoveryActionKind Kind { get; }
            public int TokenIndex { get; }
            public int ExpectedFrom { get; }
            public int ExpectedToExclusive { get; }

            public RecoveryAction(RecoveryActionKind kind, int tokenIndex, int expectedFrom, int expectedToExclusive)
            {
                Kind = kind;
                TokenIndex = tokenIndex;
                ExpectedFrom = expectedFrom;
                ExpectedToExclusive = expectedToExclusive;
            }
        }

        private sealed class RecoveryPlan
        {
            public int RecoveryCount { get; }
            public int DeletedTokenCount { get; }
            public int InsertedSymbolCount { get; }
            public List<RecoveryAction> Actions { get; }
            public ExpectedSymbol[] Pattern { get; }

            public RecoveryPlan(
                int recoveryCount,
                int deletedTokenCount,
                int insertedSymbolCount,
                List<RecoveryAction> actions,
                ExpectedSymbol[] pattern)
            {
                RecoveryCount = recoveryCount;
                DeletedTokenCount = deletedTokenCount;
                InsertedSymbolCount = insertedSymbolCount;
                Actions = actions ?? new List<RecoveryAction>();
                Pattern = pattern;
            }
        }

        private static bool IsFloatNumber(Token token)
        {
            if (token.Type != TokenType.NumberLiteral)
                return false;

            return Regex.IsMatch(token.Value, @"^[+-]?\d+\.\d+([eE][+-]?\d+)?$");
        }

        private static readonly ExpectedSymbol[] StatementPattern =
        {
            Keyword("final", "'final'"),
            Keyword("double", "'double'"),
            new ExpectedSymbol(token => token.Type == TokenType.Identifier, "Parser_ExpectedIdentifier"),
            Operator("=", "'='"),
            new ExpectedSymbol(token => IsFloatNumber(token), "Parser_ExpectedFloatNumber", allowLexerError: true),
            Separator(";", "';'")
        };

        private static readonly ExpectedSymbol[] SignedPositiveStatementPattern =
        {
            Keyword("final", "'final'"),
            Keyword("double", "'double'"),
            new ExpectedSymbol(token => token.Type == TokenType.Identifier, "Parser_ExpectedIdentifier"),
            Operator("=", "'='"),
            Operator("+", "'+'"),
            new ExpectedSymbol(token => IsFloatNumber(token), "Parser_ExpectedFloatNumber", allowLexerError: true),
            Separator(";", "';'")
        };

        private static readonly ExpectedSymbol[] SignedNegativeStatementPattern =
        {
            Keyword("final", "'final'"),
            Keyword("double", "'double'"),
            new ExpectedSymbol(token => token.Type == TokenType.Identifier, "Parser_ExpectedIdentifier"),
            Operator("=", "'='"),
            Operator("-", "'-'"),
            new ExpectedSymbol(token => IsFloatNumber(token), "Parser_ExpectedFloatNumber", allowLexerError: true),
            Separator(";", "';'")
        };

        private static readonly ExpectedSymbol[][] StatementPatterns =
        {
            StatementPattern,
            SignedPositiveStatementPattern,
            SignedNegativeStatementPattern
        };

        private const string EmptyInputMessageKey = "Parser_EmptyInput";
        private const string WhitespaceOnlyInputMessageKey = "Parser_WhitespaceOnlyInput";
        private const string InvalidFragmentMessageKey = "Parser_InvalidFragment";

        private readonly List<Token> sourceTokens;
        private List<Token> syntaxTokens;
        private int position;
        private List<ParserSyntaxError> errors;

        public Parser(List<Token> tokens)
        {
            sourceTokens = tokens?.ToList() ?? new List<Token>();
            syntaxTokens = new List<Token>();
            errors = new List<ParserSyntaxError>();
        }

        public ParseResult Parse()
        {
            errors = new List<ParserSyntaxError>();
            syntaxTokens = sourceTokens
                .Where(token => token.Type != TokenType.Whitespace)
                .ToList();
            position = 0;

            if (sourceTokens.Count == 0)
            {
                errors.Add(new ParserSyntaxError("", 1, 1, LocalizationManager.GetString(EmptyInputMessageKey)));
                return BuildResult();
            }

            if (syntaxTokens.Count == 0)
            {
                errors.Add(new ParserSyntaxError("", 1, 1, LocalizationManager.GetString(WhitespaceOnlyInputMessageKey)));
                return BuildResult();
            }

            ParseProgram();
            return BuildResult();
        }

        private ParseResult BuildResult()
        {
            return new ParseResult
            {
                Success = errors.Count == 0,
                Errors = errors
            };
        }

        private void ParseProgram()
        {
            while (position < syntaxTokens.Count)
            {
                ParseStatement();
            }
        }

        private void ParseStatement()
        {
            if (position >= syntaxTokens.Count)
            {
                return;
            }

            var statementTokens = new List<Token>();

            while (position < syntaxTokens.Count)
            {
                var current = CurrentToken();
                if (current == null)
                {
                    break;
                }

                statementTokens.Add(current);
                position++;

                if (current.Type == TokenType.Separator && current.Value == ";")
                {
                    break;
                }
            }

            if (statementTokens.Count > 0)
            {
                ValidateStatement(statementTokens);
            }
        }

        private void ValidateStatement(List<Token> tokens)
        {
            if (tokens == null || tokens.Count == 0)
            {
                return;
            }

            RecoveryPlan bestPlan = null;

            foreach (var pattern in StatementPatterns)
            {
                var memo = new RecoveryPlan[tokens.Count + 1, pattern.Length + 1];
                var calculated = new bool[tokens.Count + 1, pattern.Length + 1];
                var plan = BuildRecoveryPlan(tokens, 0, 0, pattern, memo, calculated);
                bestPlan = ChooseBetter(bestPlan, plan);
            }

            if (bestPlan == null)
            {
                return;
            }

            for (var actionIndex = 0; actionIndex < bestPlan.Actions.Count; actionIndex++)
            {
                var action = bestPlan.Actions[actionIndex];

                if (action.Kind == RecoveryActionKind.InsertMissing)
                {
                    var fragmentInfo = GetFragmentInfoForInsertion(tokens, bestPlan.Actions, action);
                    AddMissingSequenceError(
                        bestPlan.Pattern,
                        action.ExpectedFrom,
                        action.ExpectedToExclusive,
                        fragmentInfo.Fragment,
                        fragmentInfo.Line,
                        fragmentInfo.Column);
                    continue;
                }

                if (action.Kind == RecoveryActionKind.DeleteUnexpected)
                {
                    var deletedTokenIndexes = new List<int> { action.TokenIndex };

                    while (actionIndex + 1 < bestPlan.Actions.Count
                        && bestPlan.Actions[actionIndex + 1].Kind == RecoveryActionKind.DeleteUnexpected)
                    {
                        actionIndex++;
                        deletedTokenIndexes.Add(bestPlan.Actions[actionIndex].TokenIndex);
                    }

                    if (actionIndex + 1 < bestPlan.Actions.Count
                        && bestPlan.Actions[actionIndex + 1].Kind == RecoveryActionKind.InsertMissing)
                    {
                        continue;
                    }

                    AddInvalidFragmentError(tokens, deletedTokenIndexes);
                    continue;
                }

                if (action.Kind == RecoveryActionKind.ReportLexerError
                    && action.TokenIndex >= 0
                    && action.TokenIndex < tokens.Count)
                {
                    var token = tokens[action.TokenIndex];
                    errors.Add(new ParserSyntaxError(
                        token.Value,
                        token.Line,
                        token.Column,
                        ConvertLexerErrorToMessage(token)));
                }
            }
        }

        private void AddInvalidFragmentError(List<Token> tokens, List<int> deletedTokenIndexes)
        {
            if (tokens == null
                || deletedTokenIndexes == null
                || deletedTokenIndexes.Count == 0)
            {
                return;
            }

            deletedTokenIndexes.Sort();

            var startIndex = deletedTokenIndexes[0];
            var endIndex = deletedTokenIndexes[^1];

            if (startIndex < 0
                || endIndex >= tokens.Count
                || endIndex < startIndex)
            {
                return;
            }

            var startToken = tokens[startIndex];
            errors.Add(new ParserSyntaxError(
                BuildFragment(tokens, startIndex, endIndex),
                startToken.Line,
                startToken.Column,
                LocalizationManager.GetString(InvalidFragmentMessageKey)));
        }

        private RecoveryPlan BuildRecoveryPlan(
            List<Token> tokens,
            int tokenIndex,
            int expectedIndex,
            ExpectedSymbol[] pattern,
            RecoveryPlan[,] memo,
            bool[,] calculated)
        {
            if (calculated[tokenIndex, expectedIndex])
            {
                return memo[tokenIndex, expectedIndex];
            }

            RecoveryPlan bestPlan;

            if (tokenIndex >= tokens.Count)
            {
                if (expectedIndex >= pattern.Length)
                {
                    bestPlan = new RecoveryPlan(0, 0, 0, new List<RecoveryAction>(), pattern);
                }
                else
                {
                    bestPlan = new RecoveryPlan(
                        1,
                        0,
                        pattern.Length - expectedIndex,
                        new List<RecoveryAction>
                        {
                            new RecoveryAction(RecoveryActionKind.InsertMissing, tokenIndex, expectedIndex, pattern.Length)
                        },
                        pattern);
                }
            }
            else if (expectedIndex >= pattern.Length)
            {
                var deleteActions = new List<RecoveryAction>();
                for (var i = tokenIndex; i < tokens.Count; i++)
                {
                    deleteActions.Add(new RecoveryAction(
                        RecoveryActionKind.DeleteUnexpected,
                        i,
                        expectedIndex,
                        expectedIndex));
                }

                bestPlan = new RecoveryPlan(0, tokens.Count - tokenIndex, 0, deleteActions, pattern);
            }
            else
            {
                bestPlan = null;
                var currentToken = tokens[tokenIndex];

                if (IsMatch(currentToken, expectedIndex, pattern))
                {
                    var matchPlan = BuildRecoveryPlan(tokens, tokenIndex + 1, expectedIndex + 1, pattern, memo, calculated);
                    RecoveryAction matchAction = null;

                    if (pattern[expectedIndex].AllowLexerError && currentToken.Type == TokenType.Error)
                    {
                        matchAction = new RecoveryAction(
                            RecoveryActionKind.ReportLexerError,
                            tokenIndex,
                            expectedIndex,
                            expectedIndex + 1);
                    }

                    var candidate = PrependAction(matchPlan, matchAction, 0, 0, 0, pattern);
                    bestPlan = ChooseBetter(bestPlan, candidate);
                }

                var deletePlan = BuildRecoveryPlan(tokens, tokenIndex + 1, expectedIndex, pattern, memo, calculated);
                var deleteAction = new RecoveryAction(
                    RecoveryActionKind.DeleteUnexpected,
                    tokenIndex,
                    expectedIndex,
                    expectedIndex);
                bestPlan = ChooseBetter(bestPlan, PrependAction(deletePlan, deleteAction, 0, 1, 0, pattern));

                for (var syncIndex = expectedIndex + 1; syncIndex < pattern.Length; syncIndex++)
                {
                    if (!IsMatch(currentToken, syncIndex, pattern))
                    {
                        continue;
                    }

                    var syncPlan = BuildRecoveryPlan(tokens, tokenIndex, syncIndex, pattern, memo, calculated);
                    var insertAction = new RecoveryAction(
                        RecoveryActionKind.InsertMissing,
                        tokenIndex,
                        expectedIndex,
                        syncIndex);

                    bestPlan = ChooseBetter(
                        bestPlan,
                        PrependAction(syncPlan, insertAction, 1, 0, syncIndex - expectedIndex, pattern));
                }
            }

            calculated[tokenIndex, expectedIndex] = true;
            memo[tokenIndex, expectedIndex] = bestPlan;
            return bestPlan;
        }

        private static RecoveryPlan PrependAction(
            RecoveryPlan basePlan,
            RecoveryAction action,
            int recoveryDelta,
            int deletedDelta,
            int insertedDelta,
            ExpectedSymbol[] pattern)
        {
            var actions = new List<RecoveryAction>();
            if (action != null)
            {
                actions.Add(action);
            }

            if (basePlan != null && basePlan.Actions.Count > 0)
            {
                actions.AddRange(basePlan.Actions);
            }

            return new RecoveryPlan(
                (basePlan != null ? basePlan.RecoveryCount : 0) + recoveryDelta,
                (basePlan != null ? basePlan.DeletedTokenCount : 0) + deletedDelta,
                (basePlan != null ? basePlan.InsertedSymbolCount : 0) + insertedDelta,
                actions,
                basePlan?.Pattern ?? pattern);
        }

        private static RecoveryPlan ChooseBetter(RecoveryPlan current, RecoveryPlan candidate)
        {
            if (candidate == null)
            {
                return current;
            }

            if (current == null)
            {
                return candidate;
            }

            if (candidate.DeletedTokenCount != current.DeletedTokenCount)
            {
                return candidate.DeletedTokenCount < current.DeletedTokenCount ? candidate : current;
            }

            if (candidate.RecoveryCount != current.RecoveryCount)
            {
                return candidate.RecoveryCount < current.RecoveryCount ? candidate : current;
            }

            if (candidate.InsertedSymbolCount != current.InsertedSymbolCount)
            {
                return candidate.InsertedSymbolCount < current.InsertedSymbolCount ? candidate : current;
            }

            return candidate.Actions.Count < current.Actions.Count ? candidate : current;
        }

        private static bool IsMatch(Token token, int expectedIndex, ExpectedSymbol[] pattern)
        {
            var expected = pattern[expectedIndex];
            if (expected.Matches(token))
            {
                return true;
            }

            return expected.AllowLexerError && token.Type == TokenType.Error;
        }

        private (string Fragment, int Line, int Column) GetFragmentInfoForInsertion(
            List<Token> tokens,
            List<RecoveryAction> actions,
            RecoveryAction insertAction)
        {
            if (tokens == null || tokens.Count == 0)
            {
                return ("", 1, 1);
            }

            var insertActionIndex = actions.IndexOf(insertAction);
            if (insertActionIndex > 0)
            {
                var deletedTokenIndexes = new List<int>();

                for (var i = insertActionIndex - 1; i >= 0; i--)
                {
                    var action = actions[i];
                    if (action.Kind != RecoveryActionKind.DeleteUnexpected)
                    {
                        break;
                    }

                    deletedTokenIndexes.Add(action.TokenIndex);
                }

                if (deletedTokenIndexes.Count > 0)
                {
                    deletedTokenIndexes.Sort();

                    var startIndex = deletedTokenIndexes[0];
                    var endIndex = deletedTokenIndexes[^1];
                    var startToken = tokens[startIndex];

                    return (
                        BuildFragment(tokens, startIndex, endIndex),
                        startToken.Line,
                        startToken.Column);
                }
            }

            if (insertAction.TokenIndex >= 0 && insertAction.TokenIndex < tokens.Count)
            {
                var token = tokens[insertAction.TokenIndex];
                return (token.Value, token.Line, token.Column);
            }

            var lastToken = tokens[^1];
            return ("EOF", lastToken.Line, lastToken.Column + Math.Max(lastToken.Length, 1));
        }

        private static string BuildFragment(List<Token> tokens, int startIndex, int endIndex)
        {
            if (tokens == null
                || tokens.Count == 0
                || startIndex < 0
                || endIndex < startIndex
                || endIndex >= tokens.Count)
            {
                return "";
            }

            var parts = new List<string>();
            for (var i = startIndex; i <= endIndex; i++)
            {
                parts.Add(tokens[i].Value);
            }

            return string.Join(" ", parts);
        }

        private void AddMissingSequenceError(
            ExpectedSymbol[] pattern,
            int expectedFrom,
            int expectedToExclusive,
            string fragment,
            int line,
            int column)
        {
            if (expectedFrom >= expectedToExclusive)
            {
                return;
            }

            for (var i = expectedFrom; i < expectedToExclusive; i++)
            {
                errors.Add(new ParserSyntaxError(
                    fragment,
                    line,
                    column,
                    LocalizationManager.GetString("Parser_ExpectedPrefix") + " "
                    + LocalizationManager.GetString(pattern[i].DisplayName)));
            }
        }

        private static string ConvertLexerErrorToMessage(Token token)
        {
            if (token == null)
            {
                return LocalizationManager.GetString("Parser_InvalidNumberFormat");
            }

            if (token.Value == ".")
            {
                return LocalizationManager.GetString("Parser_ExpectedDigitAfterDecimal");
            }

            if (token.Value == "e" || token.Value == "E")
            {
                return LocalizationManager.GetString("Parser_ExpectedDigitAfterExponent");
            }

            return LocalizationManager.GetString("Parser_InvalidNumberFormat");
        }

        private Token CurrentToken()
        {
            if (position < syntaxTokens.Count)
            {
                return syntaxTokens[position];
            }

            return null;
        }

        private int CurrentLine()
        {
            if (position < syntaxTokens.Count)
            {
                return syntaxTokens[position].Line;
            }

            return 1;
        }

        private static ExpectedSymbol Keyword(string value, string displayName)
        {
            return new ExpectedSymbol(
                token => token.Type == TokenType.Keyword && token.Value == value,
                displayName);
        }

        private static ExpectedSymbol Operator(string value, string displayName)
        {
            return new ExpectedSymbol(
                token => token.Type == TokenType.Operator && token.Value == value,
                displayName);
        }

        private static ExpectedSymbol Separator(string value, string displayName)
        {
            return new ExpectedSymbol(
                token => token.Type == TokenType.Separator && token.Value == value,
                displayName);
        }
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
        public string Fragment { get; set; }

        public ParserSyntaxError(string fragment, int line, int column, string message)
        {
            Fragment = fragment;
            Line = line;
            Column = column;
            Message = message;
        }
    }
}
