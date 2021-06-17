using Avalonia;
using Avalonia.Animation.Animators;
using Avalonia.Media;

namespace PolyLineAnimation
{
    public class PolyLineAnimator : Animator<Geometry>
    {
        public override Geometry Interpolate(double progress, Geometry oldValue, Geometry newValue)
        {
            var clone = (oldValue as PathGeometry).ClonePathGeometry();

            PolyLineMorph.To(clone, newValue as PathGeometry, progress);

            return clone;
        }
    }
}