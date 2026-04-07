using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DivideCentered
{
    public class DivideCenteredComponent : GH_Component
    {
        public DivideCenteredComponent()
            : base(
                "Divide Centered",
                "DivCen",
                "Divide a curve with equal spacing from center, leaving equal remainders at both ends",
                "Reticle",
                "Curve")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve to divide", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "D", "Segment length (spacing between division points)", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Ends", "E", "True = replace end division points with curve endpoints, False = all division points + curve endpoints", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Division points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Remainder", "R", "Equal remainder length at each end", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve crv = null;
            double spacing = 0;
            bool ends = true;

            if (!DA.GetData(0, ref crv)) return;
            if (!DA.GetData(1, ref spacing)) return;
            DA.GetData(2, ref ends);

            if (crv == null || spacing <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid curve or spacing");
                return;
            }

            double L = crv.GetLength();

            if (spacing >= L)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Spacing exceeds curve length");
                DA.SetDataList(0, new List<Point3d>());
                DA.SetData(1, L / 2.0);
                return;
            }

            int n = (int)Math.Floor(L / spacing);
            double remainder = L - (n * spacing);
            double startOffset = remainder / 2.0;

            // Compute division points at arc length distances
            var divPoints = new List<Point3d>();
            for (int i = 0; i <= n; i++)
            {
                double d = startOffset + i * spacing;
                double t;
                if (crv.LengthParameter(d, out t))
                {
                    divPoints.Add(crv.PointAt(t));
                }
            }

            // Build output point list based on ends toggle
            var points = new List<Point3d>();

            if (ends)
            {
                // Replace first/last division points with curve endpoints
                points.Add(crv.PointAtStart);
                for (int i = 1; i < divPoints.Count - 1; i++)
                    points.Add(divPoints[i]);
                points.Add(crv.PointAtEnd);

                DA.SetData(1, startOffset + spacing);
            }
            else
            {
                // All division points + curve endpoints
                points.Add(crv.PointAtStart);
                points.AddRange(divPoints);
                points.Add(crv.PointAtEnd);

                DA.SetData(1, startOffset);
            }

            DA.SetDataList(0, points);
        }

        protected override Bitmap Icon
        {
            get
            {
                var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("DivideCentered.Resources.icon_24.png");
                if (stream != null)
                    return new Bitmap(stream);
                return null;
            }
        }

        public override Guid ComponentGuid =>
            new Guid("a7c3e1f0-5d28-4b91-9e6a-3f8c2d104e57");
    }
}
