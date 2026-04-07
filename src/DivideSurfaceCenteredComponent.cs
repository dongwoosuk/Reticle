using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace DivideCentered
{
    public class DivideSurfaceCenteredComponent : GH_Component
    {
        public DivideSurfaceCenteredComponent()
            : base(
                "Divide Surface Centered",
                "DivSrfCen",
                "Divide a surface UV from center with equal spacing, leaving equal remainders at all edges",
                "Reticle",
                "Surface")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Surface to divide", GH_ParamAccess.item);
            pManager.AddNumberParameter("U Distance", "U", "Segment length in U direction", GH_ParamAccess.item);
            pManager.AddNumberParameter("V Distance", "V", "Segment length in V direction", GH_ParamAccess.item);
            pManager.AddBooleanParameter("U Ends", "Eu", "U direction: True = replace end divisions with surface edges, False = all division points + edges", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("V Ends", "Ev", "V direction: True = replace end divisions with surface edges, False = all division points + edges", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Division points grid (tree: {row}[col])", GH_ParamAccess.tree);
            pManager.AddSurfaceParameter("Surfaces", "S", "Sub-surfaces (tree: {row}[col])", GH_ParamAccess.tree);
            pManager.AddNumberParameter("U Remainder", "Ru", "Equal remainder length at U edges", GH_ParamAccess.item);
            pManager.AddNumberParameter("V Remainder", "Rv", "Equal remainder length at V edges", GH_ParamAccess.item);
        }

        private List<double> CenteredParams(Curve curve, double spacing, bool ends, out double remainder)
        {
            double L = curve.GetLength();
            var paramList = new List<double>();
            remainder = L / 2.0;

            if (spacing >= L)
                return paramList;

            int n = (int)Math.Floor(L / spacing);
            double rem = L - (n * spacing);
            double startOffset = rem / 2.0;

            // Division point parameters
            var divParams = new List<double>();
            for (int i = 0; i <= n; i++)
            {
                double d = startOffset + i * spacing;
                double t;
                if (curve.LengthParameter(d, out t))
                    divParams.Add(t);
            }

            if (ends)
            {
                // Replace first/last with curve endpoints
                paramList.Add(curve.Domain.Min);
                for (int i = 1; i < divParams.Count - 1; i++)
                    paramList.Add(divParams[i]);
                paramList.Add(curve.Domain.Max);
                remainder = startOffset + spacing;
            }
            else
            {
                // All division points + endpoints
                paramList.Add(curve.Domain.Min);
                paramList.AddRange(divParams);
                paramList.Add(curve.Domain.Max);
                remainder = startOffset;
            }

            return paramList;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Surface srf = null;
            double uSpacing = 0, vSpacing = 0;
            bool uEnds = true, vEnds = true;

            if (!DA.GetData(0, ref srf)) return;
            if (!DA.GetData(1, ref uSpacing)) return;
            if (!DA.GetData(2, ref vSpacing)) return;
            DA.GetData(3, ref uEnds);
            DA.GetData(4, ref vEnds);

            if (srf == null || uSpacing <= 0 || vSpacing <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid surface or spacing");
                return;
            }

            // Get mid-isocurves to measure actual lengths
            Interval uDomain = srf.Domain(0);
            Interval vDomain = srf.Domain(1);
            double uMid = (uDomain.Min + uDomain.Max) / 2.0;
            double vMid = (vDomain.Min + vDomain.Max) / 2.0;

            Curve isoU = srf.IsoCurve(0, vMid); // U-direction at mid-V
            Curve isoV = srf.IsoCurve(1, uMid); // V-direction at mid-U

            double uRemainder, vRemainder;
            var uParams = CenteredParams(isoU, uSpacing, uEnds, out uRemainder);
            var vParams = CenteredParams(isoV, vSpacing, vEnds, out vRemainder);

            if (uParams.Count == 0 || vParams.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Spacing exceeds surface dimension");
                return;
            }

            // Points tree {row}[col]
            var ptTree = new DataTree<GH_Point>();
            for (int j = 0; j < vParams.Count; j++)
            {
                var path = new GH_Path(j);
                for (int i = 0; i < uParams.Count; i++)
                {
                    Point3d pt = srf.PointAt(uParams[i], vParams[j]);
                    ptTree.Add(new GH_Point(pt), path);
                }
            }

            // Sub-surfaces tree {row}[col]
            var srfTree = new DataTree<GH_Surface>();
            for (int j = 0; j < vParams.Count - 1; j++)
            {
                var path = new GH_Path(j);
                for (int i = 0; i < uParams.Count - 1; i++)
                {
                    var uInterval = new Interval(uParams[i], uParams[i + 1]);
                    var vInterval = new Interval(vParams[j], vParams[j + 1]);
                    Surface trimmed = srf.Trim(uInterval, vInterval);
                    if (trimmed != null)
                        srfTree.Add(new GH_Surface(trimmed), path);
                }
            }

            DA.SetDataTree(0, ptTree);
            DA.SetDataTree(1, srfTree);
            DA.SetData(2, uRemainder);
            DA.SetData(3, vRemainder);
        }

        protected override Bitmap Icon
        {
            get
            {
                var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("DivideCentered.Resources.icon_surface_24.png");
                if (stream != null)
                    return new Bitmap(stream);
                return null;
            }
        }

        public override Guid ComponentGuid =>
            new Guid("c8d5e2b1-7f4a-4c93-be7c-2a9f4d316e89");
    }
}
