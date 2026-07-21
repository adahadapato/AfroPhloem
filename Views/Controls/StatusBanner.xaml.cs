using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroPhloem.Views.Controls;

	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StatusBanner : ContentView
	{
		public StatusBanner ()
		{
			InitializeComponent ();
            Loaded += (_, _) => ApplyState();
        }

    public static readonly BindableProperty MessageProperty =
    BindableProperty.Create(nameof(Message), typeof(string), typeof(StatusBanner), string.Empty);

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly BindableProperty IsErrorProperty =
        BindableProperty.Create(nameof(IsError), typeof(bool), typeof(StatusBanner), false,
            propertyChanged: OnIsErrorChanged);

    public bool IsError
    {
        get => (bool)GetValue(IsErrorProperty);
        set => SetValue(IsErrorProperty, value);
    }

    private static void OnIsErrorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StatusBanner banner)
            banner.ApplyState();
    }

    private void ApplyState()
    {
        VisualStateManager.GoToState(StatusBorder, IsError ? "Error" : "Success");
    }

}