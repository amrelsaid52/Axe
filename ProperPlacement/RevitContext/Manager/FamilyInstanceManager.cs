using Autodesk.Revit.DB;

namespace AXE.RevitContext.Manager
{
    internal class FamilyInstanceManager
    {
        public static void AdjustFamilyInstanceFlipping(FamilyInstance familyInstance, PointLocation pointLocation)
        {
            if (pointLocation == null) return;


            //if point is right ==> facing's x is negative
            if (pointLocation.HzDirection.Equals(HorizontalDirection.Right))
            {
                if (familyInstance.CanFlipFacing && familyInstance.FacingOrientation.X > 0)
                {
                    familyInstance.flipFacing();
                }
            }
            //if point is left ==> facing's x is positive
            else if (pointLocation.HzDirection.Equals(HorizontalDirection.Left))
            {
                if (familyInstance.CanFlipFacing && familyInstance.FacingOrientation.X < 0)
                {
                    familyInstance.flipFacing();
                }
            }

            //if point is top ==> facing's x=y is negative
            if (pointLocation.VrDirection.Equals(VerticalDirection.Top))
            {
                if (familyInstance.CanFlipHand && familyInstance.HandOrientation.Y > 0)
                {
                    familyInstance.flipHand();
                }
            }
            //if point is bottom ==> facing's y is positive
            else if (pointLocation.VrDirection.Equals(VerticalDirection.Bottom))
            {
                if (familyInstance.CanFlipHand && familyInstance.HandOrientation.Y < 0)
                {
                    familyInstance.flipHand();
                }
            }

        }

        public static void AdjustFamilyInstanceLocation(FamilyInstance familyInstance, PointLocation pointLocation,double distance)
        {
            if(familyInstance == null) return;
            if (pointLocation == null) return;

            if (pointLocation.VrDirection.Equals(VerticalDirection.Top))
            {
                XYZ translation = new XYZ(0,-distance,0);
                ElementTransformUtils.MoveElement(familyInstance.Document, familyInstance.Id, translation);
            }else if (pointLocation.VrDirection.Equals(VerticalDirection.Bottom))
            {
                XYZ translation = new XYZ(0, distance, 0);
                ElementTransformUtils.MoveElement(familyInstance.Document, familyInstance.Id, translation);
            }
        }
    }
}
