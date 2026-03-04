using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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

            var output = new System.Text.StringBuilder();
            foreach (var token in tokens)
            {
                if (token.Type != TokenType.Whitespace)
                {
                    output.AppendLine(token.ToString());
                }
            }
            ResultOutputTextBox.Text = output.ToString();

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
    }
}