using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace YourNamespace
{
    public partial class MainWindow
    {
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            CodeInputTextBox.Clear();
            UpdateLineNumbers(CodeInputTextBox, LineNumbersTextBlock);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string text = File.ReadAllText(openFileDialog.FileName);
                    CodeInputTextBox.Text = text;
                    UpdateLineNumbers(CodeInputTextBox, LineNumbersTextBlock);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка открытия файла: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveAs_Click(sender, e);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, CodeInputTextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения файла: {ex.Message}", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        
    }
}