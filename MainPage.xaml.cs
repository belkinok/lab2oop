using Lab2_DayOfFaculty.Models;
using Lab2_DayOfFaculty.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using System.Xml.Linq;

namespace Lab2_DayOfFaculty;

public partial class MainPage : ContentPage
{
    private const string RecordElement = "Event";
    private const string RootElement = "FacultyDay";
    private readonly XmlDocumentService _documentService;
    private readonly XmlSearchService _searchService;
    private string _xmlPath = string.Empty;
    private string _xslPath = string.Empty;
    private IReadOnlyList<EventRecord> _allRecords = [];
    private IReadOnlyList<EventRecord> _visibleRecords = [];

    public MainPage(XmlDocumentService documentService, XmlSearchService searchService)
    {
        InitializeComponent();
        _documentService = documentService;
        _searchService = searchService;
        StrategyPicker.ItemsSource = _documentService.StrategyNames.ToList();
        StrategyPicker.SelectedIndex = 2;
        _ = InitializeSampleFilesAsync();
    }

    private async Task InitializeSampleFilesAsync()
    {
        try
        {
            _xmlPath = await CopyAssetToCacheAsync("FacultyDay.xml", "FacultyDay.xml");
            _xslPath = await CopyAssetToCacheAsync("FacultyDay.xsl", "FacultyDay.xsl");
            await LoadDocumentAsync(_xmlPath);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося завантажити демонстраційні файли: {ex.Message}", "OK");
        }
    }

    private static async Task<string> CopyAssetToCacheAsync(string assetName, string targetName)
    {
        var path = Path.Combine(FileSystem.CacheDirectory, targetName);
        await using var source = await FileSystem.OpenAppPackageFileAsync(assetName);
        await using var destination = File.Create(path);
        await source.CopyToAsync(destination);
        return path;
    }

    private async Task LoadDocumentAsync(string path)
    {
        _xmlPath = path;
        var attributes = _documentService.GetAttributeNames(_xmlPath, RecordElement);
        AttributePicker.ItemsSource = attributes.ToList();
        AttributePicker.SelectedIndex = attributes.Count > 0 ? 0 : -1;
        _allRecords = _documentService.Parse(_xmlPath, RecordElement, SelectedStrategy);
        _visibleRecords = _allRecords;
        FileNameLabel.Text = Path.GetFileName(path);
        RenderResults(_visibleRecords);
        StatusLabel.Text = $"Документ завантажено. Записів: {_allRecords.Count}.";
        await Task.CompletedTask;
    }

    private string SelectedStrategy => StrategyPicker.SelectedItem?.ToString() ?? _documentService.StrategyNames[2];

