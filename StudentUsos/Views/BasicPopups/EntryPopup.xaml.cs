﻿using StudentUsos.Views;

namespace StudentUsos;

public partial class EntryPopup : PopupBase
{
    EntryPopupViewModel viewModel;
    public EntryPopup(EntryPopupParameters parameters)
    {
        InitializeComponent();
        BindingContext = this.viewModel = new EntryPopupViewModel(parameters);
    }

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
            App.Current?.MainPage?.Navigation.PushModalAsync(new EntryPopup(parameters), false);
        });
    }
}