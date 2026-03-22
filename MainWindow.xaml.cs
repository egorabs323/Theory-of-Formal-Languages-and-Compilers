using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Linq;

namespace YourNamespace
{
    public partial class MainWindow : Window
    {
        private double currentFontSize = 12;
        private List<Tuple<TextBlock, TextBox>> editors = new List<Tuple<TextBlock, TextBox>>();
        private Dictionary<TextBox, bool> tabModifiedState = new Dictionary<TextBox, bool>();

        public MainWindow()
        {
            InitializeComponent();
            FindInterfaceElements();
            InitializeInterface();
            InitializeErrorGrid();
            CodeTabs.SelectionChanged += CodeTabs_SelectionChanged;
            ResultTabs.SelectionChanged += ResultTabs_SelectionChanged;
            UpdateUIForCurrentLanguage();

        }

        private void InitializeInterface()
        {
            if (CodeInputTextBox != null && LineNumbersTextBlock != null)
            {
                editors.Add(new Tuple<TextBlock, TextBox>(LineNumbersTextBlock, CodeInputTextBox));

                CodeInputTextBox.TextChanged += (s, e) =>
                {
                    UpdateLineNumbers(CodeInputTextBox, LineNumbersTextBlock);
                    MarkAsModified(CodeInputTextBox);
                };

                CodeInputTextBox.SelectionChanged += CodeInputTextBox_SelectionChanged;
                tabModifiedState[CodeInputTextBox] = false;
            }
            UpdateStatusBar();
        }
        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            string code = CodeInputTextBox.Text;
                       var lexer = new Lexer(code);
            var tokens = lexer.Tokenize();

            var lexemes = new List<LexemeEntry>();
            var allErrors = new List<ParserErrorEntry>();

            foreach (var token in tokens)
            {
                //if (token.Type == TokenType.Whitespace) continue; 

                int codeValue = token.Type switch
                {
                    TokenType.Keyword => 1,
                    TokenType.Identifier => 2,
                    TokenType.NumberLiteral => 2,
                    TokenType.Operator => 4,
                    TokenType.Separator => 5,
                    TokenType.Whitespace => 6,
                    TokenType.Error => 7,
                    _ => 99
                };

                string typeString = token.Type switch
                {
                    TokenType.Keyword => "keyword",
                    TokenType.Identifier => "identifier",
                    TokenType.NumberLiteral => "number",
                    TokenType.Operator => "operator",
                    TokenType.Separator => "separator",
                    TokenType.Whitespace => "whitespace",
                    TokenType.Error => "error",
                    _ => "unknown"
                };

                string location = $"строка {token.Line}, {token.Column}-{token.Column + token.Length - 1}";
                lexemes.Add(new LexemeEntry(codeValue, typeString, token.Value, location));

                if (token.Type == TokenType.Error)
                {
                    allErrors.Add(new ParserErrorEntry(
                        $"Недопустимый символ: '{token.Value}'",
                        token.Line, token.Column));
                }
            }
            var parser = new Parser(tokens);
            var parseResult = parser.Parse();

            if (parseResult.Errors != null)
            {
                foreach (var err in parseResult.Errors)
                {
                    allErrors.Add(new ParserErrorEntry(err.Message, err.Line, err.Column));
                }
            }

            TokensDataGrid.ItemsSource = lexemes;

            if (allErrors.Count > 0)
            {
                ErrorsDataGrid.ItemsSource = allErrors;
                ResultTabs.SelectedItem = ResultTabs.Items[1]; 
            }
            else
            {
                ErrorsDataGrid.ItemsSource = null;

                if (parseResult.Success)
                {
                    var successEntry = new LexemeEntry(0, " Успех", "Синтаксис корректен",
                        $"Обработано {lexemes.Count} лексем");
                    TokensDataGrid.ItemsSource = new[] { successEntry }.Concat(lexemes).ToList();
                }
            }

            UpdateStatusBar();
        }
        private void ErrorsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ErrorsDataGrid.SelectedItem is ErrorEntry selectedError)
            {
                var lineMatch = System.Text.RegularExpressions.Regex.Match(selectedError.Message, @"строке\s+(\d+)");
                var colMatch = System.Text.RegularExpressions.Regex.Match(selectedError.Message, @"колонка\s+(\d+)");

                if (lineMatch.Success && colMatch.Success)
                {
                    if (int.TryParse(lineMatch.Groups[1].Value, out int line) &&
                        int.TryParse(colMatch.Groups[1].Value, out int col))
                    {
                        if (CodeTabs.SelectedItem is TabItem selectedTab)
                        {
                            var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                            if (textBox != null)
                            {
                                try
                                {
                                    int caretIndex = textBox.GetCharacterIndexFromLineIndex(line - 1) + col - 1;

                                    if (caretIndex >= 0 && caretIndex <= textBox.Text.Length)
                                    {
                                        textBox.CaretIndex = caretIndex;
                                        textBox.Focus();
                                        textBox.ScrollToVerticalOffset(textBox.GetLineIndexFromCharacterIndex(caretIndex));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Ошибка навигации: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }
        public class ParserErrorEntry
        {
            public string Timestamp { get; set; }
            public string Level { get; set; }
            public string Module { get; set; }
            public string Message { get; set; }
            public string ErrorCode { get; set; }

            public ParserErrorEntry(string message, int line, int column)
            {
                Timestamp = DateTime.Now.ToString("HH:mm:ss");
                Level = "Error";
                Module = "Parser";
                Message = $"Строка {line}, поз. {column}: {message}";
                ErrorCode = "SYN";
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
}