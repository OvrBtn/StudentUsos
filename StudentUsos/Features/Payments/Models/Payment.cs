using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLite;
using StudentUsos.Resources.LocalizedStrings;
using System.Globalization;

namespace StudentUsos.Features.Payments.Models
{



    public partial class Payment : ObservableObject
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string SaldoAmount { get; set; }
        public string ChosenInstallmentPlan { get; set; }
        public string WhoChosePlan { get; set; }
        public string DateOfPlanChoice { get; set; }
        public string AvailableInstallmentPlans { get; set; }
        public string TypeId { get; set; }
        public string TypeDescription { get => Utilities.GetLocalizedStringFromJson(TypeDescriptionJson); }
        public string TypeDescriptionJson { get; set; }
        public string Description { get => Utilities.GetLocalizedStringFromJson(DescriptionJson); }
        public string DescriptionJson { get; set; }
        public string State { get; set; }
        public bool IsPaid { get => State == "paid"; }
        public string AccountNumber { get; set; }
        public string PaymentDeadline { get; set; }
        [Ignore]
        public DateTime PaymentDeadlineDateTime
        {
            get
            {
                if (DateTime.TryParseExact(PaymentDeadline, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
                return DateTime.MinValue;
            }
        }
        public string BonusDeadline { get; set; }
        public string BonusAmount { get; set; }
        public string HasBonus { get; set; }
        public string Interest { get; set; }
        public string TotalAmount { get; set; }
        public string CurrencyId { get; set; }
        public string CurrencyName { get => Utilities.GetLocalizedStringFromJson(CurrencyNameJson); }
        public string CurrencyNameJson { get; set; }
        public string FacultyId { get; set; }
        public string FacultyName { get => Utilities.GetLocalizedStringFromJson(FacultyNameJson); }
        public string FacultyNameJson { get; set; }
        public string DefaultChoiceDate { get; set; }

        [RelayCommand]
        void CopyAccountNumberToClipboard()
        {
            Clipboard.Default.SetTextAsync(AccountNumber);
            ApplicationService.Default.ShowToast(LocalizedStrings.PaymentsPage_AccountNumberCopied);
        }
    }



}
