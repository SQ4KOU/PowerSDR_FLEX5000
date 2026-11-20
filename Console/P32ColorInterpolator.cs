using System.Drawing;

namespace PowerSDR
{
    public class ColorInterpolator
    {
        delegate byte ComponentSelector(Color color);
        static ComponentSelector _alphaSelector = color => color.A;
        static ComponentSelector _redSelector = color => color.R;
        static ComponentSelector _greenSelector = color => color.G;
        static ComponentSelector _blueSelector = color => color.B;

        public static Color InterpolateBetween(Color endPoint1, Color endPoint2, double lambda)
        {
            if (lambda < 0 || lambda > 1) return Color.Empty;
            return Color.FromArgb(
                InterpolateComponent(endPoint1, endPoint2, lambda, _alphaSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _redSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _greenSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _blueSelector));
        }

        static byte InterpolateComponent(Color endPoint1, Color endPoint2, double lambda, ComponentSelector selector)
        {
            return (byte)(selector(endPoint1) + (selector(endPoint2) - selector(endPoint1)) * lambda);
        }
    }
}