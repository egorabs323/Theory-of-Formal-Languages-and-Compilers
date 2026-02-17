using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace YourNamespace
{
    public partial class MainWindow
    {
        private GroupBox _codeGroupBox = null;
        private GroupBox _resultGroupBox = null;

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

        private void UpdateUIForCurrentLanguage()
        {
            this.Title = LocalizationManager.GetString("AppName");
            UpdateMenuItems();
            UpdateToolbarButtons();
            CodeGroupBox.Header = LocalizationManager.GetString("CodeGroup");
            ResultGroupBox.Header = LocalizationManager.GetString("ResultGroup");
            UpdateStatusBar();

            if (CodeTabs.SelectedItem is TabItem selectedTab)
            {
                var textBox = FindVisualChild<TextBox>(selectedTab.Content as DependencyObject);
                if (textBox != null)
                {
                    UpdateCursorPosition(textBox);
                }
            }
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

                    UpdateCursorPosition(textBox);
                }
            }
        }

        private void CodeInputTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                UpdateCursorPosition(textBox);
            }
        }

        private void UpdateCursorPosition(TextBox textBox)
        {
            if (textBox == null || StatusCursorPosition == null) return;

            int line = textBox.GetLineIndexFromCharacterIndex(textBox.CaretIndex) + 1;
            int column = textBox.CaretIndex - textBox.GetCharacterIndexFromLineIndex(line - 1) + 1;

            string lineText = LocalizationManager.GetString("StatusLine");
            string colText = LocalizationManager.GetString("StatusColumn");

            StatusCursorPosition.Text = $"{lineText} {line}, {colText} {column}";
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

        private void UpdateAllTabFontSizes()
        {
            foreach (var pair in editors)
            {
                pair.Item1?.SetValue(TextBlock.FontSizeProperty, currentFontSize);
                pair.Item2?.SetValue(TextBox.FontSizeProperty, currentFontSize);
            }
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
    }
}