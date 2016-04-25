To build the add-in:

1) Update TemplateExporter version number in Properties/AddinInfo.cs
2) Rebuild TemplateExporter
3a) Run MpackCreator 
3b) Or : run in terminal (from: TemplateExporter/TemplateExporter/bin/debug): "/Applications/Xamarin Studio.app/Contents/MacOS/mdtool" -v setup pack TemplateExporter.dll

To install the add-in:

1) In Xamarin Studio menu select: "Add-in Manager..."
2) Click "Install from file..."
3) navigate to your TemplateExporter.TemplateExporter.mpack file and double click to install
4) Restart Xamarin Studio