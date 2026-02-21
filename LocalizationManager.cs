using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace YourNamespace
{
    public static class LocalizationManager
    {
        private static Dictionary<string, Dictionary<string, string>> languageResources = new Dictionary<string, Dictionary<string, string>>
        {
            ["ru-RU"] = new Dictionary<string, string>
            {
                ["AppName"] = "Текстовый редактор",
                ["CodeGroup"] = "Код для выполнения",
                ["ResultGroup"] = "Результат выполнения",
                ["FileMenu"] = "Файл",
                ["EditMenu"] = "Правка",
                ["HelpMenu"] = "Справка",
                ["ViewMenu"] = "Вид",
                ["CreateMenu"] = "Создать",
                ["OpenMenu"] = "Открыть",
                ["SaveMenu"] = "Сохранить",
                ["SaveAsMenu"] = "Сохранить как...",
                ["CloseMenu"] = "Закрыть",
                ["ExitMenu"] = "Выход",
                ["UndoMenu"] = "Отменить",
                ["RedoMenu"] = "Повторить",
                ["CutMenu"] = "Вырезать",
                ["CopyMenu"] = "Копировать",
                ["PasteMenu"] = "Вставить",
                ["DeleteMenu"] = "Удалить",
                ["SelectAllMenu"] = "Выделить все",
                ["UserGuideMenu"] = "Руководство пользователя",
                ["AboutMenu"] = "О программе",
                ["FontSizeMenu"] = "Размер шрифта",
                ["LanguageMenu"] = "Язык интерфейса",
                ["ConfirmationExit"] = "Подтверждение выхода",
                ["ConfirmationClose"] = "Подтверждение закрытия",
                ["UnsavedChangesTab"] = "У вас есть несохраненные изменения во вкладке",
                ["SaveBeforeExit"] = "Сохранить перед выходом?",
                ["SaveBeforeClose"] = "Сохранить перед закрытием?",
                ["UserGuideTitle"] = "Руководство пользователя",
                ["FunctionsList"] = "Реализованные функции:",
                ["CreateOpenSave"] = "- Создание, открытие, сохранение файлов",
                ["TextOperations"] = "- Операции с текстом: отмена/возврат, вырезание, копирование, вставка, удаление",
                ["SelectAll"] = "- Выделение всего текста",
                ["FontSizeChange"] = "- Изменение размера шрифта",
                ["LineNumbering"] = "- Нумерация строк в окне редактирования",
                ["MultiDocs"] = "- Работа с несколькими документами через вкладки",
                ["AdaptiveInterface"] = "- Адаптивный интерфейс с возможностью изменения размеров областей",
                ["AboutTitle"] = "О программе",
                ["StatusModified"] = "Изменено",
                ["StatusReady"] = "Готово",
                ["Ln"] = "Ст",
                ["Col"] = "Ср",
                ["ResultTab_Output"] = "Вывод",
                ["ResultTab_Errors"] = "Ошибки",
                ["ResultTab_Stats"] = "Статистика",
                ["ErrorsGrid_Time"] = "Время",
                ["ErrorsGrid_Level"] = "Уровень",
                ["ErrorsGrid_Module"] = "Модуль",
                ["ErrorsGrid_Message"] = "Сообщение",
                ["ErrorsGrid_Code"] = "Код",
                ["ErrorLevel_Error"] = "Ошибка",
                ["ErrorLevel_Warning"] = "Предупреждение",
                ["ErrorLevel_Info"] = "Инфо",
                ["Errors_Clear"] = "Очистить ошибки",
                ["Errors_Copy"] = "Копировать сообщение",
                ["Errors_Export"] = "Экспорт в файл...",
                ["Errors_NoErrors"] = "Ошибки не обнаружены ✓"
            },
            ["en-US"] = new Dictionary<string, string>
            {
                ["AppName"] = "Text Editor",
                ["CodeGroup"] = "Code for execution",
                ["ResultGroup"] = "Execution result",
                ["FileMenu"] = "File",
                ["EditMenu"] = "Edit",
                ["HelpMenu"] = "Help",
                ["ViewMenu"] = "View",
                ["CreateMenu"] = "Create",
                ["OpenMenu"] = "Open",
                ["SaveMenu"] = "Save",
                ["SaveAsMenu"] = "Save As...",
                ["CloseMenu"] = "Close",
                ["ExitMenu"] = "Exit",
                ["UndoMenu"] = "Undo",
                ["RedoMenu"] = "Redo",
                ["CutMenu"] = "Cut",
                ["CopyMenu"] = "Copy",
                ["PasteMenu"] = "Paste",
                ["DeleteMenu"] = "Delete",
                ["SelectAllMenu"] = "Select All",
                ["UserGuideMenu"] = "User Guide",
                ["AboutMenu"] = "About",
                ["FontSizeMenu"] = "Font Size",
                ["LanguageMenu"] = "Interface Language",
                ["ConfirmationExit"] = "Exit Confirmation",
                ["ConfirmationClose"] = "Close Confirmation",
                ["UnsavedChangesTab"] = "You have unsaved changes in tab",
                ["SaveBeforeExit"] = "Save before exit?",
                ["SaveBeforeClose"] = "Save before close?",
                ["UserGuideTitle"] = "User Guide",
                ["FunctionsList"] = "Implemented functions:",
                ["CreateOpenSave"] = "- Creating, opening, saving files",
                ["TextOperations"] = "- Text operations: undo/redo, cut, copy, paste, delete",
                ["SelectAll"] = "- Select all text",
                ["FontSizeChange"] = "- Change font size",
                ["LineNumbering"] = "- Line numbering in edit window",
                ["MultiDocs"] = "- Working with multiple documents through tabs",
                ["AdaptiveInterface"] = "- Adaptive interface with possibility to change sizes of areas",
                ["AboutTitle"] = "About Program",
                ["StatusModified"] = "Modified",
                ["StatusReady"] = "Ready",
                ["Ln"] = "Ln",
                ["Col"] = "Col",
                ["ResultTab_Output"] = "Output",
                ["ResultTab_Errors"] = "Errors",
                ["ResultTab_Stats"] = "Statistics",
                ["ErrorsGrid_Time"] = "Time",
                ["ErrorsGrid_Level"] = "Level",
                ["ErrorsGrid_Module"] = "Module",
                ["ErrorsGrid_Message"] = "Message",
                ["ErrorsGrid_Code"] = "Code",
                ["ErrorLevel_Error"] = "Error",
                ["ErrorLevel_Warning"] = "Warning",
                ["ErrorLevel_Info"] = "Info",
                ["Errors_Clear"] = "Clear Errors",
                ["Errors_Copy"] = "Copy Message",
                ["Errors_Export"] = "Export to File...",
                ["Errors_NoErrors"] = "No errors detected ✓"
            }
        };

        private static string currentCulture = "ru-RU";

        public static void SwitchLanguage(string cultureCode)
        {
            currentCulture = cultureCode;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
        }

        public static string GetCurrentCulture()
        {
            return currentCulture;
        }

        public static string GetString(string key)
        {
            if (languageResources.ContainsKey(currentCulture) && languageResources[currentCulture].ContainsKey(key))
            {
                return languageResources[currentCulture][key];
            }
            return key;
        }
    }
}