using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Animators;
using Avalonia.Data;
using Avalonia.Media;

namespace PolyLineAnimation
{
    public class GeometryAnimator : Animator<Geometry>
    {
        public override Geometry Interpolate(double progress, Geometry oldValue, Geometry newValue)
        {
            var clone = (oldValue as PathGeometry).ClonePathGeometry();

            //Morph.To(clone, newValue as PathGeometry, progress);
            WPFAnimations.Morph.To(clone, newValue as PathGeometry, progress);

            return clone;
        }
    }
}