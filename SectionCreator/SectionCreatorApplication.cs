using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AXE.RevitContext.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AXE
{
 
    public class SectionCreatorApplication : IExternalApplication
    {
       
        
        private static SectionViewUpdater _updater;

        public Result OnStartup(UIControlledApplication application)
       {
            

            _updater = new SectionViewUpdater();
            UpdaterRegistry.RegisterUpdater(_updater, true);

            ElementClassFilter viewSectionFilter = new ElementClassFilter(typeof(ViewSection));
            UpdaterRegistry.AddTrigger(_updater.GetUpdaterId(), viewSectionFilter, Element.GetChangeTypeElementAddition());

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            UpdaterRegistry.UnregisterUpdater(_updater.GetUpdaterId());
            return Result.Succeeded;
        }
    }
}
