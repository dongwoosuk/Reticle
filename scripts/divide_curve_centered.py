"""
Divide Centered - Divide curve from center with equal remainders at both ends

@input: crv, spacing, ends
@output: Points, Remainder

Inputs:
- crv (Curve): Curve to divide
- spacing (float): Segment length (distance between division points)
- ends (bool): Merge end division points with curve endpoints (True = replace first/last division points with curve endpoints, False = all division points + curve endpoints)

Outputs:
- Points (list): Division points
- Remainder (float): Equal remainder length at each end
"""

import Rhino.Geometry as rg
import rhinoscriptsyntax as rs
import math

# Convert Guid to curve geometry if needed
if crv and not isinstance(crv, rg.Curve):
    crv = rs.coercecurve(crv)

# Default ends to True if not provided
try:
    ends = bool(ends)
except:
    ends = True

# Input validation
if crv and spacing and spacing > 0:
    L = crv.GetLength()

    if spacing >= L:
        # Spacing exceeds curve length; no division possible
        Points = []
        Remainder = L / 2.0
    else:
        n = int(math.floor(L / spacing))
        remainder = L - (n * spacing)
        start_offset = remainder / 2.0

        # Compute arc length distances for division points
        distances = [start_offset + i * spacing for i in range(n + 1)]

        # Convert arc length to curve parameter and generate points
        div_points = []
        for d in distances:
            ok, t = crv.LengthParameter(d)
            if ok:
                div_points.append(crv.PointAt(t))

        if ends:
            # Replace first/last division points with curve endpoints
            points = [crv.PointAtStart] + div_points[1:-1] + [crv.PointAtEnd]
            Remainder = start_offset + spacing
        else:
            # All division points + curve endpoints
            points = [crv.PointAtStart] + div_points + [crv.PointAtEnd]
            Remainder = start_offset

        Points = points
else:
    Points = []
    Remainder = 0
