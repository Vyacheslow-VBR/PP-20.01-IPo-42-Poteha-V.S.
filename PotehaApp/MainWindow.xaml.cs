using PotehaApp.ViewModels;
using PotehaLibrary;
using PotehaLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Globalization;
using System.Xml;

namespace PotehaApp
{
    public partial class MainWindow : Window
    {
        private AppDbContext _context;
        private List<PartnerViewModel> _partners;

        public MainWindow()
        {
            InitializeComponent();

            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");

           

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _context = new AppDbContext();

                var partners = _context.Partners
                    .Include(p => p.PartnerType)
                    .Include(p => p.Sales)
                        .ThenInclude(s => s.Product)
                    .ToList();

                _partners = partners.Select(p => new PartnerViewModel(p)).ToList();
                PartnersListBox.ItemsSource = _partners;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPartnerSales(int partnerId)
        {
            try
            {
                var sales = _context.Sales
                    .Include(s => s.Product)
                    .Where(s => s.PartnerId == partnerId)
                    .OrderByDescending(s => s.SaleDate)
                    .ToList();

                SalesDataGrid.ItemsSource = sales.Select(s => new SaleViewModel(s));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продаж: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PartnersListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                LoadPartnerSales(selectedPartner.Id);
            }
        }

        // ===== Обработчики меню =====

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Данные сохранены", "Информация",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ===== Экспорт =====

        private void SaveAsTxtMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToFile("txt", "Текстовые файлы (*.txt)|*.txt", SaveAsTxt);
        }

        private void SaveAsExcelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToFile("csv", "CSV файлы (*.csv)|*.csv", SaveAsCsv);
        }

