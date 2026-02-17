using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YourNamespace
{
    public partial class MainWindow
    {
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var textBox = FindVisualChild<TextBox>(tab.Content as DependencyObject);
                if (textBox != null && textBox.CanUndo)
                    textBox.Undo();
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var textBox = FindVisualChild<TextBox>(tab.Content as DependencyObject);
                if (textBox != null && textBox.CanRedo)
                    textBox.Redo();
            }
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var textBox = FindVisualChild<TextBox>(tab.Content as DependencyObject);
                if (textBox != null && textBox.SelectionLength > 0)
                    textBox.Cut();
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var textBox = FindVisualChild<TextBox>(tab.Content as DependencyObject);
                if (textBox != null && textBox.SelectionLength > 0)
                    textBox.Copy();
            }
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var textBox = FindVisualChild<TextBox>(tab.Content as DependencyObject);
                if (textBox != null)
                    textBox.Paste();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var textBox = FindVisualChild<TextBox>(tab.Content as DependencyObject);
                if (textBox != null && textBox.SelectionLength > 0)
                    textBox.SelectedText = string.Empty;
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var textBox = FindVisualChild<TextBox>(tab.Content as DependencyObject);
                if (textBox != null)
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