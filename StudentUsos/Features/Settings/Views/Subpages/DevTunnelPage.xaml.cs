using StudentUsos.Controls;
using StudentUsos.Services.ServerConnection;

namespace StudentUsos.Features.Settings.Views.Subpages;

public partial class DevTunnelPage : CustomContentPageNotAnimated
{
    IDevTunnelService devTunnelService;
    public DevTunnelPage(IDevTunnelService devTunnelService)
    {
        this.devTunnelService = devTunnelService;

        InitializeComponent();

        textEdit.Text = devTunnelService.GetDevTunnelId() ?? string.Empty;
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue))
        {
            devTunnelService.DeleteDevTunnel();
        }
        else
        {
            devTunnelService.SaveDevTunnelId(e.NewTextValue);
        }
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        devTunnelService.DeleteDevTunnel();
        textEdit.Text = string.Empty;
    }

    private void TextEdit_TextChanged(object sender, EventArgs e)
    {
        string text = textEdit.Text;
        if (string.IsNullOrEmpty(text))
        {
            devTunnelService.DeleteDevTunnel();
        }
        else
        {
            devTunnelService.SaveDevTunnelId(text);
        }
    }
}