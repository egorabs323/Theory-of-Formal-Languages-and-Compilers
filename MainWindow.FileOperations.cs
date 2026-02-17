using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace YourNamespace
{
    public partial class MainWindow
    {
        private void Create_Click(object sender, RoutedEventArgs e) => AddNewTab();

        private void AddNewTab()
        {
            var tab = new TabItem
            {
                Header = $"{LocalizationManager.GetString("Новый документ")} {CodeTabs.Items.Count + 1}"
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

            textBox.TextChanged += (s, e) =>
            {
                UpdateLineNumbers(textBox, lineNumbersBlock);
                MarkAsModified(textBox);
            };

            textBox.SelectionChanged += (s, e) => UpdateCursorPosition(textBox);

            Grid.SetColumn(lineNumbersBorder, 0);
            Grid.SetColumn(textBox, 1);

            grid.Children.Add(lineNumbersBorder);
            grid.Children.Add(textBox);

            tab.Content = grid;
            CodeTabs.Items.Add(tab);
            CodeTabs.SelectedItem = tab;

            editors.Add(new Tuple<TextBlock, TextBox>(lineNumbersBlock, textBox));
            UpdateLineNumbers(textBox, lineNumbersBlock);
            tabModifiedState[textBox] = false;
            UpdateCursorPosition(textBox);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                var selectedTab = CodeTabs.SelectedItem as TabItem;
                if (selectedTab == null ||
                      selectedTab.Header?.ToString().StartsWith(LocalizationManager.GetString("NewDocument")) == true)
                {
                    AddNewTab();
                    selectedTab = CodeTabs.SelectedItem as TabItem;
                }

                if (selectedTab != null)
                {
                    var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                    if (textBox != null)
                    {
                        textBox.Text = File.ReadAllText(openFileDialog.FileName);
                        selectedTab.Header = Path.GetFileName(openFileDialog.FileName);
                        selectedTab.Tag = openFileDialog.FileName;
                    }
                }
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0];
                    if (Path.GetExtension(filePath).ToLower() == ".txt")
                    {
                        OpenFile(filePath);
                    }
                }
            }
        }

        private void OpenFile(string filePath)
        {
            var selectedTab = CodeTabs.SelectedItem as TabItem;
            if (selectedTab == null ||
                selectedTab.Header?.ToString().Contains("NewDocument") == true ||
                selectedTab.Header?.ToString().Contains("Create") == true)
            {
                AddNewTab();
                selectedTab = CodeTabs.SelectedItem as TabItem;
            }

            if (selectedTab != null)
            {
                var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                if (textBox != null)
                {
                    textBox.Text = File.ReadAllText(filePath);
                    selectedTab.Header = Path.GetFileName(filePath);
                    selectedTab.Tag = filePath;
                    MarkAsSaved(textBox);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
                SaveTab(tab, false);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
                SaveTab(tab, true);
        }

        private void SaveTab(TabItem tab, bool forceSaveAs = false)
        {
            var textBox = FindVisualChild<TextBox>(tab.Content as DependencyObject);
            if (textBox == null) return;

            string filePath = tab.Tag as string;

            if (forceSaveAs || string.IsNullOrEmpty(filePath))
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, textBox.Text);
                    tab.Header = Path.GetFileName(saveFileDialog.FileName);
                    tab.Tag = saveFileDialog.FileName;

                    if (tabModifiedState.ContainsKey(textBox))
                        tabModifiedState[textBox] = false;
                    UpdateStatusBar();
                }
            }
            else
            {
                File.WriteAllText(filePath, textBox.Text);
                tab.Header = Path.GetFileName(filePath);
                if (tabModifiedState.ContainsKey(textBox))
                    tabModifiedState[textBox] = false;
                UpdateStatusBar();
            }
            UpdateStatusBar();
        }

        private bool ConfirmSaveBeforeExit()
        {
            foreach (TabItem tab in CodeTabs.Items.OfType<TabItem>())
            {
                var textBox = FindVisualChild<TextBox>(tab.Content as DependencyObject);
                if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
                {
                    MessageBoxResult result = MessageBox.Show(
                        $"{LocalizationManager.GetString("UnsavedChangesTab")} '{tab.Header}'. {LocalizationManager.GetString("SaveBeforeExit")}",
                        LocalizationManager.GetString("ConfirmationExit"),
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            SaveTab(tab, false);
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

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ConfirmSaveBeforeExit())
                e.Cancel = true;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmSaveBeforeExit())
                Application.Current.Shutdown();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem tab)
            {
                var textBox = FindVisualChild<TextBox>(tab.Content as DependencyObject);
                if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
                {
                    MessageBoxResult result = MessageBox.Show(
                        $"{LocalizationManager.GetString("UnsavedChangesTab")} '{tab.Header}'. {LocalizationManager.GetString("SaveBeforeClose")}",
                        LocalizationManager.GetString("ConfirmationClose"),
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            SaveTab(tab, false);
                            goto RemoveTab;
                        case MessageBoxResult.No:
                        RemoveTab:
                            CodeTabs.Items.Remove(tab);
                            break;
                        case MessageBoxResult.Cancel:
                            return;
                        default:
                            return;
                    }
                }
                else
                {
                    CodeTabs.Items.Remove(tab);
                }

                if (CodeTabs.Items.Count == 0)
                    AddNewTab();
            }
        }
    }
}