    private void OnAttributeChanged(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_xmlPath) || AttributePicker.SelectedItem is not string attribute) return;
        var values = _documentService.GetAttributeValues(_xmlPath, RecordElement, attribute);
        ValuePicker.ItemsSource = values.ToList();
        ValuePicker.SelectedIndex = -1;
    }

    private async void OnOpenXmlClicked(object? sender, EventArgs e)
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть XML-документ",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    [DevicePlatform.WinUI] = [".xml"],
                    [DevicePlatform.Android] = ["application/xml", "text/xml"],
                    [DevicePlatform.iOS] = ["public.xml"],
                    [DevicePlatform.MacCatalyst] = ["xml"]
                })
            });
            if (file is null) return;
            var localPath = Path.Combine(FileSystem.CacheDirectory, Path.GetFileName(file.FileName));
            await using (var input = await file.OpenReadAsync())
            await using (var output = File.Create(localPath))
                await input.CopyToAsync(output);
            await LoadDocumentAsync(localPath);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка відкриття", ex.Message, "OK");
        }
    }

    private async void OnSearchClicked(object? sender, EventArgs e)
    {
        try
        {
            _allRecords = _documentService.Parse(_xmlPath, RecordElement, SelectedStrategy);
            var attribute = AttributePicker.SelectedItem?.ToString();
            var value = ValuePicker.SelectedItem?.ToString();
            _visibleRecords = _searchService.Search(_allRecords, attribute, value, KeywordEntry.Text);
            RenderResults(_visibleRecords);
            StatusLabel.Text = $"Пошук виконано методом «{SelectedStrategy}». Знайдено: {_visibleRecords.Count}.";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка аналізу XML", ex.Message, "OK");
        }
    }

    private void OnShowAllClicked(object? sender, EventArgs e)
    {
        _visibleRecords = _documentService.Parse(_xmlPath, RecordElement, SelectedStrategy);
        RenderResults(_visibleRecords);
        StatusLabel.Text = "Відображено всі записи документа.";
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        AttributePicker.SelectedIndex = -1;
        ValuePicker.ItemsSource = null;
        ValuePicker.SelectedIndex = -1;
        KeywordEntry.Text = string.Empty;
        _visibleRecords = [];
        RenderResults(_visibleRecords);
        StatusLabel.Text = "Поле результатів і параметри пошуку очищено.";
    }

    private void RenderResults(IEnumerable<EventRecord> records)
    {
        ResultsLayout.Children.Clear();
        var list = records.ToList();
        CountLabel.Text = $"{list.Count} запис(ів)";

        foreach (var record in list)
        {
            var title = record.GetValue("Name");
            var details = record.DisplayText;

            var content = new VerticalStackLayout { Spacing = 6 };

            content.Children.Add(new Label
            {
                Text = title,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#F1F5F9")
            });

            content.Children.Add(new Label
            {
                Text = details,
                FontSize = 13,
                TextColor = Color.FromArgb("#94A3B8")
            });

            ResultsLayout.Children.Add(new Border
            {
                Stroke = Color.FromArgb("#334155"),
                StrokeThickness = 1,
                BackgroundColor = Color.FromArgb("#1E293B"),
                Padding = 14,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Content = content
            });
        }

        if (list.Count == 0)
        {
            ResultsLayout.Children.Add(new Label
            {
                Text = "Нічого не знайдено. Змініть параметри пошуку або натисніть «Усі записи».",
                TextColor = Color.FromArgb("#94A3B8"),
                FontSize = 14,
                Margin = new Thickness(0, 12, 0, 0)
            });
        }
    }

    private async void OnSaveXmlClicked(object? sender, EventArgs e)
    {
        if (_visibleRecords.Count == 0)
        {
            await DisplayAlert("Збереження", "Немає записів для збереження.", "OK");
            return;
        }
        var path = Path.Combine(FileSystem.CacheDirectory, "FilteredFacultyDay.xml");
        try
        {
            _documentService.SaveFilteredXml(_visibleRecords, path, RootElement, RecordElement);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Зберегти відфільтрований XML",
                File = new ShareFile(path)
            });
            StatusLabel.Text = "Відфільтрований XML сформовано.";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка збереження", ex.Message, "OK");
        }
    }

    private async void OnExportHtmlClicked(object? sender, EventArgs e)
    {
        try
        {
            var sourcePath = Path.Combine(FileSystem.CacheDirectory, "FacultyDayForHtml.xml");
            var htmlPath = Path.Combine(FileSystem.CacheDirectory, "FacultyDay.html");
            _documentService.SaveFilteredXml(_visibleRecords, sourcePath, RootElement, RecordElement);
            _documentService.TransformToHtml(sourcePath, _xslPath, htmlPath);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Експортувати HTML",
                File = new ShareFile(htmlPath)
            });
            StatusLabel.Text = "HTML-документ створено на основі XSL-шаблону.";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка трансформації", ex.Message, "OK");
        }
    }

    private async void OnAboutClicked(object? sender, EventArgs e)
    {
        await DisplayAlert("Про програму",
            "Робота з XML-документами «День факультету».\n\nАвтор: Роман Бєлянушкін\nГрупа: К-24\nРік: 2026\n\nЗастосунок виконує пошук у XML трьома способами (SAX, DOM, LINQ to XML), зберігає вибрані записи та трансформує XML у HTML за допомогою XSLT.", "Закрити");
    }

    private async void OnExitClicked(object? sender, EventArgs e)
    {
        var answer = await DisplayAlert("Вихід", "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");
        if (answer) Application.Current?.Quit();
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(() => OnExitClicked(this, EventArgs.Empty));
        return true;
    }
}
