using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace YourNamespace
{
    public sealed class FlexBisonAnalysisResult
    {
        public List<Token> Tokens { get; } = new List<Token>();
        public List<ParserSyntaxError> ParseErrors { get; } = new List<ParserSyntaxError>();
        public string InfrastructureError { get; set; }
    }

    public static class FlexBisonAnalyzer
    {
        public static FlexBisonAnalysisResult Analyze(string code)
        {
            var result = new FlexBisonAnalysisResult();
            string analyzerPath = ResolveAnalyzerPath();

            if (string.IsNullOrWhiteSpace(analyzerPath) || !File.Exists(analyzerPath))
            {
                result.InfrastructureError = "flex/bison analyzer executable was not found.";
                return result;
            }

            string tempFile = Path.GetTempFileName();

            try
            {
                File.WriteAllText(tempFile, code ?? string.Empty, new UTF8Encoding(false));

                var startInfo = new ProcessStartInfo
                {
                    FileName = analyzerPath,
                    Arguments = $"\"{tempFile}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(analyzerPath) ?? AppContext.BaseDirectory
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    result.InfrastructureError = "Unable to start flex/bison analyzer process.";
                    return result;
                }

                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                ParseOutput(stdout, result);

                if (process.ExitCode != 0 && result.ParseErrors.Count == 0)
                {
                    result.InfrastructureError = string.IsNullOrWhiteSpace(stderr)
                        ? "flex/bison analyzer failed."
                        : stderr.Trim();
                }
            }
            finally
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch
                {
                    // ignored
                }
            }

            return result;
        }

        private static string ResolveAnalyzerPath()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "flex_bison", "fb_analyzer.exe"),
                Path.Combine(AppContext.BaseDirectory, "fb_analyzer.exe"),
                Path.Combine(Directory.GetCurrentDirectory(), "flex_bison", "fb_analyzer.exe"),
                Path.Combine(Directory.GetCurrentDirectory(), "fb_analyzer.exe")
            };

            foreach (var path in candidates)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private static void ParseOutput(string output, FlexBisonAnalysisResult result)
        {
            if (string.IsNullOrWhiteSpace(output))
            {
                return;
            }

            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("TOKEN|", StringComparison.Ordinal))
                {
                    ParseTokenLine(line, result);
                    continue;
                }

                if (line.StartsWith("ERROR|", StringComparison.Ordinal))
                {
                    ParseErrorLine(line, result);
                }
            }
        }

        private static void ParseTokenLine(string line, FlexBisonAnalysisResult result)
        {
            var fields = SplitEscaped(line);
            if (fields.Count != 6)
            {
                return;
            }

            if (!int.TryParse(fields[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var lineNumber))
            {
                return;
            }

            if (!int.TryParse(fields[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var column))
            {
                return;
            }

            if (!int.TryParse(fields[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out var length))
            {
                return;
            }

            result.Tokens.Add(new Token
            {
                Type = MapTokenType(fields[1]),
                Value = Unescape(fields[2]),
                Line = lineNumber,
                Column = column,
                Length = length
            });
        }

        private static void ParseErrorLine(string line, FlexBisonAnalysisResult result)
        {
            var fields = SplitEscaped(line);
            if (fields.Count != 5)
            {
                return;
            }

            if (!int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var lineNumber))
            {
                lineNumber = 1;
            }

            if (!int.TryParse(fields[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var column))
            {
                column = 1;
            }

            result.ParseErrors.Add(new ParserSyntaxError(
                Unescape(fields[1]),
                lineNumber,
                column,
                Unescape(fields[4])));
        }

        private static List<string> SplitEscaped(string line)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            bool escaped = false;

            foreach (var ch in line)
            {
                if (escaped)
                {
                    current.Append(ch);
                    escaped = false;
                    continue;
                }

                if (ch == '\\')
                {
                    current.Append(ch);
                    escaped = true;
                    continue;
                }

                if (ch == '|')
                {
                    parts.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(ch);
            }

            parts.Add(current.ToString());
            return parts;
        }

        private static string Unescape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var output = new StringBuilder();
            bool escaped = false;

            foreach (var ch in value)
            {
                if (!escaped)
                {
                    if (ch == '\\')
                    {
                        escaped = true;
                    }
                    else
                    {
                        output.Append(ch);
                    }

                    continue;
                }

                escaped = false;

                switch (ch)
                {
                    case 'n':
                        output.Append('\n');
                        break;
                    case 'r':
                        output.Append('\r');
                        break;
                    case 't':
                        output.Append('\t');
                        break;
                    case '|':
                        output.Append('|');
                        break;
                    case '\\':
                        output.Append('\\');
                        break;
                    default:
                        output.Append(ch);
                        break;
                }
            }

            if (escaped)
            {
                output.Append('\\');
            }

            return output.ToString();
        }

        private static TokenType MapTokenType(string tokenType)
        {
            return tokenType switch
            {
                "Keyword" => TokenType.Keyword,
                "Identifier" => TokenType.Identifier,
                "NumberLiteral" => TokenType.NumberLiteral,
                "Operator" => TokenType.Operator,
                "Separator" => TokenType.Separator,
                "Whitespace" => TokenType.Whitespace,
                "Error" => TokenType.Error,
                _ => TokenType.Error
            };
        }
    }
}
