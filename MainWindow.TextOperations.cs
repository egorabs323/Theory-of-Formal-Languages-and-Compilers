using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YourNamespace
{
    public partial class MainWindow
    {
        private TextBox GetActiveTextBox()
        {
            if (CodeTabs.SelectedItem is not TabItem tab)
            {
                return null;
            }

            return FindVisualChild<TextBox>(tab.Content as DependencyObject);
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            var textBox = GetActiveTextBox();
            if (textBox != null && textBox.CanUndo)
            {
                textBox.Focus();
                textBox.Undo();
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            var textBox = GetActiveTextBox();
            if (textBox != null && textBox.CanRedo)
            {
                textBox.Focus();
                textBox.Redo();
            }
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            var textBox = GetActiveTextBox();
            if (textBox != null && textBox.SelectionLength > 0)
            {
                textBox.Focus();
                textBox.Cut();
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var textBox = GetActiveTextBox();
            if (textBox != null && textBox.SelectionLength > 0)
            {
                textBox.Focus();
                textBox.Copy();
            }
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            var textBox = GetActiveTextBox();
            if (textBox != null)
            {
                textBox.Focus();
                textBox.Paste();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var textBox = GetActiveTextBox();
            if (textBox != null && textBox.SelectionLength > 0)
            {
                textBox.Focus();
                textBox.SelectedText = string.Empty;
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            var textBox = GetActiveTextBox();
            if (textBox != null)
            {
                textBox.Focus();
                textBox.SelectAll();
            }
        }

        private void IncreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            if (currentFontSize < 24)
            {
                currentFontSize++;
                UpdateAllTabFontSizes();
            }
        }

        private void DecreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            if (currentFontSize > 8)
            {
                currentFontSize--;
                UpdateAllTabFontSizes();
            }
        }

        private void FontSize_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is string tag && double.TryParse(tag, out double fontSize))
            {
                currentFontSize = fontSize;
                UpdateAllTabFontSizes();
            }
        }
    }
}
