using System.Text.Json.Serialization;
using StudentUsos.Features.Payments.Models;

namespace StudentUsos.Features.Payments.Services
{
    [JsonSerializable(typeof(List<PaymentJson>))]
    internal partial class PaymentJsonContext : JsonSerializerContext
    {

    }

    internal class PaymentJson
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("user")]
        public Person.Models.Person User { get; set; }
        [JsonPropertyName("saldo_amount"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string SaldoAmount { get; set; }
        [JsonPropertyName("chosen_installment_plan")]
        public string ChosenInstallmentPlan { get; set; }
        [JsonPropertyName("who_chose_plan")]
        public string WhoChosePlan { get; set; }
        [JsonPropertyName("date_of_plan_choice")]
        public string DateOfPlanChoice { get; set; }
        [JsonPropertyName("available_installment_plans")]
        public string AvailableInstallmentPlans { get; set; }
        [JsonPropertyName("type")]
        public PaymentType Type { get; set; }
        [JsonPropertyName("description"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string DescriptionJson { get; set; }
        [JsonPropertyName("state")]
        public string State { get; set; }
        [JsonPropertyName("account_number")]
        public string AccountNumber { get; set; }
        [JsonPropertyName("payment_deadline")]
        public string PaymentDeadline { get; set; }
        [JsonPropertyName("bonus_deadline")]
        public string BonusDeadline { get; set; }
        [JsonPropertyName("bonus_amount")]
        public string BonusAmount { get; set; }
        [JsonPropertyName("has_bonus"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string HasBonus { get; set; }
        /// <summary>
        /// WARNING: USOS API is inconsistent with value of this property, sometimes it's a float (also returning 0.0 when no interest) and sometimes it's a null
        /// </summary>
        [JsonPropertyName("interest"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string Interest { get; set; }
        [JsonPropertyName("total_amount"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string TotalAmount { get; set; }
        [JsonPropertyName("currency")]
        public PaymentCurrency Currency { get; set; }
        [JsonPropertyName("faculty")]
        public Faculty Faculty { get; set; }
        [JsonPropertyName("default_choice_date")]
        public string DefaultChoiceDate { get; set; }
    }

    internal class PaymentType
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("description"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string Description { get; set; }
    }

    internal class PaymentCurrency
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string Name { get; set; }
    }

    internal static class PaymentJsonExtensions
    {
        internal static Payment ToPayment(this PaymentJson paymentJson)
        {
            return new()
            {
                Id = paymentJson.Id,
                UserId = paymentJson.User.Id,
                UserFirstName = paymentJson.User.FirstName,
                UserLastName = paymentJson.User.LastName,
                SaldoAmount = paymentJson.SaldoAmount,
                ChosenInstallmentPlan = paymentJson.ChosenInstallmentPlan,
                WhoChosePlan = paymentJson.WhoChosePlan,
                DateOfPlanChoice = paymentJson.DateOfPlanChoice,
                AvailableInstallmentPlans = paymentJson.AvailableInstallmentPlans,
                TypeId = paymentJson.Type.Id,
                TypeDescriptionJson = paymentJson.Type.Description,
                DescriptionJson = paymentJson.DescriptionJson,
                State = paymentJson.State,
                AccountNumber = paymentJson.AccountNumber,
                PaymentDeadline = paymentJson.PaymentDeadline,
                BonusDeadline = paymentJson.BonusDeadline,
                BonusAmount = paymentJson.BonusAmount,
                HasBonus = paymentJson.HasBonus,
                Interest = paymentJson.Interest,
                TotalAmount = paymentJson.TotalAmount,
                CurrencyId = paymentJson.Currency.Id,
                CurrencyNameJson = paymentJson.Currency.Name,
                FacultyId = paymentJson.Faculty.Id,
                FacultyNameJson = paymentJson.Faculty.NameJson,
                DefaultChoiceDate = paymentJson.DefaultChoiceDate
            };
        }
    }
}
