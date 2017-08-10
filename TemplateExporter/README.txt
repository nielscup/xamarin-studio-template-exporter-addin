To be able to OPEN the TemplateExporter project:

1) Open Visual Studio for Mac
2) Visual Studio Menu > Add-ins..
3) Addin Development > Addin maker > Install


To BUILD TemplateExporter add-in:

1) Update TemplateExporter version number in Properties/AddinInfo.cs
2) Rebuild TemplateExporter
3a) Run MpackCreator 
3b) Or: run in terminal (from: TemplateExporter/TemplateExporter/bin/debug): "/Applications/Xamarin Studio.app/Contents/MacOS/mdtool" -v setup pack TemplateExporter.dll


To INSTALL TemplateExporter add-in in Xamarin Studio:

1) In Visual Studio for Mac menu select: "Add-in Manager..."
2) Click "Install from file..."
3) navigate to your TemplateExporter.TemplateExporter.mpack file and double click to install
4) Restart Visual Studio


To DEBUG TemplateExporter add-in:
1) Set TemplateExporter as Startup project
2) Run
3) A new instance of XS will be opened: In this new instance, open a project that you would like to export
4) Select the solution in the solution explorer
5) Menu: File > Export Template
6) You should now be able to debug the ExportTemplateHandler.cs


RELEASE NOTES:

0.20:
Updated for Visual Studio for Mac

0.11:
Added Xamarin Studio 6 support