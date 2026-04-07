# Reticle

Centered division tools for Grasshopper. Divide curves and surfaces with equal spacing from center, leaving equal remainders at both ends.

![Rhino 8](https://img.shields.io/badge/Rhino-8-blue)
![Grasshopper](https://img.shields.io/badge/Grasshopper-Add--on-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

## Why Reticle?

Grasshopper's built-in `Divide Distance` and `Divide Length` components start dividing from one end of a curve, leaving an uneven remainder at the other end. **Reticle** solves this by dividing from the center outward, ensuring equal remainders on both sides.

## Demo

### Divide Centered (Curve)

https://github.com/user-attachments/assets/b5105d13-4e4f-4a18-ab2f-c63f6dd4593c

### Divide Surface Centered

https://github.com/user-attachments/assets/96609f25-d2c6-4912-a693-e831332a7a71

## Components

### Divide Centered (Curve)
Divide a curve with equal spacing from center.

| | Input | Description |
|---|---|---|
| **C** | Curve | Curve to divide |
| **D** | Distance | Segment length |
| **E** | Ends | `True` = merge end points with curve endpoints, `False` = all division points + endpoints |

| | Output | Description |
|---|---|---|
| **P** | Points | Division points |
| **R** | Remainder | Equal remainder length at each end |

### Divide Surface Centered (Surface)
Divide a surface UV from center with equal spacing.

| | Input | Description |
|---|---|---|
| **S** | Surface | Surface to divide |
| **U** | U Distance | Segment length in U direction |
| **V** | V Distance | Segment length in V direction |
| **Eu** | U Ends | U direction ends toggle |
| **Ev** | V Ends | V direction ends toggle |

| | Output | Description |
|---|---|---|
| **P** | Points | Division points grid (tree) |
| **S** | Surfaces | Sub-surfaces (tree) |
| **Ru** | U Remainder | Equal remainder at U edges |
| **Rv** | V Remainder | Equal remainder at V edges |

## Installation

### Option 1: Download .gha
1. Download `DivideCentered.gha` from [Releases](https://github.com/dongwoosuk/Reticle/releases)
2. Copy to `%AppData%\Grasshopper\Libraries\`
3. Unblock the file (right-click > Properties > Unblock)
4. Restart Rhino

### Option 2: Build from source
```bash
cd src
dotnet build -c Release
```
The compiled `.gha` will be in `src/bin/Release/net48/`.

## Python Scripts

Standalone GH Python Script versions are available in the `scripts/` folder for use without installing the plugin.

## Requirements

- Rhino 8
- Grasshopper
- .NET Framework 4.8 (for building from source)

## License

[MIT](LICENSE)
