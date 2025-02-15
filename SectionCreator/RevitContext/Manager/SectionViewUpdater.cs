using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace SectionCreator.RevitContext.Manager
{
    public class SectionViewUpdater : IUpdater
    {
        


        private static AddInId appId = new AddInId(new Guid("3E3B941C-EBAB-4897-8C93-CD64EC17086A"));
        private static UpdaterId updaterId = new UpdaterId(appId, new Guid("2C611DE7-2C61-4B2A-8675-6C8DF55EDF05"));

        public SectionViewUpdater()
        {
            

        }

        public void Execute(UpdaterData updateData)
        {
            Document doc = updateData.GetDocument();

            try
            {
                foreach (ElementId eid in updateData.GetAddedElementIds())
                {
                    ViewSection viewSection = doc.GetElement(eid) as ViewSection;
                    if (viewSection != null)
                    {
                        double offset = 12;
                        Level level = doc.GetElement((doc.ActiveView as ViewPlan).GenLevel.Id) as Level;
                        BoundingBoxXYZ sectionBox = new BoundingBoxXYZ
                        {
                            Transform = viewSection.CropBox.Transform,
                            Min = new XYZ(viewSection.CropBox.Min.X, level.Elevation - offset, viewSection.CropBox.Min.Z ),
                            Max = new XYZ(viewSection.CropBox.Max.X, level.Elevation + offset, viewSection.CropBox.Max.Z ),
                        };
                        viewSection.CropBox = sectionBox;

                    }



                }

            }
            catch (Exception ex) { }
        }
          

        public string GetAdditionalInformation()
        {
            return "Change Section View Hieght";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Views;
        }

        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }

        public string GetUpdaterName()
        {
            return "Section View Updater";
        }
    }
}
