# *Theory-of-Formal-Languages-and-Compilers*

## <a id="lab2"></a> Лабораторная работа 2. Разработка лексического анализатора (сканера)

### 1)Цель работы.
Изучить назначение и принципы работы лексического анализатора в структуре компилятора. Спроектировать алгоритм (диаграмму состояний) и выполнить программную реализацию сканера для выделения лексем из входного текста. Интегрировать разработанный модуль в ранее созданный графический интерфейс языкового процессора.

### 2)Постановка задачи.
Разработать лексический анализатор (сканер) в соответствии с индивидуальным вариантом задания, интегрировать его в приложение из лабораторной работы №1 и обеспечить наглядный вывод результатов.

### 3)Сведения об авторе.
- Выолнил -> Олейник Егор Викторович 
- Группа -> АП-326

### 4) Путь к готовому исполняемому файлу.
- Далее ссылка для скачивания готового проекта
  [Скачать исполняемый файл](https://github.com/egorabs323/Theory-of-Formal-Languages-and-Compilers/releases/download/2лаб/laba1.exe)
  
### 5)Вариант задания.

#### 5.1.Номер варианта, его текстовое описание.

- Вещенственные константы 
- Вариант РГР №52 Объявление вещественной константы с инициализацией на языке Java
![скрин](img/1.png)
#### 5.2.Примеры корректных входных строк

![скрин](img/2.png)

Рисунок 1 - Пример входных строк

#### 5.3.Перечень допустимых лексем.

- "LETTER"
- "DIGIT"
- "WHITESPACE"
- "+"
- "-"
- "="
- ";"

### 6)Диаграмма состояний.

<img width="563" height="640" alt="image" src="https://github.com/user-attachments/assets/0acd6e7a-101d-4de3-b0fd-960401a20874" />

Рисунок 2 - Диаграмма состояний

### 7)Тестовые примеры.

![скрин](img/4.png)

Рисунок 3 - Тест №1

![скрин](img/7.png)

Рисунок 4 - Тест №2

![скрин](img/6.png)

Рисунок 5 - Тест №3

## *Дополнительное задание FLEX&BISON*

### 1) Разработанная грамматика

```
<program>     -> <declaration>
<declaration> -> final double <identifier> = <value> ;
<value>       -> <float> | + <float> | - <float>
<identifier>  -> <letter_or_underscore> {<letter_or_digit_or_underscore>}
<float>       -> <digits> . <digits> [<exponent>]
<exponent>    -> e [ + | - ] <digits> | E [ + | - ] <digits>
<digits>      -> <digit> {<digit>}
```

### 2) Грамматика для FLEX&BISON

`fb_parser.y`:

```
input       : declaration ;
declaration : FINAL DOUBLE IDENTIFIER ASSIGN value SEMICOLON ;
value       : NUMBER | PLUS NUMBER | MINUS NUMBER ;
```

`fb_lexer.l` (лексемы):

```
"final"                -> FINAL
"double"               -> DOUBLE
"="                    -> ASSIGN
";"                    -> SEMICOLON
"+"                    -> PLUS
"-"                    -> MINUS
[0-9]+\.[0-9]+([eE][+-]?[0-9]+)? -> NUMBER
[A-Za-z_][A-Za-z0-9_]*  -> IDENTIFIER
[ \t\r\n]+             -> WHITESPACE (пропуск для синтаксиса)
.                       -> INVALID
```

### 3) Классификация грамматики

Грамматика относится к контекстно-свободным (тип 2 по Хомскому).
Для данного варианта также является праволинейной (регулярной, тип 3).

### 4) Примеры допустимых строк

```
final double PI = 3.141592653589793;
final double X = +1.25;
final double Y = -2.5e-3;
```

### 5) Тестовые примеры

Корректные:

```
final double PI = 3.141592653589793;
final double R = 10.0;
final double G = -6.67e-11;
```

<img width="857" height="548" alt="Снимок экрана 2026-05-09 191528" src="https://github.com/user-attachments/assets/7cdd8d89-e27a-42cf-bedc-5f2f70d20a6e" />

Рисунок 1 - Корректный пример для программы

<img width="1520" height="412" alt="Снимок экрана 2026-05-09 190509" src="https://github.com/user-attachments/assets/8e17261f-bdef-446c-af80-1b981b276874" />

Рисунок 2 - Корректный пример для терминала

Некорректные:

```
final double = 3.14;
final double PI = 3.;
final double PI 3.14;
```

<img width="1404" height="300" alt="Снимок экрана 2026-05-09 191549" src="https://github.com/user-attachments/assets/9998a14b-b014-4194-bab0-9065fe966f71" />

Рисунок 3 - Некорректный пример для программы

<img width="1534" height="293" alt="Снимок экрана 2026-05-09 191350" src="https://github.com/user-attachments/assets/fe9813cf-5dc7-45f0-975d-06f77d2f238e" />

Рисунок 4 - Некорректный пример для терминале
