using System;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;

namespace WPFAnimations
{
    internal static class GeometryCloneExtensions
    {
        public static PathGeometry ClonePathGeometry(this PathGeometry pathIn)
        {
            var pathOut = new PathGeometry()
            {
                FillRule = pathIn.FillRule
            };

            foreach (var figureIn in pathIn.Figures)
            {
                var figureOut = figureIn.ClonePathFigure();
                pathOut.Figures.Add(figureOut);
            }

            return pathOut;
        }

        public static PathFigure ClonePathFigure(this PathFigure figureIn)
        {
           var figureOut = new PathFigure()
            {
                IsClosed = figureIn.IsClosed,
                IsFilled = figureIn.IsClosed,
                StartPoint = figureIn.StartPoint
            };

            if (figureIn.Segments is null)
            {
                return figureOut;
            }

            foreach (var pathSegmentIn in figureIn.Segments)
            {
                switch (pathSegmentIn)
                {
                    case ArcSegment arcSegmentIn:
                    {
                        var arcSegmentOut = new ArcSegment()
                        {
                            IsLargeArc = arcSegmentIn.IsLargeArc,
                            Point = arcSegmentIn.Point,
                            RotationAngle = arcSegmentIn.RotationAngle,
                            Size = arcSegmentIn.Size,
                            SweepDirection = arcSegmentIn.SweepDirection
                        };
                        figureOut.Segments?.Add(arcSegmentOut);
                    }
                        break;
                    case BezierSegment bezierSegmentIn:
                    {
                        var bezierSegmentOut = new BezierSegment()
                        {
                            Point1 = bezierSegmentIn.Point1,
                            Point2 = bezierSegmentIn.Point2,
                            Point3 = bezierSegmentIn.Point3
                        };
                        figureOut.Segments?.Add(bezierSegmentOut);
                    }
                        break;
                    case LineSegment lineSegmentIn:
                    {
                        var lineSegmentOut = new BezierSegment()
                        {
                            Point1 = lineSegmentIn.Point
                        };
                        figureOut.Segments?.Add(lineSegmentOut);
                    }
                        break;
                    case QuadraticBezierSegment quadraticBezierSegmentIn:
                    {
                        var quadraticBezierSegmentOut = new QuadraticBezierSegment()
                        {
                            Point1 = quadraticBezierSegmentIn.Point1,
                            Point2 = quadraticBezierSegmentIn.Point2,
                        };
                        figureOut.Segments?.Add(quadraticBezierSegmentOut);
                    }
                        break;
                    case PolyLineSegment polyLineSegmentIn:
                    {
                        var polyLineSegmentOut = new PolyLineSegment()
                        {
                            Points = new AvaloniaList<Point>()
                        };
                        foreach (var pt in polyLineSegmentIn.Points)
                        {
                            polyLineSegmentOut.Points.Add(pt);
                        }
                        figureOut.Segments?.Add(polyLineSegmentOut);
                    }
                        break;
                    default:
                    {
                        // TODO:
                    }
                        break;
                }
            }

            return figureOut;
        }
    }
}