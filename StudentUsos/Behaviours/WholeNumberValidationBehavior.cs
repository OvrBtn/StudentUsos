namespace StudentUsos.Behaviours;

public class WholeNumberValidationBehavior : Behavior<Entry>
{
    protected override void OnAttachedTo(Entry bindable)
    {
        bindable.TextChanged += Bindable_TextChanged;
        base.OnAttachedTo(bindable);
    }

    protected override void OnDetachingFrom(Entry bindable)
    {
        bindable.TextChanged -= Bindable_TextChanged;
        base.OnDetachingFrom(bindable);
    }

    private void Bindable_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        if (string.IsNullOrEmpty(e.NewTextValue) == false)
        {
            bool isWholeNumber = int.TryParse(e.NewTextValue, out int value) && value > 0;
            if (isWholeNumber == false)
            {
                ((Entry)sender).Text = e.OldTextValue;
            }
        }
        else
        {
            ((Entry)sender).Text = string.Empty;
        }
    }
}