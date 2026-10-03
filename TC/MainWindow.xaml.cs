using System;
using System.Windows;
using System.Windows.Controls;
using System.Numerics;

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


			if (MainTabControl != null) MainTabControl.SelectedIndex = 0;
			if (FromUnit != null) FromUnit.SelectedIndex = 1; // Выбираем "Минуты" по умолчанию
			
		}

		// 1. Клик по кнопке-гамбургеру открывает всплывающее меню под ней
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

		private void CloseMenuOnOutsideClick (object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (MenuPopup != null && !HamburgerButton.IsMouseOver && !MenuPopup.IsMouseOver)
			{
				MenuPopup.IsOpen = false;
				this.PreviewMouseDown -= CloseMenuOnOutsideClick;
			}
		}
		// 2. Клик по пункту из выпадающего списка переключает вкладку и СВОРЫВАЕТ меню обратно
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
				DateResultText == null || TimeResultText == null || TotalResultText == null)
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
			// 1. РАЗНОСТЬ ДАТ (Ваша оригинальная логика)
			// ==========================================
			if (isDateValid)
			{
				DateTime start = StartDatePicker.SelectedDate.Value;
				DateTime end = EndDatePicker.SelectedDate.Value;

				// Сохраняем для общего расчёта
				startDateTime = start;
				endDateTime = end;

				// Ваша логика перестановки мест, если дата конца меньше даты начала
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

				DateResultText.Text = $"{totalDays} дней \n({years} лет, {months} мес, {days} дн.)";
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
				TimeResultText.Text = $"{(int)absTimeDiff.TotalHours} ч. {absTimeDiff.Minutes} мин. \n({absTimeDiff:hh\\:mm\\:ss})";
			}
			else
			{
				TimeResultText.Text = ""; // Хотя бы одно время пустое — результат пустой
			}

			// ==========================================
			// 3. ОБЩАЯ РАЗНОСТЬ (Дата1+Время1 и Дата2+Время2)
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
				TotalResultText.Text = $"Общая разность: {totalDiffDays} дн. {totalDiff:hh\\:mm\\:ss}";
			}
			else
			{
				TotalResultText.Text = ""; // Если хотя бы одно из 4-х полей пустое — пусто
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
				FullFormattedResult == null)
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
				FullFormattedResult.Text = "Введите корректное положительное число";
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
			FullFormattedResult.Text = string.Format("{0:N0}\u00A0Лет {1}\u00A0Месяцев {2}\u00A0Дней {3}\u00A0Часов {4}\u00A0Минут {5}\u00A0Секунд",
				years, months, days, hours, minutes, seconds);
		}

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
						// Защита на случай, если буфер обмена временно заблокирован другой программой
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
			FullFormattedResult.Text = "0\u00A0Лет 0\u00A0Месяцев 0\u00A0Дней 0\u00A0Часов 0\u00A0Минут 0\u00A0Секунд";
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
						// Заменяем стандартный текст "Выбор даты" на наш
						watermarkControl.Content = "Дата";

						// === ИЗМЕНЕНИЕ ЦВЕТА ПОДСКАЗКИ ===
						// Красим саму надпись "Дата" в тёмный цвет карточки (#4E464B) для хорошей видимости
						watermarkControl.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4E464B"));
					}
				};
			}
		} // Скобка закрытия метода добавлена, так как в вашем фрагменте её не было


		

		// Вызывается каждый раз, когда открывается (активируется) вкладка 4
		private void IntervalTab_Loaded(object sender, RoutedEventArgs e)
		{
			// Восстанавливаем текст результата, если он уже вычислялся ранее
			if (IntervalResultText != null)
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

			// Защита от отрицательного времени (если нужно уйти в минус, просто удалите это условие)
			if (currentTotalSeconds < 0) currentTotalSeconds = 0;

			UpdateIntervalResultText();
			ResetFocusToDays();
		}

		// Очистить поля ввода
		private void ClearInputFields_Click(object sender, RoutedEventArgs e)
		{
			if (InputIntervalDays != null) InputIntervalDays.Text = "";
			if (InputIntervalHours != null) InputIntervalHours.Text = "";
			if (InputIntervalMinutes != null) InputIntervalMinutes.Text = "";
			if (InputIntervalSeconds != null) InputIntervalSeconds.Text = "";

			ResetFocusToDays();
		}

		// Очистить накопленный результат
		private void ClearResult_Click(object sender, RoutedEventArgs e)
		{
			currentTotalSeconds = 0;
			UpdateIntervalResultText();
			ResetFocusToDays();
		}

		// Копирование строки формата в буфер
		private void CopyIntervalResult_Click(object sender, RoutedEventArgs e)
		{
			if (IntervalResultText == null) return;
			try
			{
				Clipboard.SetText(IntervalResultText.Text);
			}
			catch (System.Runtime.InteropServices.ExternalException) { }
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
			if (IntervalResultText == null) return;

			long tempSeconds = currentTotalSeconds;

			long days = tempSeconds / 86400;
			tempSeconds %= 86400;

			long hours = tempSeconds / 3600;
			tempSeconds %= 3600;

			long minutes = tempSeconds / 60;
			long seconds = tempSeconds % 60;

			IntervalResultText.Text = string.Format("{0} д {1} ч {2} мин {3} с", days, hours, minutes, seconds);
		}

		private void ResetFocusToDays()
		{
			if (InputIntervalDays != null)
			{
				InputIntervalDays.Focus();
				InputIntervalDays.SelectAll();
			}
		}

		// Блокируем нажатие любых клавиш, кроме цифр от 0 до 9
		private void InputValue_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			if (e.Text.Length > 0 && !char.IsDigit(e.Text, e.Text.Length - 1))
			{
				e.Handled = true; // Блокируем буквы и знаки
			}
		}

		private void InputValue_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter)
			{
				e.Handled = true; // Блокируем перенос строки
			}
		}
	}
}
