using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace YourNamespace
{
    public partial class MainWindow : Window
    {
        private double currentFontSize = 12;
        private List<Tuple<TextBlock, TextBox>> editors = new List<Tuple<TextBlock, TextBox>>();

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;

            var firstTabContent = (CodeTabs.Items[0] as TabItem)?.Content as Grid;
            if (firstTabContent != null)
            {
                var textBox = FindVisualChild<TextBox>(firstTabContent);
                var lineNumbersBlock = FindVisualChild<TextBlock>(firstTabContent);
                if (textBox != null && lineNumbersBlock != null)
                {
                    editors.Add(new Tuple<TextBlock, TextBox>(lineNumbersBlock, textBox));
                    textBox.TextChanged += CodeInputTextBox_TextChanged;
                }
            }
        }

        private void CodeInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            var pair = editors.FirstOrDefault(x => x.Item2 == textBox);
            if (pair != null && pair.Item1 != null)
            {
                UpdateLineNumbers(pair.Item2, pair.Item1);
            }
        }

        private void UpdateLineNumbers(TextBox textBox, TextBlock lineNumbers)
        {
            int lineCount = textBox.LineCount;
            if (lineCount <= 0)
            {
                lineNumbers.Text = "1"; // если не сломается 
                return;
            }

            string lineNumbersText = string.Join("\n", Enumerable.Range(1, lineCount));
            lineNumbers.Text = lineNumbersText;
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ConfirmSaveBeforeExit())
            {
                e.Cancel = true;
            }
        }

        private bool ConfirmSaveBeforeExit()
        {
            foreach (TabItem tab in CodeTabs.Items)
            {
                var textBox = FindVisualChild<TextBox>(tab.Content as DependencyObject);
                if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
                {
                    MessageBoxResult result = MessageBox.Show(
                        $"У вас есть несохраненные изменения во вкладке '{tab.Header}'. Сохранить перед выходом?",
                        "Подтверждение выхода",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            SaveTab(tab);
                            break;
                        case MessageBoxResult.No:
                            break;
                        case MessageBoxResult.Cancel:
                            return false;
                        default:
                            return false;
                    }
                }
            }
            return true;
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab();
        }

        private void AddNewTab()
        {
            var tab = new TabItem
            {
                Header = $"Новый документ {CodeTabs.Items.Count + 1}"
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var lineNumbersBlock = new TextBlock
            {
                Padding = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = Brushes.Gray,
                FontFamily = new FontFamily("Consolas"),
                FontSize = currentFontSize
            };

            var lineNumbersBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 60, 60, 60)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 155, 89, 182)),
                BorderThickness = new Thickness(0, 0, 1, 0)
            };
            lineNumbersBorder.Child = lineNumbersBlock;

            var textBox = new TextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.NoWrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = new SolidColorBrush(Color.FromArgb(255, 44, 44, 44)),
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Consolas"),
                FontSize = currentFontSize
            };

            textBox.TextChanged += (s, e) => UpdateLineNumbers(textBox, lineNumbersBlock);

            Grid.SetColumn(lineNumbersBorder, 0);
            Grid.SetColumn(textBox, 1);

            grid.Children.Add(lineNumbersBorder);
            grid.Children.Add(textBox);

            tab.Content = grid;
            CodeTabs.Items.Add(tab);
            CodeTabs.SelectedItem = tab;

            editors.Add(new Tuple<TextBlock, TextBox>(lineNumbersBlock, textBox));
            UpdateLineNumbers(textBox, lineNumbersBlock);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                var selectedTab = CodeTabs.SelectedItem as TabItem;
                if (selectedTab == null || selectedTab.Header.ToString().StartsWith("Новый документ"))
                {
                    AddNewTab();
                    selectedTab = CodeTabs.SelectedItem as TabItem;
                }

                var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                if (textBox != null)
                {
                    textBox.Text = System.IO.File.ReadAllText(openFileDialog.FileName);
                    selectedTab.Header = System.IO.Path.GetFileName(openFileDialog.FileName);
                    selectedTab.Tag = openFileDialog.FileName; 
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = CodeTabs.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                SaveTab(selectedTab, false);
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = CodeTabs.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                SaveTab(selectedTab, true); 
            }
        }

        private void SaveTab(TabItem tab, bool forceSaveAs = false)
        {
            var textBox = FindVisualChild<TextBox>(tab.Content as DependencyObject);
            if (textBox != null)
            {
                string filePath = tab.Tag as string; 

                if (forceSaveAs || string.IsNullOrEmpty(filePath))
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        System.IO.File.WriteAllText(saveFileDialog.FileName, textBox.Text);
                        tab.Header = System.IO.Path.GetFileName(saveFileDialog.FileName);
                        tab.Tag = saveFileDialog.FileName; 
                    }
                }
                else
                {
                    System.IO.File.WriteAllText(filePath, textBox.Text);
                    tab.Header = System.IO.Path.GetFileName(filePath);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmSaveBeforeExit())
            {
                Application.Current.Shutdown();
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = CodeTabs.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                if (textBox != null && textBox.CaretIndex > 0)
                    textBox.Undo();
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = CodeTabs.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                if (textBox != null && textBox.CaretIndex > 0)
                    textBox.Redo();
            }
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = CodeTabs.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                if (textBox != null && textBox.SelectionLength > 0)
                    textBox.Cut();
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = CodeTabs.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                if (textBox != null && textBox.SelectionLength > 0)
                    textBox.Copy();
            }
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = CodeTabs.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                if (textBox != null)
                    textBox.Paste();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = CodeTabs.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                if (textBox != null && textBox.SelectionLength > 0)
                    textBox.SelectedText = string.Empty;
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = CodeTabs.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                if (textBox != null)
                    textBox.SelectAll();
            }
        }

        private void IncreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            if (currentFontSize < 24)
            {
                currentFontSize += 1;
                UpdateAllTabFontSizes();
            }
        }

        private void DecreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            if (currentFontSize > 8)
            {
                currentFontSize -= 1;
                UpdateAllTabFontSizes();
            }
        }

        private void UpdateAllTabFontSizes()
        {
            foreach (var pair in editors)
            {
                if (pair.Item1 != null) pair.Item1.FontSize = currentFontSize;
                if (pair.Item2 != null) pair.Item2.FontSize = currentFontSize;
            }
        }

        private void FontSize_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag != null)
            {
                if (double.TryParse(menuItem.Tag.ToString(), out double fontSize))
                {
                    currentFontSize = fontSize;
                    UpdateAllTabFontSizes();
                }
            }
        }

        private void UserGuide_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Реализованные функции:\n" +
                           "- Создание, открытие, сохранение файлов\n" +
                           "- Операции с текстом: отмена/возврат, вырезание, копирование, вставка, удаление\n" +
                           "- Выделение всего текста\n" +
                           "- Изменение размера шрифта\n" +
                           "- Нумерация строк в окне редактирования\n" +
                           "- Работа с несколькими документами через вкладки\n" +
                           "- Адаптивный интерфейс с возможностью изменения размеров областей",
                           "Руководство пользователя");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Дисциплина -> Теория формальных языков и компиляторов \n" +
                            "Выполнил -> Студент гр.АП-326 Олейник Е.В.\n" +
                            "Проверил -> Ассистент АСУ Антонянц Е.Н.");
        }
    }
}