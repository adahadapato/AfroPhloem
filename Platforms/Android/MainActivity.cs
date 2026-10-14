using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;

namespace Phloem;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    // ---------- Pinch-to-zoom (whole app) ----------
    const float MaxZoom = 3f;                 // up to 3x
    ScaleGestureDetector? _zoomDetector;
    float _zoom = 1f, _tx, _ty;               // current zoom and position
    float _lastFocusX, _lastFocusY;
    bool _zoomGestureActive;

    /// <summary>The view that holds the whole app (pages + bottom bar).</summary>
    Android.Views.View? ZoomTarget => FindViewById(Android.Resource.Id.Content);

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Pinch-to-zoom: listens to every touch in the app
        _zoomDetector = new ScaleGestureDetector(this, new ZoomListener(this));

        if (Window is null) return;

        // Phloem deep emerald (BrandDark)
        var green = Android.Graphics.Color.ParseColor("#043D1F");

        // Android 14 and older: colour the status bar directly
        Window.SetStatusBarColor(green);

        // Android 15+: the status bar is see-through, so paint the area behind it green
        Window.DecorView.SetBackgroundColor(green);

        // White clock / battery / signal icons on the dark green
        WindowCompat.GetInsetsController(Window, Window.DecorView).AppearanceLightStatusBars = false;
    }

    /// <summary>Every touch passes through here first, so pinching works on every page.</summary>
    public override bool DispatchTouchEvent(MotionEvent? e)
    {
        if (e is null || _zoomDetector is null)
            return base.DispatchTouchEvent(e);

        _zoomDetector.OnTouchEvent(e);

        if (_zoomDetector.IsInProgress)
        {
            if (!_zoomGestureActive)
            {
                // Tell buttons and lists the finger has gone, so the pinch doesn't tap or scroll anything
                using var cancel = MotionEvent.Obtain(e);
                if (cancel is not null)
                {
                    cancel.Action = MotionEventActions.Cancel;
                    base.DispatchTouchEvent(cancel);
                }
                _zoomGestureActive = true;
            }
            return true;
        }

        // After a pinch, ignore the leftover finger until it's lifted
        if (_zoomGestureActive)
        {
            if (e.ActionMasked is MotionEventActions.Up or MotionEventActions.Cancel)
                _zoomGestureActive = false;
            return true;
        }

        return base.DispatchTouchEvent(e);
    }

    bool OnZoomBegin(ScaleGestureDetector d)
    {
        _lastFocusX = d.FocusX;
        _lastFocusY = d.FocusY;
        return true;
    }

    bool OnZoom(ScaleGestureDetector d)
    {
        var view = ZoomTarget;
        if (view is null) return false;

        float newZoom = Math.Clamp(_zoom * d.ScaleFactor, 1f, MaxZoom);

        // Keep the spot between your fingers under your fingers (also lets two fingers drag around)
        float pointX = (_lastFocusX - view.Left - _tx) / _zoom;
        float pointY = (_lastFocusY - view.Top - _ty) / _zoom;
        _tx = (d.FocusX - view.Left) - pointX * newZoom;
        _ty = (d.FocusY - view.Top) - pointY * newZoom;
        _zoom = newZoom;

        // Don't let the app slide off the screen
        _tx = Math.Clamp(_tx, view.Width - view.Width * _zoom, 0f);
        _ty = Math.Clamp(_ty, view.Height - view.Height * _zoom, 0f);

        Apply(view);
        _lastFocusX = d.FocusX;
        _lastFocusY = d.FocusY;
        return true;
    }

    void OnZoomEnd()
    {
        // Pinched (almost) all the way back out: snap to normal
        if (_zoom < 1.05f)
        {
            _zoom = 1f; _tx = 0f; _ty = 0f;
            if (ZoomTarget is { } view) Apply(view);
        }
    }

    void Apply(Android.Views.View view)
    {
        view.PivotX = 0f;
        view.PivotY = 0f;
        view.ScaleX = _zoom;
        view.ScaleY = _zoom;
        view.TranslationX = _tx;
        view.TranslationY = _ty;
    }

    class ZoomListener : ScaleGestureDetector.SimpleOnScaleGestureListener
    {
        readonly MainActivity _activity;
        public ZoomListener(MainActivity activity) => _activity = activity;

        public override bool OnScaleBegin(ScaleGestureDetector detector) => _activity.OnZoomBegin(detector);
        public override bool OnScale(ScaleGestureDetector detector) => _activity.OnZoom(detector);
        public override void OnScaleEnd(ScaleGestureDetector detector) => _activity.OnZoomEnd();
    }
}