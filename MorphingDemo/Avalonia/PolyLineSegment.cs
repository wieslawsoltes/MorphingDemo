using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;

namespace MorphingDemo.Avalonia
{
    public sealed class PolyLineSegment : PathSegment
    {
        public static readonly StyledProperty<AvaloniaList<Point>?> PointsProperty
            = AvaloniaProperty.Register<ArcSegment, AvaloniaList<Point>?>(nameof(Points));

        public AvaloniaList<Point>? Points
        {
            get => GetValue(PointsProperty);
            set => SetValue(PointsProperty, value);
        }

        protected override void ApplyTo(StreamGeometryContext ctx)
        {
            if (Points?.Count > 0)
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    ctx.LineTo(Points[i]);
                }
            }
        }

        public override string ToString()
            => Points?.Count >= 1 ? "L " + string.Join(' ', Points) : "";
    }
}