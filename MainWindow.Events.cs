using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YourNamespace
{
    public partial class MainWindow
    {
        private void Language_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is string cultureCode)
            {
                
            }
        }

        private void UserGuide_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Руководство пользователя", "Справка");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Дисциплина -> Теория формальных языков и компиляторов \n" +
                            "Выполнил -> Студент гр.АП-326 Олейник Е.В.\n" +
                            "Проверил -> Ассистент АСУ Антонянц Е.Н.",
                            "О программе");
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
            else if (e.Key == Key.Delete)
            {
                Delete_Click(null, null);
                e.Handled = true;
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
        }
    }
}