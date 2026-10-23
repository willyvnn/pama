using System.Windows;
using System.Windows.Media.Animation;

namespace Pama.Desktop.Startup;

/// <summary>Startup card with a progress bar; driven by <see cref="SplashController"/>.</summary>
public partial class SplashWindow : Window
{
    private static readonly Duration StepAnimation = new(TimeSpan.FromMilliseconds(250));

    public SplashWindow(string version)
    {
        InitializeComponent();
        VersionText.Text = version;
    }

    public void ShowStep(double fraction, string text)
    {
        StepText.Text = text;
        var trackWidth = ((FrameworkElement)ProgressFill.Parent).ActualWidth;
        ProgressFill.BeginAnimation(WidthProperty, new DoubleAnimation(trackWidth * fraction, StepAnimation));
    }
}
