using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

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

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Whitespace) continue; 

                int codeValue = token.Type switch
                {
                    TokenType.Keyword => 14, 
                    TokenType.Identifier => 2, 
                    TokenType.NumberLiteral => 1, 
                    TokenType.Operator => 10, 
                    TokenType.Separator => 16, 
                    _ => 99
                };

                string typeString = token.Type switch
                {
                    TokenType.Keyword => "keyword",
                    TokenType.Identifier => "identifier",
                    TokenType.NumberLiteral => "number",
                    TokenType.Operator => "operator",
                    TokenType.Separator => "separator",
                    TokenType.Error => "error",
                    TokenType.Whitespace => "whitespace",
                    _ => "unknown"
                };

                string location = $"строка {token.Line}, {token.Column}-{token.Column + token.Length - 1}";

                lexemes.Add(new LexemeEntry(codeValue, typeString, token.Value, location));
            }

            TokensDataGrid.ItemsSource = lexemes;
            ErrorsDataGrid.ItemsSource = null;
            var errors = tokens.FindAll(t => t.Type == TokenType.Error);
            if (errors.Count > 0)
            {
                var errorList = new List<ErrorEntry>();
                foreach (var err in errors)
                {
                    errorList.Add(new ErrorEntry(
                        LocalizationManager.GetString("ErrorLevel_Error"),
                        "Лексер",
                        $"Недопустимый символ: '{err.Value}' на строке {err.Line}, колонка {err.Column}",
                        "LEX"
                    ));
                }
                ErrorsDataGrid.ItemsSource = errorList;
            }
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
    }
}