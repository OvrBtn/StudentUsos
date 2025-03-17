using CommunityToolkit.Maui.Views;

namespace StudentUsos;

public partial class MessagePopupPage : Popup
{

    public MessagePopupPage(YesAndNoPopupParameters parameters)
    {
        InitializeComponent();
        BindingContext = new MessagePopupPageViewModel(this, parameters);
    }

    public MessagePopupPage(OkPopupParameters parameters)
    {
        InitializeComponent();
        BindingContext = new MessagePopupPageViewModel(this, parameters);
    }

    public struct YesAndNoPopupParameters
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Yes { get; set; }
        public string No { get; set; }
        public Action? YesAction { get; set; }
        public Action? NoAction { get; set; }
    }

    public struct OkPopupParameters
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Ok { get; set; }
        public Action? OkAction { get; set; }
    }


    public static void CreateAndShow(string title, string description, string yes, string no, Action? yesAction, Action? noAction)
    {
        var parameters = new YesAndNoPopupParameters()
        {
            Title = title,
            Description = description,
            Yes = yes,
            No = no,
            YesAction = yesAction,
            NoAction = noAction

        };
        ApplicationService.Default.MainThreadInvoke(() =>
        {
            var popup = new MessagePopupPage(parameters);
            App.Current?.MainPage?.ShowPopup(popup);
        });
    }

    public static void CreateAndShow(string title, string description, string ok, Action? okAction = null)
    {
        var parameters = new OkPopupParameters()
        {
            Title = title,
            Description = description,
            Ok = ok,
            OkAction = okAction,

        };
        ApplicationService.Default.MainThreadInvoke(() =>
        {
            var popup = new MessagePopupPage(parameters);
            App.Current?.MainPage?.ShowPopup(popup);
        });
    }
}