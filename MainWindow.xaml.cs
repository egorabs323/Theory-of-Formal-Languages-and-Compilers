using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace YourNamespace
{
    public partial class MainWindow : Window
    {
        private bool hasUnsavedChanges = false;
        private double currentFontSize = 12;

        public MainWindow()
        {
            InitializeComponent();
            CodeInputTextBox.TextChanged += CodeInputTextBox_TextChanged;
            this.Closing += MainWindow_Closing;
        }

        private void CodeInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            hasUnsavedChanges = true;
            UpdateLineNumbers();
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
            if (hasUnsavedChanges && !string.IsNullOrEmpty(CodeInputTextBox.Text))
            {
                MessageBoxResult result = MessageBox.Show(
                    "У вас есть несохраненные изменения. Сохранить перед выходом?",
                    "Подтверждение выхода",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save_Click(null, null);
                        return true;
                    case MessageBoxResult.No:
                        return true;
                    case MessageBoxResult.Cancel:
                        return false;
                    default:
                        return false;
                }
            }
            return true;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmSaveBeforeExit()) return;

            CodeInputTextBox.Clear();
            ResultOutputTextBox.Clear();
            UpdateLineNumbers();
            hasUnsavedChanges = false;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmSaveBeforeExit()) return;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                CodeInputTextBox.Text = System.IO.File.ReadAllText(openFileDialog.FileName);
                UpdateLineNumbers();
                hasUnsavedChanges = false;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(saveFileDialog.FileName, CodeInputTextBox.Text);
                hasUnsavedChanges = false;
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(saveFileDialog.FileName, CodeInputTextBox.Text);
                hasUnsavedChanges = false;
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
            if (CodeInputTextBox.CaretIndex > 0)
                CodeInputTextBox.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (CodeInputTextBox.CaretIndex > 0)
                CodeInputTextBox.Redo();
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            if (CodeInputTextBox.SelectionLength > 0)
                CodeInputTextBox.Cut();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (CodeInputTextBox.SelectionLength > 0)
                CodeInputTextBox.Copy();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            CodeInputTextBox.Paste();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (CodeInputTextBox.SelectionLength > 0)
                CodeInputTextBox.SelectedText = string.Empty;
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            CodeInputTextBox.SelectAll();
        }

        private Slider GetFontSizeSlider()
        {
            var toolBar = FindName("FontSizeSlider") as Slider;
            if (toolBar != null) return toolBar;
            foreach (var child in LogicalTreeHelper.GetChildren(this))
            {
                if (child is ToolBarTray tray)
                {
                    foreach (var tb in tray.ToolBars)
                    {
                        var slider = tb.Template.FindName("FontSizeSlider", tb) as Slider;
                        if (slider != null) return slider;
                    }
                }
            }
            return null;
        }

        private void IncreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            var slider = GetFontSizeSlider();
            if (slider != null && currentFontSize < 24)
            {
                currentFontSize += 1;
                slider.Value = currentFontSize;
            }
            else if (currentFontSize < 24)
            {
                currentFontSize += 1;
                CodeInputTextBox.FontSize = currentFontSize;
                ResultOutputTextBox.FontSize = currentFontSize;
            }
        }

        private void DecreaseFontSize_Click(object sender, RoutedEventArgs e)
        {
            var slider = GetFontSizeSlider();
            if (slider != null && currentFontSize > 8)
            {
                currentFontSize -= 1;
                slider.Value = currentFontSize;
            }
            else if (currentFontSize > 8)
            {
                currentFontSize -= 1;
                CodeInputTextBox.FontSize = currentFontSize;
                ResultOutputTextBox.FontSize = currentFontSize;
            }
        }

        private void FontSize_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem?.Tag != null)
            {
                double fontSize = double.Parse(menuItem.Tag.ToString());
                currentFontSize = fontSize;

                var slider = GetFontSizeSlider();
                if (slider != null)
                {
                    slider.Value = currentFontSize;
                }

                CodeInputTextBox.FontSize = currentFontSize;
                ResultOutputTextBox.FontSize = currentFontSize;
            }
        }

        private void UpdateLineNumbers()
        {
            int lineCount = CodeInputTextBox.LineCount;
            string lineNumbers = "";
            for (int i = 1; i <= lineCount; i++)
            {
                lineNumbers += i.ToString() + "\n";
            }
            LineNumbersTextBlock.Text = lineNumbers.TrimEnd('\n');
        }

        private void UserGuide_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Реализованные функции:\n" +
                           "- Создание, открытие, сохранение файлов\n" +
                           "- Операции с текстом: отмена/возврат, вырезание, копирование, вставка, удаление\n" +
                           "- Выделение всего текста\n" +
                           "- Изменение размера шрифта\n" +
                           "- Нумерация строк в окне редактирования\n" +
                           "- Адаптивный интерфейс с возможностью изменения размеров областей",
                           "Руководство пользователя");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Олейник Е.В. АП-326");
        }
    }
}