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
        private readonly FiniteAutomaton _ethereumAutomaton = FiniteAutomaton.CreateEthereumAddressAutomaton();
        private ObservableCollection<SearchMatch> _searchResults = new ObservableCollection<SearchMatch>();
        private void SearchWordsM_Click(object sender, RoutedEventArgs e)
        {
            UpdateSearchModeInfo("Используется регулярное выражение для поиска слов, начинающихся на m/M.");
            PerformSearch(@"\b[mM][a-zA-Z]*\b", "Поиск слов на m/M");
        }

        private void SearchEthereum_Click(object sender, RoutedEventArgs e)
        {
            PerformAutomatonSearch("Поиск Ethereum адресов");
        }
        //0x742d35Cc6634C0532925a3b844Bc9e7595f8fE00 

        private void SearchHTMLTags_Click(object sender, RoutedEventArgs e)
        {
            // <div id="main" class="abx">
            string pattern = @"<[a-zA-Z][a-zA-Z0-9]*\s[^>]+>";

            UpdateSearchModeInfo("Используется регулярное выражение для поиска HTML-тегов с атрибутами.");
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

        private void PerformAutomatonSearch(string searchName)
        {
            string text = CodeInputTextBox.Text;
            _searchResults.Clear();

            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    SearchResultsGrid.ItemsSource = _searchResults;
                    SearchCountText.Text = $"Найдено: 0 ({searchName}, граф автомата)";
                    UpdateSearchModeInfo(_ethereumAutomaton.BuildGraphDescription());
                    MessageBox.Show("Введите текст для анализа.", "Поиск Ethereum",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var matches = _ethereumAutomaton.FindMatches(text);

                foreach (AutomatonMatch match in matches)
                {
                    var (line, col) = GetLineColumn(text, match.StartIndex);
                    _searchResults.Add(new SearchMatch(match.Value, line, col, match.Length, match.StartIndex));
                }

                SearchResultsGrid.ItemsSource = _searchResults;
                SearchCountText.Text = $"Найдено: {_searchResults.Count} ({searchName}, граф автомата)";
                UpdateSearchModeInfo(_ethereumAutomaton.BuildGraphDescription());

                if (_searchResults.Count == 0)
                {
                    MessageBox.Show(
                        "Совпадения не найдены.\n\nПример корректного адреса:\n0x742d35Cc6634C0532925a3b844Bc9e7595f8fE00",
                        "Поиск Ethereum",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска автоматом:\n{ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            _searchResults.Clear();
            SearchResultsGrid.ItemsSource = null;
            SearchCountText.Text = "Найдено: 0";
            UpdateSearchModeInfo("Выберите тип поиска. Для Ethereum-адресов используется поиск по графу конечного автомата.");
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

        private void UpdateSearchModeInfo(string description)
        {
            if (SearchModeInfoText != null)
            {
                SearchModeInfoText.Text = description;
            }
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
