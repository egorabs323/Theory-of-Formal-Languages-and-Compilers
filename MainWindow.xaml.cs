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
        private Dictionary<TextBox, bool> tabModifiedState = new Dictionary<TextBox, bool>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeInterface();
        }

        private void InitializeInterface()
        {
            if (CodeInputTextBox != null && LineNumbersTextBlock != null)
            {
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

        private void UpdateLineNumbers(TextBox textBox, TextBlock lineNumbers)
        {
            if (textBox == null || lineNumbers == null) return;

            int lineCount = textBox.LineCount;
            lineNumbers.Text = "";
            for (int i = 1; i <= lineCount; i++)
            {
                lineNumbers.Text += i + "\n";
            }
        }

        private void MarkAsModified(TextBox textBox)
        {
            if (tabModifiedState.ContainsKey(textBox))
            {
                tabModifiedState[textBox] = true;
            }
        }

        private void CodeInputTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateStatusBar();
        }

        private void UpdateStatusBar()
        {
            if (CodeInputTextBox != null && StatusCursorPosition != null)
            {
                int line = CodeInputTextBox.GetLineIndexFromCharacterIndex(CodeInputTextBox.CaretIndex) + 1;
                int column = CodeInputTextBox.CaretIndex - CodeInputTextBox.GetCharacterIndexFromLineIndex(line - 1) + 1;
                StatusCursorPosition.Text = $"Ln {line}, Col {column}";
            }
        }
    }

    public class SearchMatch
    {
        public string Match { get; set; }
        public string Position { get; set; }
        public int Length { get; set; }
        public int StartIndex { get; set; }

        public SearchMatch(string match, int line, int col, int length, int startIndex)
        {
            Match = match;
            Position = $"{line}:{col}";
            Length = length;
            StartIndex = startIndex;
        }
    }
}