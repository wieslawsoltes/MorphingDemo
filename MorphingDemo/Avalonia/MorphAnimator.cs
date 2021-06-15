using Avalonia.Animation.Animators;
using Avalonia.Media;
using WPFAnimations;

namespace Avalonia
{
    public class MorphAnimator : Animator<Geometry>
    {
        public override Geometry Interpolate(double progress, Geometry oldValue, Geometry newValue)
        {
            var clone = (oldValue as PathGeometry).ClonePathGeometry();

            Morph.To(clone, newValue as PathGeometry, progress);

            return clone;
        }
    }
}