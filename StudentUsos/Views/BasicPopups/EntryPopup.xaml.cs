using CommunityToolkit.Maui.Views;

namespace StudentUsos;

public partial class EntryPopup : Popup
{
    EntryPopupViewModel viewModel;
    public EntryPopup(EntryPopupParameters parameters)
    {
        InitializeComponent();
        this.parameters = parameters;
        BindingContext = this.viewModel = new EntryPopupViewModel(this, parameters);
    }

    EntryPopupParameters parameters;

    public struct EntryPopupParameters
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Confirm { get; set; }
        public string Cancel { get; set; }
        public Action<string>? ConfirmAction { get; set; }
        public Action? CancelAction { get; set; }
        public Keyboard? Keyboard { get; set; }
    }

    public static void CreateAndShow(string title,
        string description,
        string confirm,
        string cancel,
        Action<string>? confirmAction,
        Action? cancelAction = null,
        Keyboard? keyboard = null)
    {
        var parameters = new EntryPopupParameters()
        {
            Title = title,
            Description = description,
            Confirm = confirm,
            Cancel = cancel,
            ConfirmAction = confirmAction,
            CancelAction = cancelAction,
            Keyboard = keyboard
        };
        ApplicationService.Default.MainThreadInvoke(() =>
        {
            var popup = new EntryPopup(parameters);
            App.Current?.Windows[0].Page?.ShowPopup(popup);
        });
    }

    protected override Task OnClosed(object? result, bool wasDismissedByTappingOutsideOfPopup, CancellationToken token = default)
    {
        //this also somehow handles navigation bar back button
        if (wasDismissedByTappingOutsideOfPopup)
        {
            parameters.CancelAction?.Invoke();
        }
        return base.OnClosed(result, wasDismissedByTappingOutsideOfPopup, token);
    }
}