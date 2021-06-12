using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Media;

namespace Avalonia
{
    /// <summary>
    /// Represents a set of line segments defined by a points collection with each Point specifying the end point of a line segment.
    /// </summary>
    public sealed class PolyLineSegment : PathSegment
    {
        /// <summary>
        /// Defines the <see cref="Points"/> property.
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<Point>?> PointsProperty
            = AvaloniaProperty.Register<PolyLineSegment, AvaloniaList<Point>?>(nameof(Points));

        /// <summary>
        /// Gets or sets the points.
        /// </summary>
        /// <value>
        /// The points.
        /// </value>
        public AvaloniaList<Point>? Points
        {
            get => GetValue(PointsProperty);
            set => SetValue(PointsProperty, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolyLineSegment"/> class.
        /// </summary>
        public PolyLineSegment()
        {
            Points = new AvaloniaList<Point>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolyLineSegment"/> class.
        /// </summary>
        /// <param name="points">The points.</param>
        public PolyLineSegment(IEnumerable<Point> points)
        {
            Points = new AvaloniaList<Point>(points);
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