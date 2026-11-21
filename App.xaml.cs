namespace Lab2_DayOfFaculty;

public partial class App : Application
{
    public App(MainPage page)
    {
        InitializeComponent();
        MainPage = new NavigationPage(page);
    }
}
