#if false
using Avalonia.Media;
using Avalonia.Threading;

namespace WPFAnimations.Visuals
{
    public class VisualBase
    {
        protected Dispatcher dispatcher;

        public double X { get; set; }
        public double Y { get; set; }
        public double Scale { get; set; }
        public double Rotate { get; set; }

        public TranslateTransform TranslateTransform { get; set; }
        public RotateTransform RotateTransform { get; set; }
        public ScaleTransform ScaleTransform { get; set; }

        public VisualBase(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }


        public void Move(double x, double y, double scale = 1, double rotate = 0, double speed = 400, double delay = 0)
        {
            double scaleTranslate = 1 / scale;
            double top = Y;
            double left = X;

            dispatcher.InvokeAsync(() =>
            {
                var scaleAnim = AnimationHelper.GetDoubleAnimation(scale, speed, delay);
                var animY = AnimationHelper.GetDoubleAnimation(y - (top * scaleTranslate), speed, delay);
                var animX = AnimationHelper.GetDoubleAnimation(x - (left * scaleTranslate), speed, delay);
                var rotateAnim = AnimationHelper.GetDoubleAnimation(rotate, speed, delay);

                RotateTransform.Angle = rotate;

                TranslateTransform.BeginAnimation(TranslateTransform.YProperty, animY);
                TranslateTransform.BeginAnimation(TranslateTransform.XProperty, animX);

                ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

                RotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnim);
            });

            this.Scale = scale;
        }
    }
}
#endif
