using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SectionCreator.RevitContext.Helpers
{
    internal class PointsHelper
    {
		static	readonly List<UV> points = new List<UV>
		{
			new UV(0, 0), new UV(79, 0),
			new UV(44, 25), new UV(13, 25),
			new UV(13, 40), new UV(-8, 40),
			new UV(55, 34), new UV(55, 10),
			new UV(79, 34), new UV(55, 34),
			new UV(0, 20), new UV(0, 0),
			new UV(55, 10), new UV(44, 12),
			new UV(-8, 40), new UV(-8, 20),
			new UV(79, 0), new UV(79, 34),
			new UV(44, 12), new UV(44, 25),
			new UV(-8, 20), new UV(0, 20),
			new UV(13, 25), new UV(13, 40)
		};

		public static List<UV> GetPoints()
		{
			return points;
		}
	}
}
