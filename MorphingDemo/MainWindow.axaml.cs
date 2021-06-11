using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MorphingDemo.Avalonia;
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
            var sourceFlattened = source.Flatten(FlattenOutput.PolyLines);
            //var sourceFlattened = source.Flatten(FlattenOutput.Lines);

            var target = PathGeometry.Parse("M10 3C6.13401 3 3 6.13401 3 10C3 13.866 6.13401 17 10 17C13.866 17 17 13.866 17 10C17 6.13401 13.866 3 10 3ZM2 10C2 5.58172 5.58172 2 10 2C14.4183 2 18 5.58172 18 10C18 14.4183 14.4183 18 10 18C5.58172 18 2 14.4183 2 10Z");
            var targetFlattened = target.Flatten(FlattenOutput.PolyLines);
            //var targetFlattened = target.Flatten(FlattenOutput.Lines);

            var cache = Morph.ToCache(sourceFlattened, targetFlattened, 0.01);

            var path = this.FindControl<Path>("Path");
            var slider = this.FindControl<Slider>("Slider");

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