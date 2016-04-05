# Xamarin Studio Template Exporter Addin
Xamarin Studio Template Exporter Addin: Exports your project to a Xamarin Studio template

# WORK IN PROGRESS

**Export Project to Template**
* Run the TemplateExporter Project: a new Xamarin Studio instance will be started.
* Open an Android project or create a new one via File > New > Solution... (currently only Android project is supported)
* In Xamarin Studio Menu select File > Export Template, template is now exported and an .mpack file is generated in the root directory of your project (if Export Template option is disabled, make sure you have selected a project in the solution explorer)

**Install Template in Xamarin Studio**
* In Xamarin Studio menu select "Xamarin Studio" > Add-in Manager...
* Click Install from file...
* Navigate to the generated .mpack file and install it
* Restart Xamarin Studio and your template should be available under File > New > Solution...
