using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TC
{
	public partial class MainWindow : Window
	{
		private long currentTotalSeconds = 0;
		public MainWindow()
		{
			InitializeComponent();

			this.Loaded += (s, e) =>
			{
				if (StartDatePicker != null) StartDatePicker.Focus();
			};

			MainTabControl.SelectionChanged += (s, e) =>
			{
				//Если переключились на вкладку Разность дат
				if (MainTabControl.SelectedIndex == 0)
				{
					Dispatcher.BeginInvoke(new Action(() =>
					{
						if (StartDatePicker != null) StartDatePicker.Focus();
					}), System.Windows.Threading.DispatcherPriority.Input);
				}
				//Если переключились на вкладку Конвертер
				if (MainTabControl.SelectedIndex == 2)
				{
					Dispatcher.BeginInvoke(new Action(() =>
					{
						if (InputValue != null)
						{
							InputValue.Focus();
							InputValue.SelectAll(); // Выделяем текст, чтобы можно было сразу вводить новое число
						}
					}), System.Windows.Threading.DispatcherPriority.Input);
				}

				// если переключились на вкладку Интервалы
				if (e.Source is TabControl && MainTabControl.SelectedIndex == 3)
				{
					Dispatcher.BeginInvoke(new Action(() =>
					{
						if (InputIntervalDays != null)
						{
							InputIntervalDays.Focus();
							InputIntervalDays.SelectAll();
						}
					}), System.Windows.Threading.DispatcherPriority.Input);
				}
			};
			// если переключились на вкладку Деление
			if (MainTabControl.SelectedIndex == 4)
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					if (DivIntervalDays != null)
					{
						DivIntervalDays.Focus();
						DivIntervalDays.SelectAll();
					}
				}), System.Windows.Threading.DispatcherPriority.Input);
			}
			if (MainTabControl != null) MainTabControl.SelectedIndex = 0;
			if (FromUnit != null) FromUnit.SelectedIndex = 1; // Выбираем "Минуты" по умолчанию

		}

		// 1. Клик по кнопке-гамбургеру
		private void MenuButton_Click(object sender, RoutedEventArgs e)
		{
			if (MenuPopup != null)
			{
				MenuPopup.IsOpen = !MenuPopup.IsOpen;

				if (MenuPopup.IsOpen)
				{
					this.PreviewMouseDown += CloseMenuOnOutsideClick;
				}
				else
				{
					this.PreviewMouseDown -= CloseMenuOnOutsideClick;
				}
			}
		}

		private void CloseMenuOnOutsideClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (MenuPopup != null && !HamburgerButton.IsMouseOver && !MenuPopup.IsMouseOver)
			{
				MenuPopup.IsOpen = false;
				this.PreviewMouseDown -= CloseMenuOnOutsideClick;
			}
		}
		// 2. Клик по пункту из выпадающего списка переключает вкладку и сворачивает меню
		private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (MainTabControl != null && MenuListBox != null && MenuListBox.SelectedIndex >= 0)
			{
				// Переключаем основную вкладку
				MainTabControl.SelectedIndex = MenuListBox.SelectedIndex;

				// Закрываем выпадающее меню
				if (MenuPopup != null) MenuPopup.IsOpen = false;

				// Сбрасываем выделение в меню, чтобы по нему можно было кликнуть повторно
				MenuListBox.SelectedIndex = -1;
			}
		}


		// 3. Операция на Вкладке 1: Дата минус Дата
		private void CalculateDateTimeDifference()
		{
			// Безопасная проверка: если UI ещё не инициализировался, выходим
			if (StartDatePicker == null || EndDatePicker == null ||
				StartTimeTextBox == null || EndTimeTextBox == null ||
				DateResultText == null || TimeResultText == null || DifferenceResult == null)
			{
				return;
			}

			// ИСПРАВЛЕНИЕ ОШИБКИ CS0165: Явно объявляем переменные времени заранее
			TimeSpan startTime = TimeSpan.Zero;
			TimeSpan endTime = TimeSpan.Zero;

			// Безопасный раздельный парсинг полей времени
			bool isStartTimeValid = TimeSpan.TryParse(StartTimeTextBox.Text, out startTime);
			bool isEndTimeValid = TimeSpan.TryParse(EndTimeTextBox.Text, out endTime);

			// Флаги валидности (обновленные)
			bool isDateValid = StartDatePicker.SelectedDate != null && EndDatePicker.SelectedDate != null;
			bool isTimeValid = isStartTimeValid && isEndTimeValid;

			// Переменные для общего расчёта
			DateTime startDateTime = DateTime.MinValue;
			DateTime endDateTime = DateTime.MinValue;

			// ==========================================
			// 1. РАЗНОСТЬ ДАТ 
			// ==========================================
			if (isDateValid)
			{
				DateTime start = StartDatePicker.SelectedDate.Value;
				DateTime end = EndDatePicker.SelectedDate.Value;

				// Сохраняем для общего расчёта
				startDateTime = start;
				endDateTime = end;

				//  логика перестановки мест, если дата конца меньше даты начала
				if (end < start)
				{
					DateTime tmp = end;
					end = start;
					start = tmp;
				}

				TimeSpan difference = end - start;
				int totalDays = Math.Abs(difference.Days);
				int years = 0;
				int months = 0;
				int days = 0;

				DateTime tmpLoop = start;
				while (tmpLoop.AddYears(1) <= end)
				{
					tmpLoop = tmpLoop.AddYears(1);
					years++;
				}
				while (tmpLoop.AddMonths(1) <= end)
				{
					tmpLoop = tmpLoop.AddMonths(1);
					months++;
				}
				days = (end - tmpLoop).Days;

				DateResultText.Text = $"{totalDays} д \n({years} лет {months} мес {days} д)";
			}
			else
			{
				DateResultText.Text = ""; // Хотя бы одна дата пустая — результат пустой
			}

			// ==========================================
			// 2. РАЗНОСТЬ ВРЕМЕНИ
			// ==========================================
			if (isTimeValid)
			{
				// Вычисляем модуль разности времени
				TimeSpan timeDiff = endTime - startTime;
				TimeSpan absTimeDiff = timeDiff.Duration();

				// Выводим в формате: Всего часов и минут \n (чч:мм:сс)
				TimeResultText.Text = $"{(int)absTimeDiff.TotalHours} ч {absTimeDiff.Minutes} мин \n({absTimeDiff:hh} ч {absTimeDiff:mm} мин {absTimeDiff:ss} с)";
				//TimeResultText.Text = $"{(int)absTimeDiff.TotalHours} ч {absTimeDiff.Minutes} мин \n({absTimeDiff:hh\\:mm\\:ss})";
			}
			else
			{
				TimeResultText.Text = ""; // Хотя бы одно время пустое — результат пустой
			}

			// ==========================================
			// 3. ОБЩАЯ РАЗНОСТЬ (Дата1 Время1 - Дата2 Время2)
			// ==========================================
			if (isDateValid && isTimeValid)
			{
				// Объединяем даты со временем
				DateTime fullStart = startDateTime.Add(startTime);
				DateTime fullEnd = endDateTime.Add(endTime);

				if (fullEnd < fullStart)
				{
					DateTime tmp = fullEnd;
					fullEnd = fullStart;
					fullStart = tmp;
				}

				TimeSpan totalDiff = fullEnd - fullStart;
				int totalDiffDays = totalDiff.Days;

				// Форматируем остаток времени (часы, минуты, секунды)
				//DifferenceResult.Text = $"{totalDiffDays} д {totalDiff:hh\\:mm\\:ss}";
				DifferenceResult.Text = $"{totalDiffDays} д {totalDiff:hh} ч {totalDiff:mm} мин {totalDiff:ss} с";
			}
			else
			{
				DifferenceResult.Text = ""; // Если хотя бы одно из 4-х полей пустое — пусто
			}
		}


		// Изменяем старый обработчик DatePicker, чтобы он вызывал общий расчёт
		private void CalculateDateDifference(object sender, SelectionChangedEventArgs e)
		{
			CalculateDateTimeDifference();
		}

		// Добавляем обработчик для TextBox (нужно привязать в XAML: TextChanged="TimeTextBox_TextChanged")
		private void TimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			CalculateDateTimeDifference();
		}


		// 4. Операция на Вкладке 3: Конвертер единиц времени
		private void ConvertTime(object sender, SelectionChangedEventArgs e) => DoConversion();
		private void ConvertTime(object sender, TextChangedEventArgs e) => DoConversion();

		private void DoConversion()
		{
			// Проверяем, что все новые элементы интерфейса уже созданы
			if (InputValue == null || FromUnit == null ||
				ResultSeconds == null || ResultMinutes == null || ResultHours == null ||
				ResultWeeks == null || ResultMonths == null || ResultYears == null ||
				ConverterResult == null)
			{
				return;
			}

			string inputText = InputValue.Text.Trim();

			// Если поле ввода пустое, очищаем все результаты
			if (string.IsNullOrEmpty(inputText))
			{
				ResetConverterFields();
				return;
			}

			// Используем стандартный double, который есть во всех версиях .NET Framework
			if (!double.TryParse(inputText, out double inputNumber) || inputNumber < 0 || double.IsInfinity(inputNumber) || double.IsNaN(inputNumber))
			{
				ConverterResult.Text = "Введите корректное положительное число";
				return;
			}

			// Константы секунд в разных периодах
			double secondsInMinute = 60.0;
			double secondsInHour = 3600.0;
			double secondsInDay = 86400.0;
			double secondsInWeek = 604800.0;
			double secondsInMonth = 2592000.0; // 30 дней
			double secondsInYear = 31536000.0; // 365 дней

			double totalSeconds = 0;

			// 1. ПЕРЕВОДИМ ВВЕДЕННОЕ ЗНАЧЕНИЕ В СЕКУНДЫ
			int selectedIndex = FromUnit.SelectedIndex;
			switch (selectedIndex)
			{
				case 0: totalSeconds = inputNumber; break; // Секунды
				case 1: totalSeconds = inputNumber * secondsInMinute; break; // Минуты
				case 2: totalSeconds = inputNumber * secondsInHour; break; // Часы
				case 3: totalSeconds = inputNumber * secondsInWeek; break; // Недели
				case 4: totalSeconds = inputNumber * secondsInMonth; break; // Месяцы
				case 5: totalSeconds = inputNumber * secondsInYear; break; // Годы
				default: totalSeconds = inputNumber; break;
			}

			// 2. ЗАПОЛНЯЕМ ПРАВУЮ ТАБЛИЦУ (перевод во все единицы одновременно с дробями)
			ResultSeconds.Text = totalSeconds.ToString("N0");
			ResultMinutes.Text = (totalSeconds / secondsInMinute).ToString("N2");
			ResultHours.Text = (totalSeconds / secondsInHour).ToString("N2");
			ResultWeeks.Text = (totalSeconds / secondsInWeek).ToString("N2");
			ResultMonths.Text = (totalSeconds / secondsInMonth).ToString("N2");
			ResultYears.Text = (totalSeconds / secondsInYear).ToString("N2");

			// 3. РАСКЛАДЫВАЕМ НА СТРОКУ ПОЛНОГО ФОРМАТА (с отсечением дробной части для вывода целых единиц)
			double remainder = Math.Floor(totalSeconds);

			double years = Math.Floor(remainder / secondsInYear);
			remainder %= secondsInYear;

			double months = Math.Floor(remainder / secondsInMonth);
			remainder %= secondsInMonth;

			double days = Math.Floor(remainder / secondsInDay);
			remainder %= secondsInDay;

			double hours = Math.Floor(remainder / secondsInHour);
			remainder %= secondsInHour;

			double minutes = Math.Floor(remainder / secondsInMinute);
			double seconds = remainder % secondsInMinute;

			// Выводим готовую строку формата
			ConverterResult.Text = string.Format("{0:N0}\u00A0лет {1}\u00A0мес {2}\u00A0д {3}\u00A0ч {4}\u00A0мин {5}\u00A0с",
				years, months, days, hours, minutes, seconds);
		}
		//вкладка 3
		private void CopyResult_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button copyButton && copyButton.Tag is TextBlock targetTextBlock)
			{
				string textToCopy = targetTextBlock.Text;

				if (!string.IsNullOrEmpty(textToCopy) && textToCopy != "0")
				{
					// Удаляем разделительные пробелы, чтобы скопировать чистое число
					string cleanNumber = textToCopy.Replace(" ", "").Replace("\u00A0", "");

					try
					{
						Clipboard.SetText(cleanNumber);
					}
					catch (System.Runtime.InteropServices.ExternalException)
					{
						// Защита на случай, если буфер обмена временно заблокирован
					}
				}
			}
		}

		// Метод сброса полей в дефолтное состояние
		private void ResetConverterFields()
		{
			ResultSeconds.Text = "0";
			ResultMinutes.Text = "0";
			ResultHours.Text = "0";
			ResultWeeks.Text = "0";
			ResultMonths.Text = "0";
			ResultYears.Text = "0";
			ConverterResult.Text = "0\u00A0лет 0\u00A0мес 0\u00A0д 0\u00A0ч 0\u00A0мин 0\u00A0с";
		}

		private void StartDatePicker_Loaded(object sender, RoutedEventArgs e)
		{
			var datePicker = sender as DatePicker;
			if (datePicker == null) return;

			// Принудительно заставляем применить шаблон к элементу
			datePicker.ApplyTemplate();

			// Находим внутреннее текстовое поле
			var textBox = datePicker.Template.FindName("PART_TextBox", datePicker) as System.Windows.Controls.Primitives.DatePickerTextBox;
			if (textBox != null)
			{
				// === ИЗМЕНЕНИЕ ЦВЕТА ПОЛЯ ВВОДА ===
				// Устанавливаем фон текстового поля как у меню (#F3EFE9) и цвет вводимых цифр (#4E464B)
				textBox.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F3EFE9"));
				textBox.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4E464B"));

				// Подписываемся на событие изменения шаблона самого текстового поля, 
				// так как именно там внутри живет встроенный TextBlock подсказки
				textBox.Loaded += (s, args) =>
				{
					var dtb = s as System.Windows.Controls.Primitives.DatePickerTextBox;
					if (dtb == null) return;

					// Ищем внутри текстового поля элемент ContentControl (это и есть Watermark)
					var watermarkControl = dtb.Template.FindName("PART_Watermark", dtb) as ContentControl;
					if (watermarkControl != null)
					{
						// Заменяем стандартный text "Выбор даты" на наш
						watermarkControl.Content = "Дата";

						// === ИЗМЕНЕНИЕ ЦВЕТА ПОДСКАЗКИ ===
						// Красим саму надпись "Дата" в тёмный цвет карточки (#4E464B) для хорошей видимости
						watermarkControl.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4E464B"));
					}
				};
			}
		}

		// Вызывается каждый раз, когда открывается (активируется) вкладка 4
		private void IntervalTab_Loaded(object sender, RoutedEventArgs e)
		{
			// Восстанавливаем текст результата, если он уже вычислялся ранее
			if (IntervalResult != null)
			{
				UpdateIntervalResultText();
			}

			// Принудительно ставим курсор в поле ввода Дней и выделяем текст внутри
			if (InputIntervalDays != null)
			{
				InputIntervalDays.Focus();
				InputIntervalDays.SelectAll();
			}
		}

		// Кнопка ПЛЮС
		private void AddInterval_Click(object sender, RoutedEventArgs e)
		{
			long inputSeconds = GetInputIntervalInSeconds();
			currentTotalSeconds += inputSeconds;

			UpdateIntervalResultText();
			ResetFocusToDays();
		}

		// Кнопка МИНУС
		private void SubtractInterval_Click(object sender, RoutedEventArgs e)
		{
			long inputSeconds = GetInputIntervalInSeconds();
			currentTotalSeconds -= inputSeconds;

			// Защита от отрицательного времени
			if (currentTotalSeconds < 0) currentTotalSeconds = 0;

			UpdateIntervalResultText();
			ResetFocusToDays();
		}

	

		// Очистить поля. Вкладка 4
		private void ClearResult_Click(object sender, RoutedEventArgs e)
		{
			currentTotalSeconds = 0;
			UpdateIntervalResultText();
			if (InputIntervalDays != null) InputIntervalDays.Text = "д";
			if (InputIntervalHours != null) InputIntervalHours.Text = "ч";
			if (InputIntervalMinutes != null) InputIntervalMinutes.Text = "мин";
			if (InputIntervalSeconds != null) InputIntervalSeconds.Text = "с";


			ResetFocusToDays();
		}

		// Копирование результата в буфер
		private void CopyFullResult_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!(sender is Button clickedButton)) return;

				string buttonName = clickedButton.Name;
				string targetControlName = buttonName.Substring(0, buttonName.Length - 4)+"Result"; // Преобразовать xxxCopy to xxxResult

				object targetControl = this.FindName(targetControlName);
				if (targetControl == null) return;

				string rawText = string.Empty;

				// 1. Получаем сырой текст из элемента управления
				if (targetControl is TextBlock textBlock)
				{
					rawText = textBlock.Text;
				}
				else if (targetControl is TextBox textBox)
				{
					rawText = textBox.Text;
				}
				else if (targetControl is DatePicker datePicker)
				{
					if (datePicker.SelectedDate != null)
					{
						Clipboard.SetText(datePicker.SelectedDate.Value.ToString("d"));
					}
					return;
				}

				if (string.IsNullOrWhiteSpace(rawText)) return;

				// строку к нижнему регистру, заменяя все пробелы на обычные
				string cleanText = rawText.Replace("\u00A0", " ").Replace("\r", " ").Replace("\n", " ").ToLower().Trim();

				// Константы перевода единиц времени в секунды
				double secondsInMinute = 60.0;
				double secondsInHour = 3600.0;
				double secondsInDay = 86400.0;
				double secondsInMonth = 2592000.0; // 30 дней
				double secondsInYear = 31536000.0; // 365 дней

				double totalSeconds = 0;
				bool parsedAnyUnit = false;

				// Разбираем строку по словам/блокам через пробелы
				string[] tokens = cleanText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				string currentNumberStr = "";

				for (int i = 0; i < tokens.Length; i++)
				{
					string token = tokens[i].Trim();
					if (string.IsNullOrEmpty(token)) continue;

					// Если токен состоит только из цифр (или цифры с запятой/точкой) — это часть числа (или разряд)
					if (long.TryParse(token.Replace(",", "").Replace(".", ""), out _))
					{
						currentNumberStr += token; // Склеиваем разряды (например "4" + "227" + "972")
						continue;
					}

					// Если токен содержит буквы, проверяем, идет ли перед ним накопленное число
					// Либо число склеено с буквой (например "33д")
					string numPart = "";
					string letterPart = "";

					// Разделяем токен на цифры и буквы, если они склеены
					foreach (char c in token)
					{
						if (char.IsDigit(c)) numPart += c;
						else letterPart += c;
					}

					if (!string.IsNullOrEmpty(numPart))
					{
						currentNumberStr = numPart;
					}

					if (!string.IsNullOrEmpty(currentNumberStr) && !string.IsNullOrEmpty(letterPart))
					{
						if (long.TryParse(currentNumberStr, out long value))
						{
							// Проверяем метки времени
							if (letterPart.StartsWith("лет") || letterPart.StartsWith("год") || letterPart == "г" || letterPart == "г.")
							{
								totalSeconds += value * secondsInYear;
								parsedAnyUnit = true;
							}
							else if (letterPart.StartsWith("мес"))
							{
								totalSeconds += value * secondsInMonth;
								parsedAnyUnit = true;
							}
							else if (letterPart.StartsWith("дн") || letterPart == "д" || letterPart == "д." || letterPart.StartsWith("ден"))
							{
								totalSeconds += value * secondsInDay;
								parsedAnyUnit = true;
							}
							else if (letterPart == "ч" || letterPart == "ч." || letterPart.StartsWith("час"))
							{
								totalSeconds += value * secondsInHour;
								parsedAnyUnit = true;
							}
							else if (letterPart.StartsWith("мин"))
							{
								totalSeconds += value * secondsInMinute;
								parsedAnyUnit = true;
							}
							else if (letterPart == "с" || letterPart == "с." || letterPart.StartsWith("сек"))
							{
								totalSeconds += value;
								parsedAnyUnit = true;
							}
						}
						currentNumberStr = ""; // Сбрасываем для следующей пары
					}
				}

				// 4. Финальная запись в буфер
				if (!parsedAnyUnit)
				{
					// Если меток времени вообще нет (чистое число из полей Конвертера вроде "13333333")
					string pureDigits = cleanText.Replace(" ", "").Replace(",", ".");
					Clipboard.SetText(pureDigits);
				}
				else
				{
					long finalSeconds = (long)Math.Round(totalSeconds);
					Clipboard.SetText(finalSeconds.ToString());
				}
			}
			catch (System.Runtime.InteropServices.ExternalException)
			{
				// Защита буфера обмена
			}
		}

		// ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ СЧЕТА И ФОКУСА
		private long GetInputIntervalInSeconds()
		{
			if (InputIntervalDays == null || InputIntervalHours == null ||
				InputIntervalMinutes == null || InputIntervalSeconds == null) return 0;

			long.TryParse(InputIntervalDays.Text, out long d);
			long.TryParse(InputIntervalHours.Text, out long h);
			long.TryParse(InputIntervalMinutes.Text, out long m);
			long.TryParse(InputIntervalSeconds.Text, out long s);

			return s + (m * 60) + (h * 3600) + (d * 86400);
		}

		private void UpdateIntervalResultText()
		{
			if (IntervalResult == null) return;

			long tempSeconds = currentTotalSeconds;

			long days = tempSeconds / 86400;
			tempSeconds %= 86400;

			long hours = tempSeconds / 3600;
			tempSeconds %= 3600;

			long minutes = tempSeconds / 60;
			long seconds = tempSeconds % 60;

			IntervalResult.Text = string.Format("{0} д {1} ч {2} мин {3} с", days, hours, minutes, seconds);
		}

		private void ResetFocusToDays()
		{
			if (InputIntervalDays != null)
			{
				InputIntervalDays.Focus();
				InputIntervalDays.SelectAll();
			}
		}

		// Обработчик ввода для поля Конвертера (разрешает только цифры и одну запятую/точку)
		private void InputValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				// Разрешаем цифры
				if (char.IsDigit(e.Text[0])) return;

				// Разрешаем одну точку или запятую для дробных чисел
				if ((e.Text == "." || e.Text == ",") && !textBox.Text.Contains(".") && !textBox.Text.Contains(","))
				{
					return;
				}

				// Блокируем всё остальное
				e.Handled = true;
			}
		}

		// Обработчик клавиш для поля Конвертера (если он прописан в XAML)
		private void InputValue_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (sender is TextBox textBox)
				{
					// Создаем запрос на перемещение фокуса к следующему элементу в порядке TabIndex
					var request = new TraversalRequest(FocusNavigationDirection.Next);

					// Перемещаем фокус
					textBox.MoveFocus(request);

					// Блокируем стандартное поведение Enter (чтобы не было звука ошибки Windows)
					e.Handled = true;
				}
			}

			// Разрешаем системные клавиши (Backspace, Delete, стрелки)
			else if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right)
			{
				return;
			}
		}


		// =========================================================================
		// ЛОГИКА МАСКИ ВРЕМЕНИ ДЛЯ ПОЛЕЙ ВВОДА В КАРТОЧКЕ (ЧЧ:ММ:СС)
		// =========================================================================

		private const string Mask = "__:__:__";
		private const string Watermark = "Время";

		// 1. ПОЛУЧЕНИЕ ФОКУСА: При переходе через Tab ставим курсор в самое начало (0 индекс)
		private void TimeTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				if (textBox.Text == Watermark)
				{
					textBox.Text = Mask;
				}
				textBox.CaretIndex = 0;
			}
		}

		// 2. ПОТЕРЯ ФОКУСА: Если поле пустое — возвращаем слово "Время", иначе нормализуем нулями
		private void TimeTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				if (string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text == Mask || IsMaskEmpty(textBox.Text))
				{
					textBox.Text = Watermark;
				}
				else
				{
					NormalizeAndFillZeros(textBox);
				}
			}
		}

		// Клик мыши: если поле пустое, курсор сбрасывается в начало, если заполнено — встает туда, куда кликнули
		private void TimeTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				if (!textBox.IsFocused)
				{
					if (textBox.Text == Watermark || textBox.Text == Mask || IsMaskEmpty(textBox.Text))
					{
						textBox.Focus();
						textBox.CaretIndex = 0;
						e.Handled = true;
					}
				}
			}
		}

		// 3. СЛУЖЕБНЫЕ КНОПКИ: Enter дополняет нулями. Delete/Backspace сбрасывают всё в маску при полном выделении.
		private void TimeTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (!(sender is TextBox textBox)) return;

			if (e.Key == Key.Enter)
			{
				NormalizeAndFillZeros(textBox);
				e.Handled = true;
				return;
			}

			if (e.Key == Key.Delete || e.Key == Key.Back)
			{
				if (textBox.SelectedText == textBox.Text)
				{
					textBox.Text = Mask;
					textBox.CaretIndex = 0;
					e.Handled = true;
					return;
				}
			}
		}

		// 4. ВВОД ЦИФР: Валидация диапазонов на лету (Часы 0-23, Минуты/Секунды 0-59)
		private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!(sender is TextBox textBox)) return;

			// ИСПРАВЛЕНО: Берем первый символ строки [0] для IsDigit
			if (string.IsNullOrEmpty(e.Text) || !char.IsDigit(e.Text[0]))
			{
				e.Handled = true;
				return;
			}

			int caret = textBox.CaretIndex;
			if (caret >= 8)
			{
				e.Handled = true;
				return;
			}

			if (caret == 2 || caret == 5)
			{
				caret++;
				textBox.CaretIndex = caret;
			}

			// ИСПРАВЛЕНО: Берем символ по индексу [0]
			char inputChar = e.Text[0];
			char[] textChars = textBox.Text.PadRight(8, '_').ToCharArray();

			// Проверка корректности диапазонов времени
			if (caret == 0 && inputChar > '2') { e.Handled = true; return; }
			if (caret == 1 && textChars[0] == '2' && inputChar > '3') { e.Handled = true; return; }
			if ((caret == 3 || caret == 6) && inputChar > '5') { e.Handled = true; return; }

			textChars[caret] = inputChar;
			string newText = new string(textChars);

			textBox.Text = newText;
			textBox.CaretIndex = caret + 1;
			e.Handled = true;
		}

		// 5. ДВОЙНОЙ КЛИК: Выделение всего значения целиком
		private void TimeTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				if (textBox.Text != Watermark && textBox.Text != Mask && !IsMaskEmpty(textBox.Text))
				{
					textBox.SelectAll();
				}
			}
		}

		// Вспомогательные функции форматирования
		private bool IsMaskEmpty(string text)
		{
			return !System.Text.RegularExpressions.Regex.IsMatch(text, @"\d");
		}

		private string FormatSegment(string segment)
		{
			if (segment == "__" || segment.Replace("_", "").Length == 0) return "00";
			string clean = segment.Replace("_", "");
			if (clean.Length == 1) return "0" + clean;
			return clean;
		}

		private void NormalizeAndFillZeros(TextBox textBox)
		{
			if (textBox.Text == Watermark || textBox.Text == Mask) return;

			string[] parts = textBox.Text.Split(':');
			if (parts.Length != 3) return;

			for (int i = 0; i < 3; i++)
			{
				parts[i] = FormatSegment(parts[i]);
			}

			textBox.Text = string.Format("{0}:{1}:{2}", parts[0], parts[1], parts[2]);
		}
		// Обработка фокуса для полей интервалов (убираем подсказку при клике или переходе по Tab)
		private void IntervalTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				if (textBox.Text == "д" || textBox.Text == "ч" || textBox.Text == "мин" || textBox.Text == "с")
				{
					textBox.Text = "";
				}
			}
		}

		// Обработка потери фокуса для полей интервалов (возвращаем подсказку, если поле осталось пустым)
		private void IntervalTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (sender is TextBox textBox)
			{
				if (string.IsNullOrWhiteSpace(textBox.Text))
				{
					if (textBox.Name == "InputIntervalDays") textBox.Text = "д";
					if (textBox.Name == "InputIntervalHours") textBox.Text = "ч";
					if (textBox.Name == "InputIntervalMinutes") textBox.Text = "мин";
					if (textBox.Name == "InputIntervalSeconds") textBox.Text = "с";
				}
			}
		}

		private void AddIntervalAdd_Click(object sender, RoutedEventArgs e)
		{
			CalculateTargetDateTime(true);
		}

		// Кнопка МИНУС на Вкладке 2
		private void AddIntervalSubtract_Click(object sender, RoutedEventArgs e)
		{
			CalculateTargetDateTime(false);
		}
		//Вставить на вкладке 2
		private void AddIntervalPaste_Click(object sender, RoutedEventArgs e)
		{
			// Проверяем, что в полях результата есть данные
			if (AddResultDate == null || AddResultTime == null || StartDate == null || StartTime == null) return;

			if (AddResultDate.SelectedDate == null)
			{
				MessageBox.Show("Нет данных для вставки. Сначала выполните расчет.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			// 1. Переносим результирующую дату наверх в поле ввода
			StartDate.SelectedDate = AddResultDate.SelectedDate;

			// 2. Переносим результирующее время наверх в поле ввода
			StartTime.Text = AddResultTime.Text;

			// 3. Сбрасываем поля ввода интервалов в исходные подсказки (д, ч, мин, с)
			if (AddIntervalDays != null) AddIntervalDays.Text = "д";
			if (AddIntervalHours != null) AddIntervalHours.Text = "ч";
			if (AddIntervalMinutes != null) AddIntervalMinutes.Text = "мин";
			if (AddIntervalSeconds != null) AddIntervalSeconds.Text = "с";

			// 4. Очищаем нижние поля результата, так как они стали начальными данными
			AddResultDate.SelectedDate = null;
			AddResultTime.Text = "00:00:00";

			// 5. Возвращаем фокус на поле ввода дней, чтобы можно было сразу вводить новый интервал
			if (AddIntervalDays != null)
			{
				AddIntervalDays.Focus();
				AddIntervalDays.SelectAll();
			}
		}

		// Кнопка ОЧИСТИТЬ на Вкладке 2
		private void AddIntervalClear_Click(object sender, RoutedEventArgs e)
		{
			// 1. Полностью очищаем базовую дату (ставим null, чтобы вернуть подсказку), а время в "Время"
			if (StartDate != null) StartDate.SelectedDate = null;
			if (StartTime != null) StartTime.Text = "Время";

			// 2. Сбрасываем числовые поля ввода интервалов в дефолтные маски
			if (AddIntervalDays != null) AddIntervalDays.Text = "д";
			if (AddIntervalHours != null) AddIntervalHours.Text = "ч";
			if (AddIntervalMinutes != null) AddIntervalMinutes.Text = "мин";
			if (AddIntervalSeconds != null) AddIntervalSeconds.Text = "с";

			// 3. Полностью очищаем нижние поля вывода результатов
			if (AddResultDate != null) AddResultDate.SelectedDate = null;
			if (AddResultTime != null) AddResultTime.Text = "00:00:00";

			// 4. Возвращаем фокус на Стартовую дату, как требует UI-логика
			if (StartDate != null) StartDate.Focus();
		}

		// Главный метод вычисления даты и времени для Вкладки 2
		private void CalculateTargetDateTime(bool isAddition)
		{
			if (StartDate == null || StartTime == null || AddResultDate == null || AddResultTime == null) return;

			// 1. Проверяем валидность базовой даты
			if (StartDate.SelectedDate == null)
			{
				MessageBox.Show("Пожалуйста, выберите базовую дату.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			DateTime baseDate = StartDate.SelectedDate.Value;

			// 2. Парсим базовое время (если там "Время" или маска пуста — берем 00:00:00)
			TimeSpan baseTime = TimeSpan.Zero;
			if (StartTime.Text != "Время" && StartTime.Text != "__:__:__")
			{
				TimeSpan.TryParse(StartTime.Text, out baseTime);
			}

			// Объединяем стартовую дату и время в единую точку отсчета
			DateTime targetDateTime = baseDate.Add(baseTime);

			// 3. Безопасно парсим введенные интервалы (если в поле осталась подсказка буквами — запишется 0)
			double.TryParse(AddIntervalDays.Text, out double d);
			double.TryParse(AddIntervalHours.Text, out double h);
			double.TryParse(AddIntervalMinutes.Text, out double m);
			double.TryParse(AddIntervalSeconds.Text, out double s);

			// Если идет вычитание, инвертируем знаки всех введенных чисел
			if (!isAddition)
			{
				d = -d;
				h = -h;
				m = -m;
				s = -s;
			}

			try
			{
				// 4. Последовательно добавляем/вычитаем каждую единицу времени.
				// Это позволяет вводить любые гигантские числа в любое поле — .NET сам переведет 
				// лишние секунды в минуты, минуты в часы, а часы в дни и изменит дату.
				targetDateTime = targetDateTime.AddDays(d);
				targetDateTime = targetDateTime.AddHours(h);
				targetDateTime = targetDateTime.AddMinutes(m);
				targetDateTime = targetDateTime.AddSeconds(s);

				// 5. Выводим результат в наши заблокированные поля
				AddResultDate.SelectedDate = targetDateTime.Date;
				AddResultTime.Text = targetDateTime.ToString("HH:mm:ss");
			}
			catch (ArgumentOutOfRangeException)
			{
				MessageBox.Show("Результат вычислений выходит за рамки допустимого календаря (минимальная дата: 01.01.0001, максимальная: 31.12.9999).", "Выход за границы", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		// =========================================================================
		// ЛОГИКА ДЛЯ ВКЛАДКИ 5 (УМНОЖЕНИЕ И ДЕЛЕНИЕ ИНТЕРВАЛОВ)
		// =========================================================================

		// Переменная для сохранения общего количества секунд результата Вкладки 5
		private long divTotalSeconds = 0;

		// Кнопка УМНОЖИТЬ (×)
		private void DivIntervalMultiply_Click(object sender, RoutedEventArgs e)
		{
			ExecuteIntervalMath(true);
		}

		// Кнопка ДЕЛЕНИЕ (/)
		private void DivIntervalDivide_Click(object sender, RoutedEventArgs e)
		{
			ExecuteIntervalMath(false);
		}

		// Кнопка ВСТАВИТЬ РЕЗУЛЬТАТ
		private void DivIntervalPaste_Click(object sender, RoutedEventArgs e)
		{
			if (DivIntervalDays == null || DivResult == null) return;

			long tempSeconds = divTotalSeconds;

			long days = tempSeconds / 86400;
			tempSeconds %= 86400;

			long hours = tempSeconds / 3600;
			tempSeconds %= 3600;

			long minutes = tempSeconds / 60;
			long seconds = tempSeconds % 60;

			// Раскладываем секунды обратно по полям ввода
			DivIntervalDays.Text = days > 0 ? days.ToString() : "д";
			DivIntervalHours.Text = hours > 0 ? hours.ToString() : "ч";
			DivIntervalMinutes.Text = minutes > 0 ? minutes.ToString() : "мин";
			DivIntervalSeconds.Text = seconds > 0 ? seconds.ToString() : "с";

			// Возвращаем делитель/множитель в состояние "1"
			if (DivInputNumber != null) DivInputNumber.Text = "1";

			// Переводим фокус на поле ввода числа модификатора
			if (DivInputNumber != null)
			{
				DivInputNumber.Focus();
				DivInputNumber.SelectAll();
			}
		}

		// Кнопка ОЧИСТИТЬ ВСЁ
		private void DivIntervalClear_Click(object sender, RoutedEventArgs e)
		{
			divTotalSeconds = 0;

			if (DivResult != null) DivResult.Text = "0 д 0 ч 0 мин 0 с";
			if (DivIntervalDays != null) DivIntervalDays.Text = "д";
			if (DivIntervalHours != null) DivIntervalHours.Text = "ч";
			if (DivIntervalMinutes != null) DivIntervalMinutes.Text = "мин";
			if (DivIntervalSeconds != null) DivIntervalSeconds.Text = "с";
			if (DivInputNumber != null) DivInputNumber.Text = "1";

			if (DivIntervalDays != null)
			{
				DivIntervalDays.Focus();
				DivIntervalDays.SelectAll();
			}
		}

		// Единый метод вычисления для Вкладки 5
		private void ExecuteIntervalMath(bool isMultiplication)
		{
			if (DivIntervalDays == null || DivInputNumber == null || DivResult == null) return;

			// Парсим исходный временной интервал
			long.TryParse(DivIntervalDays.Text, out long d);
			long.TryParse(DivIntervalHours.Text, out long h);
			long.TryParse(DivIntervalMinutes.Text, out long m);
			long.TryParse(DivIntervalSeconds.Text, out long s);

			long inputSeconds = s + (m * 60) + (h * 3600) + (d * 86400);

			// Парсим число-модификатор (множитель или делитель)
			if (!double.TryParse(DivInputNumber.Text, out double factor) || factor <= 0)
			{
				MessageBox.Show("Пожалуйста, введите корректное положительное число больше нуля.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			// Рассчитываем итоговые секунды
			if (isMultiplication)
			{
				divTotalSeconds = (long)Math.Round(inputSeconds * factor);
			}
			else
			{
				divTotalSeconds = (long)Math.Round(inputSeconds / factor);
			}

			if (divTotalSeconds < 0) divTotalSeconds = 0;

			// Переводим секунды в красивый читаемый формат
			long tempSeconds = divTotalSeconds;

			long resDays = tempSeconds / 86400;
			tempSeconds %= 86400;

			long resHours = tempSeconds / 3600;
			tempSeconds %= 3600;

			long resMinutes = tempSeconds / 60;
			long resSeconds = tempSeconds % 60;

			DivResult.Text = $"{resDays} д {resHours} ч {resMinutes} мин {resSeconds} с";
		}
	} // MainWindow
} //  TC
