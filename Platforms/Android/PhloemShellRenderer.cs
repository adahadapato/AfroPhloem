using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;

namespace Phloem.Platforms;

/// <summary>
/// Android only: gives the Shell bottom bar (Home · Explore · Orders · Account)
/// rounded top corners, like the design. Registered in MauiProgram.cs.
/// </summary>
public class PhloemShellRenderer : ShellRenderer
{
    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        => new RoundedBottomNavTracker(this, shellItem);
}

class RoundedBottomNavTracker : ShellBottomNavViewAppearanceTracker
{
    const double CornerRadius = 24;          // roundness of the top corners
    const string BarColor = "#002F1D";   // BrandNavGreen
    const string BehindColor = "#F8F5EF";   // BrandCream: shows in the corner gaps

    public RoundedBottomNavTracker(IShellContext shellContext, ShellItem shellItem)
        : base(shellContext, shellItem) { }

    public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
    {
        base.SetAppearance(bottomView, appearance); // keeps your tab colours and icons
        ApplyRoundedCorners(bottomView);
    }

    public override void ResetAppearance(BottomNavigationView bottomView)
    {
        base.ResetAppearance(bottomView);
        ApplyRoundedCorners(bottomView);
    }

    static void ApplyRoundedCorners(BottomNavigationView bottomView)
    {
        var r = (float)bottomView.Context!.ToPixels(CornerRadius);

        var shape = new global::Android.Graphics.Drawables.GradientDrawable();
        shape.SetColor(global::Android.Graphics.Color.ParseColor(BarColor));
        // top-left, top-right, bottom-right, bottom-left (x,y pairs)
        shape.SetCornerRadii(new[] { r, r, r, r, 0f, 0f, 0f, 0f });

        bottomView.Background = shape;
        bottomView.Elevation = 0;

        // The area behind the rounded corners: cream, so the curve is visible
        if (bottomView.Parent is global::Android.Views.View parent)
            parent.SetBackgroundColor(global::Android.Graphics.Color.ParseColor(BehindColor));
    }
}
