using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AXE.RevitContext.Utilities
{
    public class XYZUtils
    {
        public static XYZ CreatePointFromCoordinate(double x, double y, double z = 0)
        {
            return new XYZ(x, y, z);
        }
        public static XYZ CreatePointFromCoordinate(UV uV)
        {
            return CreatePointFromCoordinate(uV.U,uV.V);
        }

         
    }
}
