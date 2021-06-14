using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using WPFAnimations;

namespace MorphingDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            var source = PathGeometry.Parse("M3 6C3 4.34315 4.34315 3 6 3H14C15.6569 3 17 4.34315 17 6V14C17 15.6569 15.6569 17 14 17H6C4.34315 17 3 15.6569 3 14V6ZM6 4C4.89543 4 4 4.89543 4 6V14C4 15.1046 4.89543 16 6 16H14C15.1046 16 16 15.1046 16 14V6C16 4.89543 15.1046 4 14 4H6Z");
            //var source = PathGeometry.Parse("M 0, 0 L 1, 0 2, 0 3, 0 4, 0 5, 0 6, 0 7, 0 8, 0 9, 0 10, 0 11, 0 12, 0 13, 0 14, 0 15, 0 M 15, 0 L 16, 0 17, 0 18, 0 19, 0 20, 0 21, 0 22, 0 23, 0 24, 0 25, 0 26, 0 27, 0 28, 0 29, 0 30, 0 M 30, 0 L 31, 0 32, 0 33, 0 34, 0 35, 0 36, 0 37, 0 38, 0 39, 0 40, 0 41, 0 42, 0 43, 0 44, 0 45, 0");
            var sourceFlattened = source.GetFlattenedPathGeometry();

            var target = PathGeometry.Parse("M10 3C6.13401 3 3 6.13401 3 10C3 13.866 6.13401 17 10 17C13.866 17 17 13.866 17 10C17 6.13401 13.866 3 10 3ZM2 10C2 5.58172 5.58172 2 10 2C14.4183 2 18 5.58172 18 10C18 14.4183 14.4183 18 10 18C5.58172 18 2 14.4183 2 10Z");
            //var target = PathGeometry.Parse("M0 0C0 30 45 30 45 0");
            var targetFlattened = target.GetFlattenedPathGeometry();

            var cache = Morph.ToCache(sourceFlattened, targetFlattened, 0.01);

            var path = this.FindControl<Path>("path");
            var slider = this.FindControl<Slider>("slider");

            slider.Minimum = 0;
            slider.Maximum = cache.Count - 1;
            slider.SmallChange = 1;
            slider.LargeChange = 10;
            slider.TickFrequency = 1;
            slider.IsSnapToTickEnabled = true;
            slider.PropertyChanged += (_, args) =>
            {
                if (args.Property == Slider.ValueProperty)
                {
                    var index = (int)slider.Value;
                    path.Data = cache[index];
                }
            };
            slider.Value = 0;
            path.Data = cache[0];

            // DEBUG
            //path.Data = source;
            //path.Data = sourceFlattened;
            //path.Data = target;
            //path.Data = targetFlattened;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}