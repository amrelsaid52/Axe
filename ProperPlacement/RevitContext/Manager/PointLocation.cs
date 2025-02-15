using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProperPlacement.RevitContext.Manager
{
    internal class PointLocation
    {
        public HorizontalDirection HzDirection { get; set; } = HorizontalDirection.Right;

        public VerticalDirection VrDirection { get; set; } = VerticalDirection.Bottom;

    }

    internal enum HorizontalDirection
    {
        Right,
        Left,
    }
    internal enum VerticalDirection {  Top, Bottom }
}