        private void SaveAsJsonMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToFile("json", "JSON файлы (*.json)|*.json", SaveAsJson);
        }

        private void SaveDataToFile(string extension, string filter, Action<string> saveAction)
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = extension,
                FileName = $"partners_export_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    saveAction(dialog.FileName);
                    MessageBox.Show($"Данные успешно сохранены в файл:\n{dialog.FileName}",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения файла: {ex.Message}",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveAsTxt(string filename)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                writer.WriteLine("=== СПИСОК ПАРТНЕРОВ ===");
                writer.WriteLine($"Дата экспорта: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                writer.WriteLine(new string('-', 80));

                foreach (var partner in _partners)
                {
                    writer.WriteLine($"Партнер: {partner.DisplayName}");
                    writer.WriteLine($"Директор: {partner.Director}");
                    writer.WriteLine($"Телефон: {partner.Phone}");
                    writer.WriteLine($"Рейтинг: {(partner.Rating.HasValue ? partner.Rating.Value.ToString() : "Не указан")}");
                    writer.WriteLine($"Скидка: {partner.Discount}%");

                    var sales = _context.Sales
                        .Include("Product")
                        .Where(s => s.PartnerId == partner.Id)
                        .ToList();

                    if (sales.Any())
                    {
                        writer.WriteLine("Продажи:");
                        foreach (var sale in sales)
                        {
                            string productName = sale.Product != null ? sale.Product.Name : "Неизвестно";
                            writer.WriteLine($"  - {productName}: {sale.Quantity} шт. на {sale.TotalAmount:N0} ₽ (дата: {sale.SaleDate:dd.MM.yyyy})");
                        }
                    }

                    writer.WriteLine(new string('-', 80));
                }
            }
        }

        private void SaveAsCsv(string filename)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                writer.WriteLine("Партнер;Директор;Телефон;Рейтинг;Скидка;Продукт;Количество;Дата;Сумма");

                foreach (var partner in _partners)
                {
                    var sales = _context.Sales
                        .Include("Product")
                        .Where(s => s.PartnerId == partner.Id)
                        .ToList();

                    if (sales.Any())
                    {
                        foreach (var sale in sales)
                        {
                            string productName = sale.Product != null ? sale.Product.Name : "Неизвестно";
                            string rating = partner.Rating.HasValue ? partner.Rating.Value.ToString() : "";
                            writer.WriteLine($"{partner.DisplayName};{partner.Director};{partner.Phone};{rating};{partner.Discount}%;{productName};{sale.Quantity};{sale.SaleDate:dd.MM.yyyy};{sale.TotalAmount:F2} ₽");
                        }
                    }
                    else
                    {
                        string rating = partner.Rating.HasValue ? partner.Rating.Value.ToString() : "";
                        writer.WriteLine($"{partner.DisplayName};{partner.Director};{partner.Phone};{rating};{partner.Discount}%;;;;");
                    }
                }
            }
        }

        private void SaveAsJson(string filename)
        {
            var exportData = new List<object>();

            foreach (var partner in _partners)
            {
                var sales = _context.Sales
                    .Include("Product")
                    .Where(s => s.PartnerId == partner.Id)
                    .Select(s => new
                    {
                        Product = s.Product != null ? s.Product.Name : "Неизвестно",
                        Quantity = s.Quantity,
                        Date = s.SaleDate.ToString("dd.MM.yyyy"),
                        Amount = s.TotalAmount
                    }).ToList();

                var partnerData = new
                {
                    Partner = partner.DisplayName,
                    Director = partner.Director,
                    Phone = partner.Phone,
                    Rating = partner.Rating,
                    Discount = partner.Discount,
                    Sales = sales
                };

                exportData.Add(partnerData);
            }

            string json = JsonConvert.SerializeObject(exportData, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filename, json, Encoding.UTF8);
        }

        // ===== Импорт =====

        private void ImportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Поддерживаемые файлы|*.txt;*.csv;*.json|Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv|JSON файлы (*.json)|*.json",
                Title = "Выберите файл для импорта"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string extension = System.IO.Path.GetExtension(dialog.FileName).ToLower();

                    var importOption = MessageBox.Show(
                        "Как импортировать данные?\n\n" +
                        "Да - добавить к существующим данным\n" +
                        "Нет - заменить существующие данные",
                        "Выберите режим импорта",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    bool addToExisting = (importOption == MessageBoxResult.Yes);

                    switch (extension)
                    {
                        case ".txt":
                            ImportFromTxt(dialog.FileName, addToExisting);
                            break;
                        case ".csv":
                            ImportFromCsv(dialog.FileName, addToExisting);
                            break;
                        case ".json":
                            ImportFromJson(dialog.FileName, addToExisting);
                            break;
                        default:
                            MessageBox.Show("Неподдерживаемый формат файла", "Ошибка",
                                           MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при импорте: {ex.Message}",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportFromTxt(string filename, bool addToExisting)
        {
            MessageBox.Show("Импорт из TXT в разработке", "Информация",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ImportFromCsv(string filename, bool addToExisting)
        {
            MessageBox.Show("Импорт из CSV в разработке", "Информация",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ImportFromJson(string filename, bool addToExisting)
        {
            try
            {
                string jsonContent = File.ReadAllText(filename, Encoding.UTF8);
                var items = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent);

                if (items == null || items.Count == 0)
                {
                    MessageBox.Show("Файл не содержит данных", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int importedCount = 0;

                foreach (var item in items)
                {
                    try
                    {
                        string fullName = item.Partner != null ? item.Partner.ToString() : "";
                        if (string.IsNullOrWhiteSpace(fullName)) continue;

                        string typeName = "ООО";
                        string partnerName = fullName;

                        if (fullName.Contains("/"))
                        {
                            var parts = fullName.Split('/');
                            typeName = parts[0].Trim();
                            partnerName = parts[1].Trim();
                        }

                        var partnerType = _context.PartnerTypes.FirstOrDefault(t => t.Name == typeName);
                        if (partnerType == null)
                        {
                            partnerType = _context.PartnerTypes.First();
                        }

                        var partner = new Partner
                        {
                            Name = partnerName,
                            TypeId = partnerType.Id,
                            DirectorFullname = item.Director?.ToString(),
                            Phone = item.Phone?.ToString(),
                            Email = item.Email?.ToString(),
                            Rating = item.Rating != null ? (int?)Convert.ToInt32(item.Rating) : null,
                            CreatedAt = DateTime.Now
                        };

                        _context.Partners.Add(partner);
                        importedCount++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                    }
                }

                if (importedCount > 0)
                {
                    _context.SaveChanges();
                    LoadData();
                    MessageBox.Show($"Импорт JSON завершен!\n\nИмпортировано: {importedCount} записей",
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось импортировать данные", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте JSON: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== Открытие файлов =====

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Все поддерживаемые файлы|*.txt;*.csv;*.json|Текстовые файлы (*.txt)|*.txt|CSV файлы (*.csv)|*.csv|JSON файлы (*.json)|*.json",
                Title = "Открыть файл с данными"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string extension = System.IO.Path.GetExtension(dialog.FileName).ToLower();

                    switch (extension)
                    {
                        case ".txt":
                            OpenTxtFile(dialog.FileName);
                            break;
                        case ".csv":
                            OpenCsvFile(dialog.FileName);
                            break;
                        case ".json":
                            OpenJsonFile(dialog.FileName);
                            break;
                        default:
                            MessageBox.Show("Неподдерживаемый формат файла", "Ошибка",
                                           MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла: {ex.Message}",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenTxtFile(string filename)
        {
            var viewerWindow = new Window
            {
                Title = $"Просмотр файла - {System.IO.Path.GetFileName(filename)}",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var textBox = new System.Windows.Controls.TextBox
            {
                IsReadOnly = true,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 12,
                Text = File.ReadAllText(filename, Encoding.UTF8),
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                Margin = new Thickness(5)
            };

            viewerWindow.Content = textBox;
            viewerWindow.ShowDialog();
        }

        private void OpenCsvFile(string filename)
        {
            try
            {
                var lines = File.ReadAllLines(filename, Encoding.UTF8);

                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст", "Информация",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var viewerWindow = new Window
                {
                    Title = $"Просмотр CSV - {System.IO.Path.GetFileName(filename)}",
                    Width = 900,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var headers = lines[0].Split(';');
                var data = new System.Collections.ObjectModel.ObservableCollection<dynamic>();

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;

                    var values = lines[i].Split(';');
                    var expando = new System.Dynamic.ExpandoObject();
                    var dict = expando as System.Collections.Generic.IDictionary<string, object>;

                    for (int j = 0; j < headers.Length && j < values.Length; j++)
                    {
                        dict[headers[j]] = values[j];
                    }

                    data.Add(expando);
                }

                var grid = new System.Windows.Controls.DataGrid
                {
                    AutoGenerateColumns = true,
                    IsReadOnly = true,
                    Margin = new Thickness(5),
                    VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    ItemsSource = data
                };

                viewerWindow.Content = grid;
                viewerWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии CSV файла: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenJsonFile(string filename)
        {
            try
            {
                string jsonContent = File.ReadAllText(filename, Encoding.UTF8);

                try
                {
                    var parsedJson = JsonConvert.DeserializeObject(jsonContent);
                    jsonContent = JsonConvert.SerializeObject(parsedJson, Newtonsoft.Json.Formatting.Indented);
                }
                catch { }

                var viewerWindow = new Window
                {
                    Title = $"Просмотр JSON - {System.IO.Path.GetFileName(filename)}",
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var textBox = new System.Windows.Controls.TextBox
                {
                    IsReadOnly = true,
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    FontSize = 12,
                    Text = jsonContent,
                    VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    Margin = new Thickness(5)
                };

                viewerWindow.Content = textBox;
                viewerWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии JSON файла: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===== Партнеры =====

        private void AddPartnerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PartnerDialogWindow(_context);
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void EditPartnerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                var dialog = new PartnerDialogWindow(_context, selectedPartner.Partner);
                if (dialog.ShowDialog() == true)
                {
                    LoadData();
                    LoadPartnerSales(selectedPartner.Id);
                }
            }
            else
            {
                MessageBox.Show("Выберите партнера для редактирования", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeletePartnerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                var result = MessageBox.Show(
                    $"Удалить партнера {selectedPartner.DisplayName}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Partners.Remove(selectedPartner.Partner);
                        _context.SaveChanges();
                        LoadData();
                        SalesDataGrid.ItemsSource = null;

                        MessageBox.Show("Партнер успешно удален", "Информация",
                                       MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}",
                                       "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите партнера для удаления", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ===== Продажи =====

        private void AddSaleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
            {
                var dialog = new SaleDialogWindow(_context, selectedPartner.Id);
                if (dialog.ShowDialog() == true)
                {
                    LoadPartnerSales(selectedPartner.Id);
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Выберите партнера для добавления продажи", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EditSaleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SalesDataGrid.SelectedItem != null)
            {
                if (SalesDataGrid.SelectedItem is SaleViewModel selectedSale)
                {
                    if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
                    {
                        var sale = _context.Sales.Find(selectedSale.Id);
                        if (sale != null)
                        {
                            var dialog = new SaleDialogWindow(_context, selectedPartner.Id, sale);
                            if (dialog.ShowDialog() == true)
                            {
                                LoadPartnerSales(selectedPartner.Id);
                                LoadData();
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите продажу для редактирования", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteSaleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SalesDataGrid.SelectedItem != null)
            {
                if (SalesDataGrid.SelectedItem is SaleViewModel selectedSale)
                {
                    if (PartnersListBox.SelectedItem is PartnerViewModel selectedPartner)
                    {
                        var result = MessageBox.Show("Удалить выбранную продажу?",
                                                    "Подтверждение удаления",
                                                    MessageBoxButton.YesNo,
                                                    MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                var sale = _context.Sales.Find(selectedSale.Id);
                                if (sale != null)
                                {
                                    _context.Sales.Remove(sale);
                                    _context.SaveChanges();
                                    LoadPartnerSales(selectedPartner.Id);
                                    LoadData();
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка удаления: {ex.Message}",
                                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите продажу для удаления", "Предупреждение",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SalesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditSaleMenuItem_Click(sender, null);
        }

        // ===== Статистика и правила =====

        private void StatisticsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int totalPartners = _context.Partners.Count();
                int totalSales = _context.Sales.Count();
                int totalProducts = _context.Products.Count();
                decimal totalRevenue = _context.Sales.Sum(s => (decimal?)s.TotalAmount) ?? 0;

                string stats = "СТАТИСТИКА\n";
                stats += "==========\n\n";
                stats += $"Партнеров: {totalPartners}\n";
                stats += $"Продаж: {totalSales}\n";
                stats += $"Продуктов: {totalProducts}\n";
                stats += $"Выручка: {totalRevenue:N0} руб.\n";

                MessageBox.Show(stats, "Статистика", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RulesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string rules =
                "ПРАВИЛА РАСЧЕТА СКИДКИ\n" +
                "══════════════════════\n\n" +
                "Скидка зависит от общего количества\n" +
                "проданной продукции партнером:\n\n" +
                "▪ до 10 000 шт. → скидка 0%\n" +
                "▪ от 10 000 до 50 000 шт. → скидка 5%\n" +
                "▪ от 50 000 до 300 000 шт. → скидка 10%\n" +
                "▪ более 300 000 шт. → скидка 15%\n\n" +
                "Скидка пересчитывается автоматически\n" +
                "при добавлении или изменении продаж.";

            MessageBox.Show(rules, "Правила расчета скидки",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Программа для учета партнеров\n" +
                "Разработал: Poteha\n" +
                "Версия: 1.0",
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // ===== Горячие клавиши =====

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                LoadData();
                MessageBox.Show("Данные обновлены", "Информация",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (e.Key == Key.F1)
            {
                AboutMenuItem_Click(sender, null);
            }
            else if (e.Key == Key.F2 && PartnersListBox.SelectedItem != null)
            {
                EditPartnerMenuItem_Click(sender, null);
            }
            else if (e.Key == Key.Delete && PartnersListBox.SelectedItem != null)
            {
                DeletePartnerMenuItem_Click(sender, null);
            }
            else if (e.Key == Key.Insert && PartnersListBox.SelectedItem != null)
            {
                AddSaleMenuItem_Click(sender, null);
            }
        }

        // ===== Закрытие окна =====

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из программы?",
                                        "Подтверждение выхода",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
               
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}