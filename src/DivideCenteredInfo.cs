using System;
using System.Drawing;
using System.Reflection;

using Grasshopper.Kernel;

namespace DivideCentered
{
    public class DivideCenteredInfo : GH_AssemblyInfo
    {
        public override string Name => "Reticle";
        public override string Description => "Centered division tools for curves and surfaces";
        public override Guid Id => new Guid("b4d7f2a1-6e39-4c82-af5b-1a9d3e205f68");
        public override string AuthorName => "Steinberg Hart - ADG";
        public override string AuthorContact => "";
        public override string Version => "1.0.0";

        public override Bitmap Icon
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
    }
}
