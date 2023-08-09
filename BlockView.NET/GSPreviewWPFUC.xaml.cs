using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsSystem;

using System.Windows.Forms.Integration;

namespace BlockView.NET
{
    /// <summary>
    /// Interaction logic for GSPreviewWPFUC.xaml
    /// </summary>
    public partial class GSPreviewWPFUC : UserControl
    {
        private GsPreviewCtrl mPreviewCtrl;
        private List<Database> dwgs = new List<Database>();

        public GSPreviewWPFUC()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            mPreviewCtrl = FindName("GsPreviewCtrl") as GsPreviewCtrl;
        }

        public void InitDrawingControl(Document doc, Database db)
        {
            // initialize the control
            mPreviewCtrl.Init(doc, db);
            // now find out what the current view is and set the GsPreviewCtrl to the same
            SetViewTo(mPreviewCtrl.mpView, db);
            // now add the current space to the GsView
            using (BlockTableRecord curSpace = db.CurrentSpaceId.Open(OpenMode.ForRead, true, true) as BlockTableRecord)
                mPreviewCtrl.mpView.Add(curSpace, mPreviewCtrl.mpModel);
        }

        // sets a GsView to the active viewport data held by the database
        public void SetViewTo(Autodesk.AutoCAD.GraphicsSystem.View view, Database db)
        {
            // just check we have valid extents
            if (db.Extmax.X < db.Extmin.X || db.Extmax.Y < db.Extmin.Y || db.Extmax.Z < db.Extmax.Z)
            {
                db.Extmin = new Point3d(0, 0, 0);
                db.Extmax = new Point3d(400, 400, 400);
            }
            // get the dwg extents
            Point3d extMax = db.Extmax;
            Point3d extMin = db.Extmin;
            // now the active viewport info
            double height = 0.0, width = 0.0, viewTwist = 0.0;
            Point3d targetView = new Point3d();
            Vector3d viewDir = new Vector3d();
            GSUtil.GetActiveViewPortInfo(ref height, ref width, ref targetView, ref viewDir, ref viewTwist, true);
            // from the data returned let's work out the viewmatrix
            viewDir = viewDir.GetNormal();
            Vector3d viewXDir = viewDir.GetPerpendicularVector().GetNormal();
            viewXDir = viewXDir.RotateBy(viewTwist, -viewDir);
            Vector3d viewYDir = viewDir.CrossProduct(viewXDir);
            Point3d boxCenter = extMin + 0.5 * (extMax - extMin);
            Matrix3d viewMat;
            viewMat = Matrix3d.AlignCoordinateSystem(boxCenter, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis,
              boxCenter, viewXDir, viewYDir, viewDir).Inverse();
            Extents3d wcsExtents = new Extents3d(extMin, extMax);
            Extents3d viewExtents = wcsExtents;
            viewExtents.TransformBy(viewMat);
            double xMax = System.Math.Abs(viewExtents.MaxPoint.X - viewExtents.MinPoint.X);
            double yMax = System.Math.Abs(viewExtents.MaxPoint.Y - viewExtents.MinPoint.Y);
            Point3d eye = boxCenter + viewDir;
            // finally set the Gs view to the dwg view
            view.SetView(eye, boxCenter, viewYDir, xMax, yMax);

            // now update
            refreshView();
        }

        public void refreshView()
        {
            mPreviewCtrl.mpView.Invalidate();
            mPreviewCtrl.mpView.Update();
        }

        public void Cleanup()
        {
            // clean up
            if (mPreviewCtrl != null)
            {
                mPreviewCtrl.ClearAll();
                mPreviewCtrl.Dispose();
                mPreviewCtrl = null;
            }

            if (dwgs != null)
            {
                dwgs.DisposeItems();
                dwgs = null;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // pick the place where o output the image type
            Autodesk.AutoCAD.Windows.OpenFileDialog dialog = new Autodesk.AutoCAD.Windows.OpenFileDialog("Open DWG File", null, "dwg", "OpenDWGFileDialog", Autodesk.AutoCAD.Windows.OpenFileDialog.OpenFileDialogFlags.NoUrls);
            // if all is ok?
            if (System.Windows.Forms.DialogResult.OK == dialog.ShowDialog())
            {
                if (mPreviewCtrl.mpView != null)
                {
                    // clear the preview control
                    mPreviewCtrl.mpView.EraseAll();
                }
                // create a new database
                dwgs.Add(new Database(false, true));
                // now read it in
                dwgs[dwgs.Count - 1].ReadDwgFile(dialog.Filename, FileOpenMode.OpenForReadAndReadShare, true, "");
                // initialising the drawing control, pass the existing document still as the gs view still refers to it
                InitDrawingControl(AcadApp.DocumentManager.MdiActiveDocument, dwgs[dwgs.Count - 1]);
            }
        }
    }
}
