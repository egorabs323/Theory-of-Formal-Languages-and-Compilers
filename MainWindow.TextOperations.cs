using System;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace YourNamespace
{
    public partial class MainWindow
    {
        private ObservableCollection<SearchMatch> _searchResults = new ObservableCollection<SearchMatch>();
        private void SearchWordsM_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch(@"\b[mM][a-zA-Z]*\b", "Поиск слов на m/M");
        }

        private void SearchEthereum_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch(@"0x[a-fA-F0-9]{40}", "Поиск Ethereum адресов");
        }
        //0x742d35Cc6634C0532925a3b844Bc9e7595f8fE00 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B 0x5aAeb6053F3E94C9b9A09f33669435E7Ef1BeAed
        //0x742d35Cc6634C0532925a3b844Bc9e7595f8fE00 0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B 0x5aAeb6053F3E94C9b9A09f33669435E7Ef1BeAed

        private void SearchHTMLTags_Click(object sender, RoutedEventArgs e)
        {
            // <div id="main" class="abx">
            string pattern = @"<[a-zA-Z][a-zA-Z0-9]*\s[^>]+>";

            PerformSearch(pattern, "Поиск HTML тега");
        }

        private void PerformSearch(string pattern, string searchName)
        {
            string text = CodeInputTextBox.Text;
            _searchResults.Clear();

            try
            {
                var matches = Regex.Matches(text, pattern, RegexOptions.Multiline);

                foreach (Match m in matches)
                {
                    var (line, col) = GetLineColumn(text, m.Index);
                    _searchResults.Add(new SearchMatch(m.Value, line, col, m.Length, m.Index));
                }

                SearchResultsGrid.ItemsSource = _searchResults;
                SearchCountText.Text = $"Найдено: {_searchResults.Count} ({searchName})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска:\n{ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            _searchResults.Clear();
            SearchResultsGrid.ItemsSource = null;
            SearchCountText.Text = "Найдено: 0";
            CodeInputTextBox.Select(0, 0);
        }

        private void SearchResultsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsGrid.SelectedItem is SearchMatch result && CodeInputTextBox != null)
            {
                CodeInputTextBox.Focus();
                CodeInputTextBox.Select(result.StartIndex, result.Length);
                CodeInputTextBox.SelectionBrush = new SolidColorBrush(Color.FromRgb(155, 89, 182));
                CodeInputTextBox.SelectionOpacity = 0.6;

                double lineHeight = 14;
                var linesBefore = CodeInputTextBox.Text.Substring(0, result.StartIndex).Count(c => c == '\n');
                CodeInputTextBox.ScrollToVerticalOffset(Math.Max(0, linesBefore * lineHeight - 100));
            }
        }

        private (int line, int col) GetLineColumn(string text, int index)
        {
            int line = 1, col = 1;
            for (int i = 0; i < index && i < text.Length; i++)
            {
                if (text[i] == '\n') { line++; col = 1; }
                else { col++; }
            }
            return (line, col);
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (CodeInputTextBox != null && CodeInputTextBox.CanUndo)
                CodeInputTextBox.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (CodeInputTextBox != null && CodeInputTextBox.CanRedo)
                CodeInputTextBox.Redo();
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            if (CodeInputTextBox != null && !string.IsNullOrEmpty(CodeInputTextBox.SelectedText))
                CodeInputTextBox.Cut();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (CodeInputTextBox != null && !string.IsNullOrEmpty(CodeInputTextBox.SelectedText))
                CodeInputTextBox.Copy();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            if (CodeInputTextBox != null)
                CodeInputTextBox.Paste();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (CodeInputTextBox != null && !string.IsNullOrEmpty(CodeInputTextBox.SelectedText))
                CodeInputTextBox.SelectedText = string.Empty;
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (CodeInputTextBox != null)
                CodeInputTextBox.SelectAll();
        }
    }
}