using StudentUsos.Resources.LocalizedStrings;

namespace StudentUsos.Features.Person.Models;

public class PhoneNumber
{

    public string Number { get; set; }
    public Command CopyNumberCommand { get; init; }
    public PhoneNumber(string number)
    {
        Number = number;
        CopyNumberCommand = new(CopyToClipboard);
    }

    void CopyToClipboard()
    {
        Clipboard.Default.SetTextAsync(Number);
        ApplicationService.Default.ShowToast(LocalizedStrings.PersonDetailsPage_PhoneNumberCopied);
    }
}