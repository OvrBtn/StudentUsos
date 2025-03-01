
namespace StudentUsos.ViewModels
{
    public class DatePickerPopupViewModel : PopupViewModelBase
    {
        public DatePickerPopup.DatePickerParameters Parameters { get; private set; }
        public DatePickerPopupViewModel(DatePickerPopup.DatePickerParameters parameters)
        {
            Parameters = parameters;
        }
    }
}
