
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace AXE.RevitContext.Manager
{
    internal class SectionManager
    {
        public static ViewSection CreateSectionView(Document document)
        {

            // Find a section view type    
            IEnumerable<ViewFamilyType> viewFamilyTypes = from elem in new FilteredElementCollector(document).OfClass(typeof(ViewFamilyType))
                                                          let type = elem as ViewFamilyType
                                                          where type.ViewFamily == ViewFamily.Section
                                                          select type;

            // Create a BoundingBoxXYZ instance centered on wall    
            //LocationCurve lc = wall.Location as LocationCurve;
            //Transform curveTransform = lc.Curve.ComputeDerivatives(0.5, true);
            // using 0.5 and "true" (to specify that the parameter is normalized)     
            // places the transform's origin at the center of the location curve    

            XYZ origin = XYZ.Zero; // mid-point of location curve    
            XYZ viewDirection = XYZ.BasisX; // tangent vector along the location curve    
            XYZ normal = viewDirection.CrossProduct(XYZ.BasisZ).Normalize(); // location curve normal @ mid-point    

            Transform transform = Transform.Identity;
            transform.Origin = origin;
            transform.BasisX = normal;
            transform.BasisY = XYZ.BasisZ;

            // can use this simplification because wall's "up" is vertical.    
            // For a non-vertical situation (such as section through a sloped floor the surface normal would be needed)    
            transform.BasisZ = normal.CrossProduct(XYZ.BasisZ);

            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            sectionBox.Transform = transform;
            sectionBox.Min = new XYZ(-10, -12, 0);
            sectionBox.Max = new XYZ(10, 12, 5);
            // Min & Max X values (-10 & 10) define the section line length on each side of the wall    
            // Max Y (12) is the height of the section box// Max Z (5) is the far clip offset    

            // Create a new view section.    
            ViewSection viewSection = ViewSection.CreateSection(document, viewFamilyTypes.First().Id, sectionBox);

            return viewSection;

        }
    }
}
