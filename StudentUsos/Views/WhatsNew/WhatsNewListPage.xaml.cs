﻿using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.Settings.Views.Subpages;

namespace StudentUsos.Views.WhatsNew;

public partial class WhatsNewListPage : ContentPage
{
    INavigationService navigationService;
    public WhatsNewListPage(INavigationService navigationService)
    {
        InitializeComponent();

        var content = contentStackLayout.Children;
        for (int i = 0; i < content.Count; i++)
        {
            if (i == content.Count - 1)
            {
                break;
            }

            StackLayout separator = new()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromArgb("#444444"),
                HeightRequest = 1,
                Margin = new(0, 30)
            };

            content.Insert(i + 1, separator);
            i++;
        }

        this.navigationService = navigationService;
    }

    const int CurrentId = 2;
    public static void Initialize(ILocalStorageManager localStorageManager, INavigationService navigationService)
    {
        if (AuthorizationService.CheckIfSignedInAndRetrieveTokens() == false || AuthorizationService.HasJustLoggedIn)
        {
            localStorageManager.SetData(LocalStorageKeys.WhatsNewCarouselLastId, CurrentId.ToString());
            return;
        }

        if (CurrentId == 0 || (localStorageManager.TryGettingData(LocalStorageKeys.WhatsNewListLastId, out string lastId) && lastId == CurrentId.ToString()))
        {
            return;
        }

        navigationService.PushModalAsync<WhatsNewListPage>();
        localStorageManager.SetData(LocalStorageKeys.WhatsNewListLastId, CurrentId.ToString());
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        _ = Navigation.PopModalAsync();
    }

    private void GoToSettingsButton_Clicked(object sender, EventArgs e)
    {
        _ = navigationService.PushAsync<NotificationsSubpage>();
    }
}