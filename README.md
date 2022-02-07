# PrintAWordle
Print-A-Wordle STL Generator
This is the source code for the Print-A-Wordle STL generator found on http://3dmodels.tgaw.com.  This application works in conjunction with OpenSCAD which can be found at https://openscad.org.

The 3D Model is powered by an OpenSCAD project.  That can be found in the Content/OpenSCADCode directory.

The web.config file for the Web App defines a few key paths:
- OpenSCADExecutable - This is the path to the OpenSCAD executable.  On Windows machines, this is typically C:\Program Files\OpenSCAD\openscad.exe" 
- CodeDirectory - This is the path of where the OpenSCAD project will be found. In this code structure, it is in the Content\OpenSCADCode directory.
- ModelDirectory - OpenSCAD will output STL files.  This is the directory of where those STL files will be saved.
- LogFile - The path and name of a text file of where errors will be logged.
