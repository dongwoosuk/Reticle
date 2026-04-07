"""
Divide Surface Centered - Divide surface UV from center with equal remainders at all edges

@input: srf, u_spacing, v_spacing, u_ends, v_ends
@output: Points, SubSrfs, U_Remainder, V_Remainder

Inputs:
- srf (Surface): Surface to divide
- u_spacing (float): Segment length in U direction
- v_spacing (float): Segment length in V direction
- u_ends (bool): U direction - True = replace end divisions with surface edges, False = all division points + surface edges
- v_ends (bool): V direction - True = replace end divisions with surface edges, False = all division points + surface edges

Outputs:
- Points (list): Division points grid (flattened)
- SubSrfs (list): Sub-surfaces (trimmed panels)
- U_Remainder (float): Equal remainder length at U edges
- V_Remainder (float): Equal remainder length at V edges
"""

import Rhino.Geometry as rg
import rhinoscriptsyntax as rs
import math
import Grasshopper as gh
from Grasshopper import DataTree
from Grasshopper.Kernel.Data import GH_Path

# Convert Guid to surface geometry if needed
if srf and not isinstance(srf, rg.Surface):
    brep = rs.coercebrep(srf)
    if brep:
        srf = brep.Faces[0].UnderlyingSurface()

# Default ends to True if not provided
try:
    u_ends = bool(u_ends)
except:
    u_ends = True
try:
    v_ends = bool(v_ends)
except:
    v_ends = True

def centered_params(curve, spacing, ends_flag):
    """Compute centered division parameters along a curve"""
    L = curve.GetLength()
    if spacing >= L:
        return [], L / 2.0

    n = int(math.floor(L / spacing))
    remainder = L - (n * spacing)
    start_offset = remainder / 2.0

    # Arc length distances for division points
    distances = [start_offset + i * spacing for i in range(n + 1)]

    # Convert arc length to curve parameter
    params = []
    for d in distances:
        ok, t = curve.LengthParameter(d)
        if ok:
            params.append(t)

    if ends_flag:
        # Replace first/last with curve endpoints
        if len(params) >= 2:
            params = [curve.Domain.Min] + params[1:-1] + [curve.Domain.Max]
        rem = start_offset + spacing
    else:
        # Add curve endpoints
        params = [curve.Domain.Min] + params + [curve.Domain.Max]
        rem = start_offset

    return params, rem

# Input validation
if srf and u_spacing and v_spacing and u_spacing > 0 and v_spacing > 0:
    # Get surface domain
    u_domain = srf.Domain(0)
    v_domain = srf.Domain(1)

    # Use mid-isocurves to measure actual lengths
    u_mid = (u_domain.Min + u_domain.Max) / 2.0
    v_mid = (v_domain.Min + v_domain.Max) / 2.0

    # Isocurve along U at mid-V (measures U-direction length)
    iso_u = srf.IsoCurve(0, v_mid)
    # Isocurve along V at mid-U (measures V-direction length)
    iso_v = srf.IsoCurve(1, u_mid)

    # Get centered parameters for each direction
    u_params, U_Remainder = centered_params(iso_u, u_spacing, u_ends)
    v_params, V_Remainder = centered_params(iso_v, v_spacing, v_ends)

    # Generate grid points as tree {row}[cols]
    pt_tree = DataTree[rg.Point3d]()
    for j, v in enumerate(v_params):
        path = GH_Path(j)
        for u in u_params:
            pt_tree.Add(srf.PointAt(u, v), path)
    Points = pt_tree

    # Generate sub-surfaces as tree {row}[cols]
    srf_tree = DataTree[rg.NurbsSurface]()
    for j in range(len(v_params) - 1):
        path = GH_Path(j)
        for i in range(len(u_params) - 1):
            u_interval = rg.Interval(u_params[i], u_params[i + 1])
            v_interval = rg.Interval(v_params[j], v_params[j + 1])
            trimmed = srf.Trim(u_interval, v_interval)
            if trimmed:
                srf_tree.Add(trimmed, path)
    SubSrfs = srf_tree

else:
    Points = DataTree[rg.Point3d]()
    SubSrfs = DataTree[rg.NurbsSurface]()
    U_Remainder = 0
    V_Remainder = 0
