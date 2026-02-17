using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            this.Closing += MainWindow_Closing;
            FindInterfaceElements();
            InitializeInterface();
            UpdateUIForCurrentLanguage();
            CodeTabs.SelectionChanged += CodeTabs_SelectionChanged;
        }
        private GroupBox _codeGroupBox = null;
        private GroupBox _resultGroupBox = null;
        private void CodeTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CodeTabs.SelectedItem is TabItem selectedTab)
            {
                var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                if (textBox != null && tabModifiedState.ContainsKey(textBox))
                {
                    if (tabModifiedState[textBox])
                        StatusSaveState.Text = LocalizationManager.GetString("StatusModified");
                    else
                        StatusSaveState.Text = LocalizationManager.GetString("StatusReady");
                }
            }
        }
        private void UpdateUIForCurrentLanguage()
        {
            this.Title = LocalizationManager.GetString("AppName");
            UpdateMenuItems();
            UpdateToolbarButtons();
            CodeGroupBox.Header = LocalizationManager.GetString("CodeGroup");
            ResultGroupBox.Header = LocalizationManager.GetString("ResultGroup");
            UpdateStatusBar();
        }
        private void FindInterfaceElements()
        {
            _codeGroupBox = FindGroupBoxByHeader(new[] { "Код для выполнения", "Code for execution" });
            _resultGroupBox = FindGroupBoxByHeader(new[] { "Результат выполнения", "Execution result" });
        }
        private GroupBox FindGroupBoxByHeader(string[] possibleHeaders)
        {
            foreach (var groupBox in FindVisualChildren<GroupBox>(this))
            {
                string hdr = groupBox.Header?.ToString();
                if (string.IsNullOrEmpty(hdr)) continue;

                foreach (var target in possibleHeaders)
                {
                    if (hdr.StartsWith(target.Split(' ')[0]))
                        return groupBox;
                }
            }
            return null;
        }

        private void UpdateMenuItems()
        {
            var mainMenu = FindVisualChild<Menu>(this);
            if (mainMenu == null) return;

            foreach (object item in mainMenu.Items)
            {
                if (item is MenuItem topLevelItem)
                {
                    string hdr = topLevelItem.Header?.ToString();
                    if (string.IsNullOrEmpty(hdr)) continue;

                    if (hdr.Contains("Файл") || hdr.Contains("File"))
                    {
                        topLevelItem.Header = $"_{LocalizationManager.GetString("FileMenu")}";
                        UpdateSubMenuItems(topLevelItem, FileType.SubMenu);
                    }
                    else if (hdr.Contains("Правка") || hdr.Contains("Edit"))
                    {
                        topLevelItem.Header = $"_{LocalizationManager.GetString("EditMenu")}";
                        UpdateSubMenuItems(topLevelItem, FileType.EditMenu);
                    }
                    else if (hdr.Contains("Справка") || hdr.Contains("Help"))
                    {
                        topLevelItem.Header = $"_{LocalizationManager.GetString("HelpMenu")}";
                        UpdateSubMenuItems(topLevelItem, FileType.HelpMenu);
                    }
                    else if (hdr.Contains("Вид") || hdr.Contains("View"))
                    {
                        topLevelItem.Header = $"_{LocalizationManager.GetString("ViewMenu")}";
                        UpdateSubMenuItems(topLevelItem, FileType.ViewMenu);
                    }
                    else if (hdr.Contains("Язык") || hdr.Contains("Language"))
                    {
                        topLevelItem.Header = LocalizationManager.GetString("LanguageMenu");
                        UpdateSubMenuItems(topLevelItem, FileType.LanguageMenu);
                    }
                }
            }
        }

        private enum FileType
        {
            SubMenu, EditMenu, HelpMenu, ViewMenu, LanguageMenu
        }

        private void UpdateSubMenuItems(MenuItem parentItem, FileType type)
        {
            foreach (object obj in parentItem.Items)
            {
                if (obj is Separator) continue;
                if (!(obj is MenuItem)) continue;
                var subItem = (MenuItem)obj;

                string hdr = subItem.Header?.ToString();
                if (string.IsNullOrEmpty(hdr)) continue;
                string cleanHdr = hdr.Replace("_", "");

                switch (type)
                {
                    case FileType.SubMenu:
                        if (cleanHdr.Contains("Создать") || cleanHdr.Contains("Create"))
                            subItem.Header = $"_{LocalizationManager.GetString("CreateMenu")}";
                        else if (cleanHdr.Contains("Открыть") || cleanHdr.Contains("Open"))
                            subItem.Header = $"_{LocalizationManager.GetString("OpenMenu")}";
                        else if (cleanHdr.Contains("Сохранить") && !cleanHdr.Contains("как"))
                            subItem.Header = $"_{LocalizationManager.GetString("SaveMenu")}";
                        else if (cleanHdr.Contains("Сохранить") && cleanHdr.Contains("как"))
                            subItem.Header = LocalizationManager.GetString("SaveAsMenu");
                        else if (cleanHdr.Contains("Закрыть") || cleanHdr.Contains("Close"))
                            subItem.Header = $"_{LocalizationManager.GetString("CloseMenu")}";
                        else if (cleanHdr.Contains("Выход") || cleanHdr.Contains("Exit"))
                            subItem.Header = $"_{LocalizationManager.GetString("ExitMenu")}";
                        break;

                    case FileType.EditMenu:
                        if (cleanHdr.Contains("Отменить") || cleanHdr.Contains("Undo"))
                            subItem.Header = $"_{LocalizationManager.GetString("UndoMenu")}";
                        else if (cleanHdr.Contains("Повторить") || cleanHdr.Contains("Redo"))
                            subItem.Header = $"_{LocalizationManager.GetString("RedoMenu")}";
                        else if (cleanHdr.Contains("Вырезать") || cleanHdr.Contains("Cut")) 
                            subItem.Header = $"_{LocalizationManager.GetString("CutMenu")}";
                        else if (cleanHdr.Contains("Копировать") || cleanHdr.Contains("Copy"))
                            subItem.Header = $"_{LocalizationManager.GetString("CopyMenu")}";
                        else if (cleanHdr.Contains("Вставить") || cleanHdr.Contains("Paste"))
                            subItem.Header = $"_{LocalizationManager.GetString("PasteMenu")}";
                        else if (cleanHdr.Contains("Удалить") || cleanHdr.Contains("Delete"))
                            subItem.Header = $"_{LocalizationManager.GetString("DeleteMenu")}";
                        else if (cleanHdr.Contains("Выделить") || cleanHdr.Contains("Select All"))
                            subItem.Header = $"_{LocalizationManager.GetString("SelectAllMenu")}";
                        break;

                    case FileType.HelpMenu:
                        if (cleanHdr.Contains("Руководство") || cleanHdr.Contains("User Guide"))
                            subItem.Header = $"_{LocalizationManager.GetString("UserGuideMenu")}";
                        else if (cleanHdr.Contains("О программе") || cleanHdr.Contains("About"))
                            subItem.Header = $"_{LocalizationManager.GetString("AboutMenu")}";
                        break;

                    case FileType.ViewMenu:
                        if (cleanHdr.Contains("Размер шрифта") || cleanHdr.Contains("Font Size"))
                            subItem.Header = LocalizationManager.GetString("FontSizeMenu");
                        break;

                    case FileType.LanguageMenu:
                        if (cleanHdr.Contains("Русский"))
                            subItem.Header = "Русский";
                        else if (cleanHdr.Contains("English"))
                            subItem.Header = "English";
                        break;
                }
            }
        }

        private void UpdateToolbarButtons()
        {
            var toolBar = FindVisualChild<ToolBar>(this);
            if (toolBar == null) return;

            foreach (object obj in toolBar.Items)
            {
                if (obj is Button button)
                {
                    string tooltip = button.ToolTip?.ToString();
                    if (string.IsNullOrEmpty(tooltip)) continue;

                    if (tooltip.Contains("Создать") || tooltip.Contains("Create"))
                        button.ToolTip = LocalizationManager.GetString("CreateMenu") + " (Ctrl+N)";
                    else if (tooltip.Contains("Открыть") || tooltip.Contains("Open"))
                        button.ToolTip = LocalizationManager.GetString("OpenMenu") + " (Ctrl+O)";
                    else if (tooltip.Contains("Сохранить") && !tooltip.Contains("как"))
                        button.ToolTip = LocalizationManager.GetString("SaveMenu") + " (Ctrl+S)";
                    else if (tooltip.Contains("Сохранить как"))
                        button.ToolTip = LocalizationManager.GetString("SaveAsMenu");
                    else if (tooltip.Contains("Закрыть") || tooltip.Contains("Close"))
                        button.ToolTip = LocalizationManager.GetString("CloseMenu") + " (Ctrl+W)";
                    else if (tooltip.Contains("Отменить") || tooltip.Contains("Undo"))
                        button.ToolTip = LocalizationManager.GetString("UndoMenu") + " (Ctrl+Z)";
                    else if (tooltip.Contains("Повторить") || tooltip.Contains("Redo"))
                        button.ToolTip = LocalizationManager.GetString("RedoMenu") + " (Ctrl+Y)";
                    else if (tooltip.Contains("Вырезать") || tooltip.Contains("Cut"))
                        button.ToolTip = LocalizationManager.GetString("CutMenu") + " (Ctrl+X)";
                    else if (tooltip.Contains("Копировать") || tooltip.Contains("Copy"))
                        button.ToolTip = LocalizationManager.GetString("CopyMenu") + " (Ctrl+C)";
                    else if (tooltip.Contains("Вставить") || tooltip.Contains("Paste"))
                        button.ToolTip = LocalizationManager.GetString("PasteMenu") + " (Ctrl+V)";
                    else if (tooltip.Contains("Удалить") || tooltip.Contains("Delete"))
                        button.ToolTip = LocalizationManager.GetString("DeleteMenu") + " (Del)";
                    else if (tooltip.Contains("Руководство") || tooltip.Contains("User Guide"))
                        button.ToolTip = LocalizationManager.GetString("UserGuideMenu");
                    else if (tooltip.Contains("Информация") || tooltip.Contains("About"))
                        button.ToolTip = LocalizationManager.GetString("AboutMenu");
                }
            }
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

        private void Language_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is string cultureCode)
            {
                LocalizationManager.SwitchLanguage(cultureCode);
                UpdateUIForCurrentLanguage();
            }
        }
        private void CodeInputTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                int line = textBox.GetLineIndexFromCharacterIndex(textBox.CaretIndex) + 1;
                int column = textBox.CaretIndex - textBox.GetCharacterIndexFromLineIndex(line - 1) + 1;

                string lineText = LocalizationManager.GetString("StatusLine");
                string colText = LocalizationManager.GetString("StatusColumn");

                StatusCursorPosition.Text = $"{lineText} {line}, {colText} {column}";
            }
        }
        private void UpdateStatusBar()
        {
            if (CodeTabs.SelectedItem is TabItem selectedTab)
            {
                var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                bool isModified = (textBox != null && tabModifiedState.ContainsKey(textBox) && tabModifiedState[textBox]);

                if (StatusSaveState != null)
                {
                    StatusSaveState.Text = isModified
                        ? LocalizationManager.GetString("StatusModified")
                        : LocalizationManager.GetString("StatusReady");
                }
            }
        }

        private void MarkAsModified(TextBox textBox)
        {
            if (textBox != null && tabModifiedState.ContainsKey(textBox))
            {
                tabModifiedState[textBox] = true;
                UpdateStatusBar();
            }
        }

        private void MarkAsSaved(TextBox textBox)
        {
            if (textBox != null && tabModifiedState.ContainsKey(textBox))
            {
                tabModifiedState[textBox] = false;
                UpdateStatusBar();
            }
        }

        private void UpdateLineNumbers(TextBox textBox, TextBlock lineNumbers)
        {
            int lineCount = textBox.LineCount;
            lineNumbers.Text = lineCount <= 0 ? "1" : string.Join("\n", Enumerable.Range(1, lineCount));
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ConfirmSaveBeforeExit())
                e.Cancel = true;
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

        private T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj is T t) return t;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T result) return result;
                if (child != null)
                {
                    var found = FindVisualChild<T>(child);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t) yield return t;
                foreach (var grandChild in FindVisualChildren<T>(child))
                    yield return grandChild;
            }
        }

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
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                var selectedTab = CodeTabs.SelectedItem as TabItem;
                if (selectedTab == null ||
                      selectedTab.Header?.ToString().StartsWith(LocalizationManager.GetString("Новый документ")) == true)
                {
                    AddNewTab();
                    selectedTab = CodeTabs.SelectedItem as TabItem;
                }

                if (selectedTab != null)
                {
                    var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                    if (textBox != null)
                    {
                        textBox.Text = System.IO.File.ReadAllText(openFileDialog.FileName);
                        selectedTab.Header = System.IO.Path.GetFileName(openFileDialog.FileName);
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
                    if (System.IO.Path.GetExtension(filePath).ToLower() == ".txt")
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
                selectedTab.Header?.ToString().Contains("Новый документ") == true ||
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
                    textBox.Text = System.IO.File.ReadAllText(filePath);
                    selectedTab.Header = System.IO.Path.GetFileName(filePath);
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
                    System.IO.File.WriteAllText(saveFileDialog.FileName, textBox.Text);
                    tab.Header = System.IO.Path.GetFileName(saveFileDialog.FileName);
                    tab.Tag = saveFileDialog.FileName;

                    if (tabModifiedState.ContainsKey(textBox))
                        tabModifiedState[textBox] = false;
                    UpdateStatusBar();
                }
            }
            else
            {
                System.IO.File.WriteAllText(filePath, textBox.Text);
                tab.Header = System.IO.Path.GetFileName(filePath);
                if (tabModifiedState.ContainsKey(textBox))
                    tabModifiedState[textBox] = false;
                UpdateStatusBar();
            }
            UpdateStatusBar();
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

        private void UpdateAllTabFontSizes()
        {
            foreach (var pair in editors)
            {
                pair.Item1?.SetValue(TextBlock.FontSizeProperty, currentFontSize);
                pair.Item2?.SetValue(TextBox.FontSizeProperty, currentFontSize);
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

        private void UserGuide_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"{LocalizationManager.GetString("FunctionsList")}\n" +
                           $"{LocalizationManager.GetString("CreateOpenSave")}\n" +
                           $"{LocalizationManager.GetString("TextOperations")}\n" +
                           $"{LocalizationManager.GetString("SelectAll")}\n" +
                           $"{LocalizationManager.GetString("FontSizeChange")}\n" +
                           $"{LocalizationManager.GetString("LineNumbering")}\n" +
                           $"{LocalizationManager.GetString("MultiDocs")}\n" +
                           $"{LocalizationManager.GetString("AdaptiveInterface")}",
                           LocalizationManager.GetString("UserGuideTitle"));
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Дисциплина -> Теория формальных языков и компиляторов \n" +
                            "Выполнил -> Студент гр.АП-326 Олейник Е.В.\n" +
                            "Проверил -> Ассистент АСУ Антонянц Е.Н.",
                            LocalizationManager.GetString("AboutTitle"));
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.N:
                        Create_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.O:
                        Open_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.S:
                        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        {
                            SaveAs_Click(null, null);
                        }
                        else
                        {
                            Save_Click(null, null);
                        }
                        e.Handled = true;
                        break;
                    case Key.W:
                        Close_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.Z:
                        Undo_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.Y:
                        Redo_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.X:
                        Cut_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.C:
                        Copy_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.V:
                        Paste_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.A:
                        SelectAll_Click(null, null);
                        e.Handled = true;
                        break;
                }
            }
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt && e.Key == Key.F4)
            {
                Exit_Click(null, null);
                e.Handled = true;
            }
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Add || e.Key == Key.OemPlus)
                {
                    IncreaseFontSize_Click(null, null);
                    e.Handled = true;
                }
                else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
                {
                    DecreaseFontSize_Click(null, null);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Delete)
            {
                Delete_Click(null, null);
                e.Handled = true;
            }
        }
    }
}