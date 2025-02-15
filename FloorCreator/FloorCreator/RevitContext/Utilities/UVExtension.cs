using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectionCreator.RevitContext.Utilities
{
    internal static class UVExtension
    {
        public static XYZ ToXYZ(this UV point)
        {
            return XYZUtils.CreatePointFromCoordinate(point);
        }
    }
}
