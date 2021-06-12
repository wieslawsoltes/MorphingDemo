using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace PolyLineAnimation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            

            var sourcePoints = new List<Point>();
            for (double x = 0; x <= Math.PI * 4; x += 0.01)
            {
                var point = new Point(x, Math.Sin(x) + 1);
                sourcePoints.Add(point);
            }
            var source = CreatePathGeometry(sourcePoints);
            //source = source.Flatten(FlattenOutput.PolyLines);


            var targetPoints = new List<Point>();
            for (double x = 0; x <= Math.PI * 4; x += 0.01)
            {
                var point = new Point(x, Math.Cos(x) + 1);
                targetPoints.Add(point);
            }
            var target = CreatePathGeometry(targetPoints);
            //target = target.Flatten(FlattenOutput.PolyLines);     

            

            var speed = 0.01;
            var easing = new ExponentialEaseOut();
            int steps = (int)(1 / speed);
            double p = speed;
            var cache = new List<PathGeometry>(steps);

            var sourceFigure = source.Figures[0];
            var sourceSegment = sourceFigure.Segments[0] as PolyLineSegment;
            var targetFigure = target.Figures[0];
            var targetSegment = targetFigure.Segments[0] as PolyLineSegment;

            
            Debug.Assert(sourceSegment.Points.Count == targetSegment.Points.Count);

            
            
            cache.Add(source.ClonePathGeometry());
            for (int i = 0; i < steps; i++)
            {
                var clone = source.ClonePathGeometry();
                var progress = easing.Ease(p);

                var cloneFigure = clone.Figures[0];
                var cloneSegment = cloneFigure.Segments[0] as PolyLineSegment; 

                cloneFigure.StartPoint =  Interpolate(sourceFigure.StartPoint, targetFigure.StartPoint, progress);
   
                for (int j = 0; j < sourceSegment.Points.Count; j++)
                {
                    cloneSegment.Points[j] =  Interpolate(sourceSegment.Points[j], targetSegment.Points[j], progress);
                }

                p += speed;

                cache.Add(clone);
            }
            cache.Add(target.ClonePathGeometry());
            
            
            
            
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
        }

        private static double Interpolate(double from, double to, double progress)
        {
            return from + (to - from) * progress;
        }

        private static Point Interpolate(Point from, Point to, double progress)
        {
            var x = Interpolate(from.X,  to.X, progress);
            var y = Interpolate(from.Y, to.Y, progress);
            return new Point(x, y);
        }

        private static PathGeometry CreatePathGeometry(IList<Point> sourcePoints)
        {
            var source = new PathGeometry()
            {
                FillRule = FillRule.EvenOdd
            };

            var sourceFigure = new PathFigure()
            {
                IsClosed = false,
                IsFilled = false,
                StartPoint = sourcePoints.First()
            };
            source.Figures.Add(sourceFigure);

            var polylineSegment = new PolyLineSegment(sourcePoints.Skip(1));
            sourceFigure.Segments?.Add(polylineSegment);

            return source;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}