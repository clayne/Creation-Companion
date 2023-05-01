using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using ReactiveUI;
namespace CreationEditor.Avalonia.Behavior;

public sealed class ListBoxAutoScrollToBottom : Behavior<ListBox> {
    public static readonly StyledProperty<bool> ScrollingEnabledProperty = AvaloniaProperty.Register<ListBoxAutoScrollToBottom, bool>(nameof(ScrollingEnabled));

    public bool ScrollingEnabled {
        get => GetValue(ScrollingEnabledProperty);
        set => SetValue(ScrollingEnabledProperty, value);
    }

    private IDisposable? _attachedDisposable;

    protected override void OnAttachedToVisualTree() {
        base.OnAttached();

        _attachedDisposable = AssociatedObject?.WhenAnyValue(x => x.ItemCount)
            .Throttle(TimeSpan.FromMicroseconds(250), RxApp.MainThreadScheduler)
            .Subscribe(_ => {
                if (!ScrollingEnabled) return;

                switch (AssociatedObject.Scroll) {
                    case ScrollViewer scrollViewer:
                        scrollViewer.ScrollToEnd();
                        break;
                }
            });
    }

    protected override void OnDetachedFromVisualTree() {
        base.OnDetaching();

        _attachedDisposable?.Dispose();
    }
}
