# АНАЛИЗ ДАННЫХ И ИСКУССТВЕННЫЙ ИНТЕЛЛЕКТ [in GameDev]
Отчет по лабораторной работе #1 выполнил:
- Казанцев Михаил Максимович
- РИ-210914
Отметка о выполнении заданий (заполняется студентом):

| Задание | Выполнение | Баллы |
| ------ | ------ | ------ |
| Задание 1 | # | - |
| Задание 2 | # | - |
| Задание 3 | # | - |

знак "*" - задание выполнено; знак "#" - задание не выполнено;

Работу проверили:
-

## Структура отчета

- Данные о работе: название работы, фио, группа, выполненные задания.
- Цель работы.
- "Hello world" на Unity и Python
- Задание 1.
- Код реализации выполнения задания. Визуализация результатов выполнения (если применимо).
- Задание 2.
- Код реализации выполнения задания. Визуализация результатов выполнения (если применимо).
- Задание 3.
- Код реализации выполнения задания. Визуализация результатов выполнения (если применимо).
- Выводы.

## "Hello world" на Unity и Python

![da_lab1_python1](images/da_lab1_python1.png?raw=true "Python")
![da_lab1_unity1](images/da_lab1_unity1.png?raw=true "Unity")

## Цель работы
Ознакомиться с основными операторами языка Python на примере реализации линейной регрессии.

## Задание 1
### Пошагово выполнить каждый пункт раздела "ход работы" с описанием и примерами реализации задач
Ход работы:
- Произведём подготовку данных для работы с алгоритмом линейной регрессии. 10 видов данных были установлены случайным образом, и данные находились в линейной зависимости. Данные преобразуются в формат массива, чтобы их можно было вычислить напрямую при использовании умножения и сложения.

![da_lab1_python2](images/da_lab1_python2.png?raw=true)
![da_lab1_python3](images/da_lab1_python3.png?raw=true)

- Определим связанные функции.
- Функция модели: определяет модель линейной регрессии wx+b. Функция потерь: функция потерь среднеквадратичной ошибки.

![da_lab1_python4](images/da_lab1_python4.png?raw=true)

- Функция оптимизации: метод градиентного спуска для нахождения частных производных w и b.

![da_lab1_python5](images/da_lab1_python5.png?raw=true)

- Так же заведём функцию, которая производит некоторое число итераций, после чего выводит результаты модели

![da_lab1_python6](images/da_lab1_python6.png?raw=true)

(Таким образом каждый эксперимент будет занимать всего одну строчку)

- Инициализация модели

![da_lab1_python7](images/da_lab1_python7.png?raw=true)

- Результаты модели после 1, 2, 3, 4, 5 и 10000 итераций соответственно

![da_lab1_python8_1](images/da_lab1_python8_1.png?raw=true)
-
![da_lab1_python8_2](images/da_lab1_python8_2.png?raw=true)
-
![da_lab1_python8_3](images/da_lab1_python8_3.png?raw=true)
-
![da_lab1_python8_4](images/da_lab1_python8_4.png?raw=true)
-
![da_lab1_python8_5](images/da_lab1_python8_5.png?raw=true)
-
![da_lab1_python8_6](images/da_lab1_python8_6.png?raw=true)

- Как видно, после достаточного числа итераций, выход модели стал похож на входные данные

## Задание 2
### Должна ли величина loss стремиться к нулю при изменении исходных данных? Ответьте на вопрос, приведите пример выполнения кода, который подтверждает ваш ответ.

![da_lab1_python9](images/da_lab1_python9.png?raw=true)

- Первый набор данных чётко следует линейной зависимости
- Второй набор данных - просто случайные числа, без всякой видимой зависимости

- Теперь запустим тот-же самый алгоритм оптимизации:

- Для первого набора данных

![da_lab1_python10](images/da_lab1_python10.png?raw=true)

- Для второго набора данных

![da_lab1_python11](images/da_lab1_python11.png?raw=true)

- В первом эксперименте, где через данные чётко шла линия, модель смогла её найти. Поэтому величина loss стала почти равна нулю
- Во втором экспирименте, где в данных небыло никакой зависимости, модель её и не нашла. Поэтому величина loss особо не изменилась после 90000 итераций
- Таким образом - у значения loss есть минимальное число, которое зависит от модели и набора данных. В процессе обучения loss не может опуститься ниже этого значения, т.к. модели не хватает параметров чтобы идеально отразить все нюансы данных
- Чем "шумнее" набор данных и чем меньше параметров имеет модель, тем дальше наш loss будет от нуля
- (к счастью, на практике это и не важно. Даже наоборот, если модель начинает операться на этот шум, тогда происходит overfitting. Это значит, что модель будет плохо работать на реальных данных)
- В изначальном наборе данных присутсвует достаточно много шума - данные не лежат на одной линии - поэтому наша модель(по сути просто линия) никогда не сможет идеально подстроится под эти данные. Соответственно и значение loss никогда не достигнет нуля(или даже ста)

## Задание 3
### Какова роль параметра Lr? Ответьте на вопрос, приведите пример выполнения кода, который подтверждает ваш ответ. В качестве эксперимента можете изменить значение параметра.

- Для начала - очень маленькое значение Lr(0.000000001)

![da_lab1_python12](images/da_lab1_python12.png?raw=true)

- Модели не хватает шагов, чтобы достичь оптимальных параметров. Поэтому нужно больше вычислительных ресурсов/времени чтобы достичь минимального loss

- Теперь очень большое значение Lr(0.5)

![da_lab1_python13](images/da_lab1_python13.png?raw=true)

- Модель выходит из под контроля и "взрывается". Т.е. значения параметров уходят в бесконечность
- (а если и не взрывается, то может иметь результат хуже, чем с малыми значениями Lr)

- Если же мы найдём идеальное значение Lr(в данном случае 0.0005), то мы можем получить отличиный результат за короткое время

![da_lab1_python14](images/da_lab1_python14.png?raw=true)

## Выводы

- Я освежил знания NumPy, Matplotlib и PyTorch
- разобрал как learning rate и распределение данных влияет на алгоритм градиентного спуска(пусть даже с очень простой моделью)
- вспомнил как работать с GitHub

## Весь код
- Можно найти в файле [DA_in_GameDev_lab1.ipynb](DA_in_GameDev_lab1.ipynb) этого репозитория
- Эксперименты выполнялись в среде [Google Colaboratory](https://colab.research.google.com/)
