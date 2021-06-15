using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using WPFAnimations;

namespace PolyLineAnimation
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            //Animation.RegisterAnimator<MorphAnimator>(prop => typeof(Geometry).IsAssignableFrom(prop.PropertyType));
            Animation.RegisterAnimator<PolyLineAnimator>(prop => typeof(Geometry).IsAssignableFrom(prop.PropertyType));

            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
        }
    }
}