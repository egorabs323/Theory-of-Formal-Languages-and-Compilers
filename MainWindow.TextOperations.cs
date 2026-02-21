using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace YourNamespace
{
    public partial class MainWindow
    {
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var richTextBox = FindVisualChild<RichTextBox>(tab.Content as DependencyObject);
                if (richTextBox != null && richTextBox.CanUndo)
                    richTextBox.Undo();
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var richTextBox = FindVisualChild<RichTextBox>(tab.Content as DependencyObject);
                if (richTextBox != null && richTextBox.CanRedo)
                    richTextBox.Redo();
            }
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var richTextBox = FindVisualChild<RichTextBox>(tab.Content as DependencyObject);
                if (richTextBox != null && !richTextBox.Selection.IsEmpty)
                    richTextBox.Cut();
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var richTextBox = FindVisualChild<RichTextBox>(tab.Content as DependencyObject);
                if (richTextBox != null && !richTextBox.Selection.IsEmpty)
                    richTextBox.Copy();
            }
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var richTextBox = FindVisualChild<RichTextBox>(tab.Content as DependencyObject);
                if (richTextBox != null)
                    richTextBox.Paste();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var richTextBox = FindVisualChild<RichTextBox>(tab.Content as DependencyObject);
                if (richTextBox != null && !richTextBox.Selection.IsEmpty)
                {
                    richTextBox.Selection.Text = string.Empty;
                }
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var richTextBox = FindVisualChild<RichTextBox>(tab.Content as DependencyObject);
                if (richTextBox != null)
                    richTextBox.SelectAll();
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