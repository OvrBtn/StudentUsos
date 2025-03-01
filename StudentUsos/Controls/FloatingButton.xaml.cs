namespace StudentUsos.Controls
{
    public partial class FloatingButton : ContentView
    {
        public static readonly BindableProperty ButtonTemplateProperty = BindableProperty.Create(nameof(ButtonTemplate), typeof(View), typeof(FloatingButton));
        public View ButtonTemplate
        {
            get => (View)GetValue(ButtonTemplateProperty);
            set => SetValue(ButtonTemplateProperty, value);
        }

        public static readonly BindableProperty ContentTemplateProperty = BindableProperty.Create(nameof(ContentTemplate), typeof(View), typeof(FloatingButton), propertyChanged: OnContentTemplatePropertyChanged);
        public View ContentTemplate
        {
            get => (View)GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }

        static void OnContentTemplatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is FloatingButton floatingButton)
            {
                floatingButton.SetContentTemplateBindingContext();
            }
        }

        void SetContentTemplateBindingContext()
        {
            ContentTemplate.BindingContext = this.Parent?.BindingContext;
        }

        public static readonly BindableProperty ButtonCornerRadiusProperty = BindableProperty.Create(nameof(ButtonCornerRadius), typeof(int), typeof(CustomButton), 15);
        public int ButtonCornerRadius
        {
            get => (int)GetValue(ButtonCornerRadiusProperty);
            set => SetValue(ButtonCornerRadiusProperty, value);
        }

        public static readonly BindableProperty AnimationLengthProperty = BindableProperty.Create(nameof(AnimationLength), typeof(int), typeof(CustomButton), 300);
        public int AnimationLength
        {
            get => (int)GetValue(AnimationLengthProperty);
            set => SetValue(AnimationLengthProperty, value);
        }

        public static readonly BindableProperty AnimationSpacingProperty = BindableProperty.Create(nameof(AnimationSpacing), typeof(int), typeof(CustomButton), 300);
        public int AnimationSpacing
        {
            get => (int)GetValue(AnimationSpacingProperty);
            set => SetValue(AnimationSpacingProperty, value);
        }

        public event Action ButtonClicked;

        public FloatingButton()
        {
            InitializeComponent();
            this.Loaded += FloatingButton_Loaded;
        }

        void FloatingButton_Loaded(object? sender, EventArgs e)
        {
            CustomButton_OnClick();

            SetContentTemplateBindingContext();
        }

        public bool AreButtonsVisible
        {
            get => areButtonsVisible;
            set
            {
                areButtonsVisible = value;
                OnPropertyChanged(nameof(AreButtonsVisible));
            }
        }
        bool areButtonsVisible = true;
        bool isAnimating = false;
        bool firstExecution = true;
        private async void CustomButton_OnClick()
        {
            if (isAnimating) return;
            if (firstExecution == false)
            {
                ButtonClicked?.Invoke();
            }
            isAnimating = true;
            var childElements = contentButtons.Content;
            if (childElements == null)
            {
                isAnimating = false;
                return;
            }
            var buttons = (childElements as Layout).Children.ToList();
            if (AreButtonsVisible == false)
            {

                for (int i = buttons.Count - 1; i >= 0; i--)
                {
                    if (buttons[i] is VisualElement visualElement)
                    {
                        AnimateButton(visualElement, i);
                        await Task.Delay(AnimationSpacing);
                    }
                }
            }
            else
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i] is VisualElement visualElement)
                    {
                        AnimateButton(visualElement, i);
                        await Task.Delay(AnimationSpacing);
                    }
                }
            }

            int lastAnimationFinishTime = (buttons.Count - 1) * AnimationSpacing + AnimationLength;
            await Task.Delay(lastAnimationFinishTime);
            AreButtonsVisible = !AreButtonsVisible;
            isAnimating = false;
            await Task.Delay(AnimationSpacing + AnimationLength);
            contentButtons.IsVisible = true;
            firstExecution = false;
        }

        void AnimateButton(VisualElement visualElement, int childNumber)
        {
            bool areButtonsVisibleLocal = AreButtonsVisible;
            visualElement.InputTransparent = areButtonsVisibleLocal;
            if (firstExecution)
            {
                if (areButtonsVisibleLocal == false)
                {
                    visualElement.Opacity = 1;
                }
                else
                {
                    visualElement.Opacity = 0;
                }
                return;
            }
            Animation animation = new((progress) =>
            {
                if (areButtonsVisibleLocal == false)
                {
                    visualElement.Opacity = progress;
                }
                else
                {
                    visualElement.Opacity = 1 - progress;
                }
            });
            animation.Commit(this, "FloatingButtonAnimationButton" + childNumber, 16, (uint)AnimationLength, Easing.Linear);
        }
    }
}