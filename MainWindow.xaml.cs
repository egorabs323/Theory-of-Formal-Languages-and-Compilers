using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Media;

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
            var allErrors = new List<ErrorEntry>();

            foreach (var token in tokens)
            {
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
                    allErrors.Add(new ErrorEntry(
                        token.Value,
                        $"строка {token.Line}, позиция {token.Column}",
                        $"Недопустимый символ: '{token.Value}'",
                        token.Line,
                        token.Column));
                }
            }

            var parser = new Parser(tokens);
            var parseResult = parser.Parse();

            if (parseResult.Errors != null)
            {
                foreach (var err in parseResult.Errors)
                {
                    string fragment = GetErrorFragment(code, err.Line, err.Column);
                    allErrors.Add(new ErrorEntry(
                        fragment,
                        $"строка {err.Line}, позиция {err.Column}",
                        err.Message,
                        err.Line,
                        err.Column));
                }
            }

            TokensDataGrid.ItemsSource = lexemes;
            ErrorsDataGrid.ItemsSource = allErrors;
            ErrorCountTextBlock.Text = allErrors.Count.ToString();

            if (allErrors.Count > 0)
            {
                ResultTabs.SelectedItem = ResultTabs.Items[1];
            }
            else
            {
                var successEntry = new LexemeEntry(0, "УСПЕХ", "Синтаксис корректен",
                    $"Обработано {lexemes.Count} лексем");
                TokensDataGrid.ItemsSource = new[] { successEntry }.Concat(lexemes).ToList();
            }

            UpdateStatusBar();
        }

        private string GetErrorFragment(string code, int line, int column)
        {
            string[] lines = code.Split('\n');
            if (line > 0 && line <= lines.Length)
            {
                string lineText = lines[line - 1];
                if (column > 0 && column <= lineText.Length)
                {
                    int endIndex = Math.Min(column + 10, lineText.Length);
                    return lineText.Substring(column - 1, endIndex - column + 1).Trim();
                }
            }
            return "";
        }

        private void ErrorsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ErrorsDataGrid.SelectedItem is ErrorEntry selectedError)
            {
                if (CodeTabs.SelectedItem is TabItem selectedTab)
                {
                    var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                    if (textBox != null)
                    {
                        try
                        {
                            int lineIndex = selectedError.Line - 1;
                            int columnIndex = selectedError.Column - 1;

                            if (lineIndex >= 0 && lineIndex < textBox.LineCount)
                            {
                                int caretIndex = textBox.GetCharacterIndexFromLineIndex(lineIndex) + columnIndex;

                                if (caretIndex >= 0 && caretIndex <= textBox.Text.Length)
                                {
                                    textBox.CaretIndex = caretIndex;
                                    textBox.Focus();
                                    int fragLength = selectedError.ErrorFragment?.Length ?? 1;
                                    textBox.Select(caretIndex, fragLength);
                                    textBox.ScrollToVerticalOffset(lineIndex * 20);
                                }
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