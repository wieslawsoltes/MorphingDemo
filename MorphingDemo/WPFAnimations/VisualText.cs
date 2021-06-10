using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace WPFAnimations.Visuals
{
    public class SimpleVisualText : VisualBase
    {
        private Panel canvas;
        
        public string Text { get; set; }
        public TextBlock Block { get; set; }

        public LinearGradientBrush Fill { get; set; }
        public SolidColorBrush Stroke { get; set; }

        public GradientStop StartColor { get; set; }
        public GradientStop EndColor { get; set; }
        public Color PrimaryColor { get; set; }

        public DoubleCollection StrokeArray { get; set; }
        public double StrokeDashOffset { get; set; }

        public double FontSize { get; set; }
        public string FontName { get; set; }

        public bool IsOptimized { get; set; }

        private SimpleVisualText() : base(null) { }

        public SimpleVisualText(double x, double y, Panel canvas, Dispatcher dispatcher) : base(dispatcher)
        {
            X = x;
            Y = y;
            this.canvas = canvas;
        }

        public void Create(string text, Color color,
           string font = "Nexa Bold",
           double fontSize = 88)
        {
            Text = text;
            FontSize = fontSize;
            FontName = font;

            dispatcher.Invoke(() =>
            {
                var stopStart = new GradientStop() { Color = color, Offset = -1 };
                var stopEnd = new GradientStop() { Color = Colors.Transparent, Offset = -1 };

                LinearGradientBrush linearGradientBrush = new LinearGradientBrush();
                linearGradientBrush.StartPoint = new Point(0, 0);
                linearGradientBrush.EndPoint = new Point(1, 0);
                linearGradientBrush.GradientStops = new GradientStopCollection();
                linearGradientBrush.GradientStops.Add(stopStart);
                linearGradientBrush.GradientStops.Add(stopEnd);

                Block = new TextBlock();
                Block.Text = text;
                Block.FontFamily = new FontFamily(FontName);
                Block.FontSize = FontSize;
                Block.Foreground = linearGradientBrush;

                var group = new TransformGroup();

                TranslateTransform = new TranslateTransform() { X = this.X, Y = this.Y };
                ScaleTransform = new ScaleTransform();
                RotateTransform = new RotateTransform();

                group.Children.Add(TranslateTransform);
                group.Children.Add(ScaleTransform);
                group.Children.Add(RotateTransform);

                Block.RenderTransform = group;

                StartColor = stopStart;
                EndColor = stopEnd;
                Fill = linearGradientBrush;

                PrimaryColor = color;

                canvas.Children.Add(Block);
            });
        }

        public void Remove()
        {
            dispatcher.Invoke(() =>
            {
                canvas.Children.Remove(Block);
            });
        }

        public void Optimize()
        {
            dispatcher.Invoke(() =>
            {
                Block.Foreground = new SolidColorBrush(PrimaryColor);
                Block.Foreground.Freeze();

                IsOptimized = true;
            });
        }

        public void UnOptimize()
        {
            dispatcher.Invoke(() =>
            {
                Block.Foreground = Fill;
                IsOptimized = false;
            });
        }

        public void Show(double speed = 400, double delay = 0)
        {
            ShowFill(speed * 2, delay + (speed / 4));
        }

        public void Hide(double speed = 400, double delay = 0)
        {
            HideFill(speed * 2, delay + (speed / 4));
        }

        public void Freeze() { }

        public void HideOpacity(double speed = 400, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                this.Block.BeginAnimation(TextBlock.OpacityProperty,
                    AnimationHelper.GetDoubleAnimation(0, speed, delay));
            });
        }

        public void ShowOpacity(double speed = 400, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                this.Block.BeginAnimation(TextBlock.OpacityProperty,
                    AnimationHelper.GetDoubleAnimation(1, speed, delay));
            });
        }

        public void ShowFill(double speed = 400, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                if (IsOptimized)
                    UnOptimize();

                var anim = AnimationHelper.GetDoubleAnimation(1, speed, delay);

                StartColor.BeginAnimation(GradientStop.OffsetProperty, anim);
                EndColor.BeginAnimation(GradientStop.OffsetProperty, anim);
            });
        }

        public void HideFill(double speed = 400, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                if (IsOptimized)
                    UnOptimize();

                var anim = AnimationHelper.GetDoubleAnimation(-1, speed, delay);

                StartColor.BeginAnimation(GradientStop.OffsetProperty, anim);
                EndColor.BeginAnimation(GradientStop.OffsetProperty, anim);
            });
        }
    }


    public class VisualText : VisualBase
    {
        private Panel canvas;

        public string Text { get; set; }
        public Path Path { get; set; }

        public LinearGradientBrush Fill { get; set; }
        public SolidColorBrush Stroke { get; set; }

        public GradientStop StartColor { get; set; }
        public GradientStop EndColor { get; set; }
        public Color PrimaryColor { get; set; }

        public DoubleCollection StrokeArray { get; set; }
        public double StrokeDashOffset { get; set; }

        public double FontSize { get; set; }
        public string FontName { get; set; }

        public bool IsOptimized { get; set; }

        private AnimationState<PathGeometry> textAnimationState;
        private AnimationState<(Range source, PathGeometry target)[]> multiAnimationState;

        private PowerEase powerEase = new PowerEase();
        private DoubleAnimation expandAnimation = null;
        private VisualText expandState;

        private VisualText() : base(null) { }

        public VisualText(double x, double y, Panel canvas, Dispatcher dispatcher) : base(dispatcher)
        {
            X = x;
            Y = y;
            this.canvas = canvas;
        }

        private PathGeometry CreateTextGeometry(
            double x,
            double y,
            string text,
            string font = "Nexa Bold",
            double fontSize = 88)
        {
            var culture = CultureInfo.InvariantCulture;
            var flow = FlowDirection.LeftToRight;
            var fontFamily = new FontFamily(font);
            var typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            var formattedText = new FormattedText(text, culture, flow, typeface, fontSize, Brushes.White, 100);

            var geometry = formattedText.BuildGeometry(new System.Windows.Point(x, y));
            var pathGeometry = geometry.GetFlattenedPathGeometry();
            var unfrozen = pathGeometry.Clone();

            return unfrozen;
        }

        public void CreateWithoutCanvas(string text, Color color,
            string font = "Nexa Bold",
            double fontSize = 88)
        {
            Text = text;
            FontSize = fontSize;
            FontName = font;

            var pathGeometry = CreateTextGeometry(X, Y, text, font, fontSize);

            var stopStart = new GradientStop() { Color = color, Offset = -1 };
            var stopEnd = new GradientStop() { Color = Colors.Transparent, Offset = -1 };

            LinearGradientBrush linearGradientBrush = new LinearGradientBrush();
            linearGradientBrush.StartPoint = new Point(0, 0);
            linearGradientBrush.EndPoint = new Point(1, 0);
            linearGradientBrush.GradientStops = new GradientStopCollection();
            linearGradientBrush.GradientStops.Add(stopStart);
            linearGradientBrush.GradientStops.Add(stopEnd);

            Stroke = new SolidColorBrush(color); 

            Path = new Path();
            Path.Data = pathGeometry;
            Path.Fill = linearGradientBrush;
            Path.StrokeDashArray = new DoubleCollection() { 2000, 2000 };
            Path.StrokeDashOffset = -2000;
            Path.Stroke = Stroke;
            Path.Stroke.Freeze();

            var group = new TransformGroup();

            TranslateTransform = new TranslateTransform();
            ScaleTransform = new ScaleTransform();
            RotateTransform = new RotateTransform();

            group.Children.Add(TranslateTransform);
            group.Children.Add(ScaleTransform);
            group.Children.Add(RotateTransform);

            Path.RenderTransform = group;

            StartColor = stopStart;
            EndColor = stopEnd;
            Fill = linearGradientBrush;

            PrimaryColor = color;
        }

        public void Remove()
        {
            dispatcher.Invoke(() =>
            {
                canvas.Children.Remove(Path);
            });
        }

        public void Create(string text, Color color,
            string font = "Nexa Bold",
            double fontSize = 88)
        {
            dispatcher.Invoke(() =>
            {
                CreateWithoutCanvas(text, color, font, fontSize);
                canvas.Children.Add(Path);
            });
        }

        public void Show(double speed = 400, double delay = 0)
        {
            ShowStroke(speed, delay);
            ShowFill(speed * 2, delay + (speed / 4));
        }

        public void Freeze()
        {
            dispatcher.Invoke(() =>
            {
                Path.Data.Freeze();
            });
        }

        public void Optimize()
        {
            dispatcher.Invoke(() =>
            {
                Path.Fill = new SolidColorBrush(PrimaryColor);
                Path.Fill.Freeze();
                Path.Stroke.Freeze();

                IsOptimized = true;
            });
        }

        public void UnOptimize()
        {
            dispatcher.Invoke(() =>
            {
                Path.Fill = Fill;

                IsOptimized = false;
            });
        }

        public void Hide(double speed = 400, double delay = 0)
        {
            HideStroke(speed, delay);
            HideFill(speed * 2, delay + (speed / 4));
        }

        public void HideOpacity(double speed = 400, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                this.Path.BeginAnimation(Path.OpacityProperty, 
                    AnimationHelper.GetDoubleAnimation(0, speed, delay));
            });
        }

        public void ShowOpacity(double speed = 400, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                this.Path.BeginAnimation(Path.OpacityProperty,
                    AnimationHelper.GetDoubleAnimation(1, speed, delay));
            });
        }

        public void ShowFill(double speed = 400, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                if (IsOptimized)
                    UnOptimize();

                var anim = AnimationHelper.GetDoubleAnimation(1, speed, delay);

                StartColor.BeginAnimation(GradientStop.OffsetProperty, anim);
                EndColor.BeginAnimation(GradientStop.OffsetProperty, anim);
            });
        }

        public void HideFill(double speed = 400, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                if (IsOptimized)
                    UnOptimize();

                var anim = AnimationHelper.GetDoubleAnimation(-1, speed, delay);

                StartColor.BeginAnimation(GradientStop.OffsetProperty, anim);
                EndColor.BeginAnimation(GradientStop.OffsetProperty,  anim);
            });
        }

        public void ShowStroke(double speed = 400, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                Path.BeginAnimation(Path.StrokeDashOffsetProperty, 
                    AnimationHelper.GetDoubleAnimation(0, speed, delay));
            });
        }

        public void HideStroke(double speed = 400, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                Path.BeginAnimation(Path.StrokeDashOffsetProperty, 
                    AnimationHelper.GetDoubleAnimation(-2000, speed, delay));
            });
        }

        public void ExpandStep(string[] by, 
            double offset = -1, double fontSize = -1, string fontName = null,
            double speed = 0.025, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                (Range source, PathGeometry target)[]
                    pathGeometries = new (Range source, PathGeometry target)[by.Length];

                int index = 0;

                if (offset == -1)
                    offset = FontSize / 2;

                double position = Path.Data.Bounds.Width + offset;
                var pathGeometry = ((PathGeometry)this.Path.Data);

                string font = FontName;
                double size = FontSize;

                if (fontName != null)
                    font = fontName;
                if (fontSize != -1)
                    size = fontSize;

                foreach (var item in by)
                {
                    var to = new VisualText();
                    var g = to.CreateTextGeometry(X + position, Y, item, font, size);
                    Range range = new Range(0, 0);
                    pathGeometries[index++] = (range, g);

                    position += g.Bounds.Width + offset;
                }

                multiAnimationState = new AnimationState<(Range source, PathGeometry target)[]>();
                multiAnimationState.ProgressIncrement = speed;
                multiAnimationState.Object = pathGeometries;
                multiAnimationState.Delay = delay;

                CompositionTarget.Rendering += RenderStepExpand;
            });
        }


        public void ExpandFrom(string[] by, double offset = -1, double speed = 0.025, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                (Range source, PathGeometry target)[]
                    pathGeometries = new (Range source, PathGeometry target)[by.Length];

                int index = 0;

                if (offset == -1)
                    offset = FontSize / 2;

                double position = Path.Data.Bounds.Width + offset;
                var pathGeometry = ((PathGeometry)this.Path.Data);

                foreach (var item in by)
                {
                    var to = new VisualText();
                    var g = to.CreateTextGeometry(X + position, Y, item, FontName, FontSize);
                    var count = ((PathGeometry)Path.Data).Figures.Count;

                    for (int i = 0; i < g.Figures.Count; i++)
                    {
                        pathGeometry.Figures
                            .Add(pathGeometry.Figures[pathGeometry.Figures.Count - 1].Clone());
                    }

                    Range range = new Range(count, count + g.Figures.Count);
                    pathGeometries[index++] = (range, g);

                    position += g.Bounds.Width + offset;
                }

                multiAnimationState = new AnimationState<(Range source, PathGeometry target)[]>();
                multiAnimationState.ProgressIncrement = speed;
                multiAnimationState.Object = pathGeometries;
                multiAnimationState.Delay = delay;

                CompositionTarget.Rendering += RenderExpand;
            });
        }

        public void PreExpand(string by, double speed = 400, double delay = 0)
        {
            VisualText to = null;

            dispatcher.Invoke(() =>
            {
                to = new VisualText(X, Y, canvas, dispatcher);
                to.Create(by, Colors.White, FontName, FontSize);

                DoubleAnimation anim = new DoubleAnimation(1, TimeSpan.FromMilliseconds(speed))
                {
                    EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut }
                };
                anim.BeginTime = TimeSpan.FromMilliseconds(delay);

                expandAnimation = new DoubleAnimation(1, TimeSpan.FromMilliseconds(speed))
                {
                    EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut }
                };
                expandAnimation.BeginTime = TimeSpan.FromMilliseconds(delay);

                //expandAnimation.Completed += OnAnimationExpandDone;
                expandState = to;

                Move(X + to.Path.Data.Bounds.Width + FontSize / 2, Y);

                to.StartColor.BeginAnimation(GradientStop.OffsetProperty, expandAnimation);
                to.EndColor.BeginAnimation(GradientStop.OffsetProperty, anim);

            });
        }

        public void Expand(string by, double speed = 400, double delay = 0)
        {
            VisualText to = null;

            dispatcher.Invoke(() =>
            {
                by = " " + by;
                to = new VisualText(X + Path.Data.Bounds.Width, Y, canvas, dispatcher);
                to.Create(by, Colors.White, FontName, FontSize);
                to.Path.RenderTransform = this.Path.RenderTransform;

                DoubleAnimation anim = new DoubleAnimation(1, TimeSpan.FromMilliseconds(speed))
                {
                    EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut }
                };
                anim.BeginTime = TimeSpan.FromMilliseconds(delay);

                expandAnimation = new DoubleAnimation(1, TimeSpan.FromMilliseconds(speed))
                {
                    EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut }
                };
                expandAnimation.BeginTime = TimeSpan.FromMilliseconds(delay);

                expandAnimation.Completed += OnAnimationExpandDone;
                expandState = to;

                to.StartColor.BeginAnimation(GradientStop.OffsetProperty, expandAnimation);
                to.EndColor.BeginAnimation(GradientStop.OffsetProperty, anim);
            });
        }

        private void OnAnimationExpandDone(object target, EventArgs args)
        {
            var pathGeometry = Geometry.Combine(this.Path.Data, expandState.Path.Data, GeometryCombineMode.Union, null);
            this.Path.Data = pathGeometry;

            canvas.Children.Remove(expandState.Path);
            expandState = null;

            expandAnimation.Completed -= OnAnimationExpandDone;
        }

        public void MorphCollapse(double speed = 0.01, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                textAnimationState = new AnimationState<PathGeometry>();
                textAnimationState.ProgressIncrement = speed;
                textAnimationState.Delay = delay;

                CompositionTarget.Rendering += RenderMorphCollapse;
            });
        }

        public void Underline(double speed = 0.01, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                double offsetY = 10;
                var pathGeometry = ((PathGeometry)this.Path.Data);
                var pathGeometries = new (Range source, PathGeometry target)[1];

                PathGeometry geometry = new PathGeometry();
                PathFigure figure = new PathFigure();

                figure.StartPoint =
                    new Point(
                        this.X + pathGeometry.Bounds.Width / 2,
                        pathGeometry.Bounds.Bottom + offsetY
                        );

                var segment = new PolyLineSegment();
                segment.Points.Add(new Point(figure.StartPoint.X, figure.StartPoint.Y));
                figure.Segments.Add(segment);

                ((PathGeometry)this.Path.Data).Figures.Add(figure);

                Range range = new Range(pathGeometry.Figures.Count - 1, pathGeometry.Figures.Count);
                PathGeometry rectGeometry = new PathGeometry();

                PathFigure rectFigure = new PathFigure();
                var rectSegment = new PolyLineSegment();

                rectSegment.Points = new PointCollection();
                rectSegment.Points.Add(new Point(X, pathGeometry.Bounds.Bottom + offsetY));
                rectSegment.Points.Add(new Point(X + pathGeometry.Bounds.Width, pathGeometry.Bounds.Bottom + offsetY));
                rectSegment.Points.Add(new Point(X + pathGeometry.Bounds.Width, pathGeometry.Bounds.Bottom + offsetY + 4));
                rectSegment.Points.Add(new Point(X, pathGeometry.Bounds.Bottom + offsetY + 4));

                rectFigure.Segments.Add(rectSegment);
                rectFigure.StartPoint = new Point(X, pathGeometry.Bounds.Bottom + offsetY);

                rectGeometry.Figures.Add(rectFigure);
                pathGeometries[0] = (range, rectGeometry);

                multiAnimationState = new AnimationState<(Range source, PathGeometry target)[]>();
                multiAnimationState.ProgressIncrement = speed;
                multiAnimationState.Object = pathGeometries;
                multiAnimationState.Delay = delay;


                CompositionTarget.Rendering += RenderExpand;
            });
        }

        public void MorphToApply(List<PathGeometry> cache)
        {
            RenderMorphCache render = null;
            dispatcher.Invoke(() =>
            {
                render = new RenderMorphCache(cache, this.Path);
                render.Run();
            });
        }

        public List<PathGeometry> MorphToCache(string other, double speed = 0.01, double delay = 0)
        {
            List<PathGeometry> result = null;

            dispatcher.Invoke(() =>
            {
                VisualText to = new VisualText();
                var geometry = to.CreateTextGeometry(X, Y, other, FontName, FontSize);

                result = Morph.ToCache((PathGeometry)this.Path.Data, geometry, speed);
            });

            return result;
        }

        public void MorphTo(string other, double speed = 0.01, double delay = 0)
        {
            MorphTo(other, FontName, FontSize, speed, delay);
        }

        public void MorphTo(string other, string fontName, double fontSize, double speed = 0.01, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                textAnimationState = new AnimationState<PathGeometry>();
                textAnimationState.ProgressIncrement = speed;

                VisualText to = new VisualText();
                var geometry = to.CreateTextGeometry(X, Y, other, fontName, fontSize);
                textAnimationState.Object = geometry;
                textAnimationState.Delay = delay;

                CompositionTarget.Rendering += RenderMorph;
            });
        }

        public void MorphTo(PathGeometry other, double speed = 0.01, double delay = 0)
        {
            dispatcher.Invoke(() =>
            {
                textAnimationState = new AnimationState<PathGeometry>();
                textAnimationState.ProgressIncrement = speed;
                textAnimationState.Object = other;
                textAnimationState.Delay = delay;

                CompositionTarget.Rendering += RenderMorph;
            });
        }

        private void RenderStepExpand(object target, EventArgs e)
        {
            RenderingEventArgs renderingEventArgs = (RenderingEventArgs)e;
            if (renderingEventArgs.RenderingTime == multiAnimationState.LastFrame)
                return;

            multiAnimationState.LastFrame = renderingEventArgs.RenderingTime;

            if (textAnimationState.Delay > 0)
            {
                textAnimationState.Delay--;
                return;
            }

            multiAnimationState.Progress += multiAnimationState.ProgressIncrement;
            var progressEase = powerEase.Ease(multiAnimationState.Progress);
            //
            // Take the first string.
            // Clone the source (last).
            // Morph.
            // Repeat.
            //
            var g = multiAnimationState.Object[multiAnimationState.State];

            var pathGeometry = ((PathGeometry)this.Path.Data);

            if (g.source.Start.Value == 0 && g.source.End.Value == 0)
            {
                int count = pathGeometry.Figures.Count;

                for (int i = 0; i < g.target.Figures.Count; i++)
                {
                    pathGeometry.Figures
                        .Add(pathGeometry.Figures[pathGeometry.Figures.Count - 1].Clone());
                }

                Range range = new Range(count, count + g.target.Figures.Count);
                g.source = range;
                multiAnimationState.Object[multiAnimationState.State] = g;

                return;
            }

            //
            // Take a range of figures (which is the clone of original figures and morph them)
            //
            Morph.To((PathGeometry)this.Path.Data, (PathGeometry)g.target, g.source,
                progressEase);

            if (multiAnimationState.Progress >= 1.0)
            {
                multiAnimationState.State++;
                multiAnimationState.Progress = multiAnimationState.ProgressIncrement;

                if (multiAnimationState.State >= multiAnimationState.Object.Length)
                    CompositionTarget.Rendering -= RenderStepExpand;
            }
        }

        private void RenderExpand(object target, EventArgs e)
        {
            RenderingEventArgs renderingEventArgs = (RenderingEventArgs)e;
            if (renderingEventArgs.RenderingTime == multiAnimationState.LastFrame)
                return;

            multiAnimationState.LastFrame = renderingEventArgs.RenderingTime;

            if (textAnimationState.Delay > 0)
            {
                textAnimationState.Delay--;
                return;
            }

            multiAnimationState.Progress += multiAnimationState.ProgressIncrement;
            var progressEase = powerEase.Ease(multiAnimationState.Progress);

            //
            // Take a range of figures (which is the clone of original figures and morph them)
            //
            var g = multiAnimationState.Object[multiAnimationState.State];

            Morph.To((PathGeometry)this.Path.Data, (PathGeometry)g.target, g.source,
                progressEase);

            if (multiAnimationState.Progress >= 1.0)
            {
                multiAnimationState.State++;
                multiAnimationState.Progress = multiAnimationState.ProgressIncrement;

                if (multiAnimationState.State >= multiAnimationState.Object.Length)
                    CompositionTarget.Rendering -= RenderExpand;
            }
        }

        private void RenderMorph(object target, EventArgs e)
        {
            RenderingEventArgs renderingEventArgs = (RenderingEventArgs)e;

            if (renderingEventArgs.RenderingTime == multiAnimationState.LastFrame)
                return;

            multiAnimationState.LastFrame = renderingEventArgs.RenderingTime;

            if (textAnimationState.Delay > 0)
            {
                textAnimationState.Delay--;
                return;
            }

            textAnimationState.Progress += textAnimationState.ProgressIncrement;
            var progressEase = powerEase.Ease(textAnimationState.Progress);

            Morph.To((PathGeometry)Path.Data, (PathGeometry)textAnimationState.Object,
                progressEase);

            if (textAnimationState.Progress >= 1.0)
            {
                Path.Data = textAnimationState.Object;

                List<int> toRemove = new List<int>();

                //
                // Hydrate path geometry and remove figures that overlap.
                //
                PathGeometry geometry = (PathGeometry)Path.Data;
                for (int i = 0; i < geometry.Figures.Count - 1; i++)
                {
                    var xDiff = Math.Abs(geometry.Figures[i].StartPoint.X - geometry.Figures[i + 1].StartPoint.X);
                    var yDiff = Math.Abs(geometry.Figures[i].StartPoint.Y - geometry.Figures[i + 1].StartPoint.Y);

                    if (xDiff < 0.1 && yDiff < 0.1)
                    {
                        geometry.Figures.RemoveAt(i);
                    }
                }

                CompositionTarget.Rendering -= RenderMorph;
            }
        }

        private void RenderMorphCollapse(object target, EventArgs e)
        {
            RenderingEventArgs renderingEventArgs = (RenderingEventArgs)e;
            if (renderingEventArgs.RenderingTime == multiAnimationState.LastFrame)
                return;

            multiAnimationState.LastFrame = renderingEventArgs.RenderingTime;

            if (textAnimationState.Delay > 0)
            {
                textAnimationState.Delay--;
                return;
            }

            textAnimationState.Progress += textAnimationState.ProgressIncrement;

            var progressEase = powerEase.Ease(textAnimationState.Progress);

            if (Morph.Collapse((PathGeometry)Path.Data, progressEase))
            {
                CompositionTarget.Rendering -= RenderMorphCollapse;
            }

            if (textAnimationState.Progress >= 1.0)
            {
                CompositionTarget.Rendering -= RenderMorphCollapse;
            }
        }
    }
}
