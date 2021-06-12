using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

namespace PolyLineAnimation
{
    public partial class MainWindow : Window
    {
        public static PathGeometry PathGeometrySine { get; set; }

        public static PathGeometry PathGeometryCosine { get; set; }

        public static PathGeometry PathGeometryPinkNoise { get; set; }

        static MainWindow()
        {
            PathGeometrySine = CreatePathGeometrySine();
            PathGeometryCosine = CreatePathGeometryCosine();
            PathGeometryPinkNoise = CreatePathGeometryPinkNoise();
        }

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            //Initialize();
        }

        private void Initialize()
        {
            // SOURCE
#if false
            var sourceFlattened = CreatePathGeometrySine();
#else
            var sourceFlattened = CreatePathGeometryPinkNoise();
#endif

            // TARGET

            var targetFlattened = CreatePathGeometryCosine();

            // CACHE

            var easing = new ElasticEaseOut(); // ExponentialEaseOut, BounceEaseOut, ElasticEaseOut
            var cache = Morph.ToCache(sourceFlattened, targetFlattened, 0.01, easing);

            // UI

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
                    var index = (int) slider.Value;
                    path.Data = cache[index];
                }
            };
            slider.Value = 0;
            path.Data = cache[0];

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1 / 60.0);
            timer.Tick += (sender, e) =>
            {
                if (slider.Value < slider.Maximum)
                {
                    slider.Value++;
                }
                else
                {
                    slider.Value = slider.Minimum;
                }
            };
#if true
            timer.Start();
#else
            slider.IsVisible = true;
#endif
        }

        private static PathGeometry CreatePathGeometryCosine()
        {
            var targetPoints = new List<Point>();
            for (double x = 0; x <= Math.PI * 4; x += 0.01)
            {
                var point = new Point(x, Math.Cos(x) + 1);
                targetPoints.Add(point);
            }

            var target = Morph.CreatePathGeometry(targetPoints);
            var targetFlattened = target;
            //var targetFlattened = target.Flatten(FlattenOutput.PolyLines);  

            return targetFlattened;
        }

        private static PathGeometry CreatePathGeometrySine()
        {
            var sourcePoints = new List<Point>();
            for (double x = 0; x <= Math.PI * 4; x += 0.01)
            {
                var point = new Point(x, Math.Sin(x) + 1);
                sourcePoints.Add(point);
            }

            var source = Morph.CreatePathGeometry(sourcePoints);
            var sourceFlattened = source;
            //var sourceFlattened = source.Flatten(FlattenOutput.PolyLines);

            return sourceFlattened;
        }

        private static PathGeometry CreatePathGeometryPinkNoise()
        {
            var sourcePoints = new List<Point>();
            var pn = new PinkNumber();
            for (double x = 0; x <= Math.PI * 4; x += 0.01)
            {
                var next = pn.GetNextValue();
                var point = new Point(x, next);
                sourcePoints.Add(point);
            }

            var max = sourcePoints.Max(p => p.Y);
            sourcePoints = sourcePoints.Select(p => new Point(p.X, p.Y / max)).ToList();

            var source = Morph.CreatePathGeometry(sourcePoints);
            var sourceFlattened = source;
            //var sourceFlattened = source.Flatten(FlattenOutput.PolyLines);

            return sourceFlattened;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}