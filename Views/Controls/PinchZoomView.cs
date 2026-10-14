namespace Phloem.Views.Controls;

/// <summary>
/// Wraps a page so it can be zoomed with two fingers (pinch), like zooming a web page.
///  • Spread two fingers  → zoom in (up to 3×), around the point between your fingers.
///  • While zoomed        → drag with one finger to move around.
///  • Pinch back to 1×    → everything snaps back to normal and scrolling works again.
/// Applied to every page through the ContentPage style in Styles.xaml.
/// </summary>
public class PinchZoomView : ContentView
{
    public const double MaxScale = 3.0;

    double _startScale = 1, _currentScale = 1;
    double _xOffset, _yOffset;
    readonly PanGestureRecognizer _pan = new();
    bool _panAttached;

    public PinchZoomView()
    {
        var pinch = new PinchGestureRecognizer();
        pinch.PinchUpdated += OnPinchUpdated;
        GestureRecognizers.Add(pinch);

        _pan.PanUpdated += OnPanUpdated;
    }

    void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        if (Content is null) return;

        switch (e.Status)
        {
            case GestureStatus.Started:
                _startScale = Content.Scale;
                Content.AnchorX = 0;
                Content.AnchorY = 0;
                break;

            case GestureStatus.Running:
                _currentScale += (e.Scale - 1) * _startScale;
                _currentScale = Math.Clamp(_currentScale, 1, MaxScale);

                // Keep the point between the fingers in place while zooming
                double renderedX = Content.X + _xOffset;
                double deltaX = renderedX / Width;
                double deltaWidth = Width / (Content.Width * _startScale);
                double originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                double renderedY = Content.Y + _yOffset;
                double deltaY = renderedY / Height;
                double deltaHeight = Height / (Content.Height * _startScale);
                double originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                double targetX = _xOffset - (originX * Content.Width) * (_currentScale - _startScale);
                double targetY = _yOffset - (originY * Content.Height) * (_currentScale - _startScale);

                Content.TranslationX = Math.Clamp(targetX, -Content.Width * (_currentScale - 1), 0);
                Content.TranslationY = Math.Clamp(targetY, -Content.Height * (_currentScale - 1), 0);
                Content.Scale = _currentScale;
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (_currentScale <= 1.02)
                    Reset();                 // back to normal
                else
                {
                    _xOffset = Content.TranslationX;
                    _yOffset = Content.TranslationY;
                    AttachPan(true);         // allow one-finger dragging while zoomed
                }
                break;
        }
    }

    void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (Content is null || _currentScale <= 1) return;

        switch (e.StatusType)
        {
            case GestureStatus.Running:
                Content.TranslationX = Math.Clamp(_xOffset + e.TotalX, -Content.Width * (_currentScale - 1), 0);
                Content.TranslationY = Math.Clamp(_yOffset + e.TotalY, -Content.Height * (_currentScale - 1), 0);
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _xOffset = Content.TranslationX;
                _yOffset = Content.TranslationY;
                break;
        }
    }

    /// <summary>Back to 1× (also used when the page is shown again).</summary>
    public void Reset()
    {
        _currentScale = _startScale = 1;
        _xOffset = _yOffset = 0;
        if (Content is not null)
        {
            Content.Scale = 1;
            Content.TranslationX = 0;
            Content.TranslationY = 0;
        }
        AttachPan(false);   // normal scrolling works again
    }

    // The pan gesture is only added while zoomed in, so it never blocks normal page scrolling.
    void AttachPan(bool attach)
    {
        if (attach && !_panAttached) { GestureRecognizers.Add(_pan); _panAttached = true; }
        else if (!attach && _panAttached) { GestureRecognizers.Remove(_pan); _panAttached = false; }
    }
}
