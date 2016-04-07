# Xamarin Studio Template Exporter Addin
Xamarin Studio Template Exporter Addin: Exports your project to a Xamarin Studio template

# WORK IN PROGRESS

**Supports**
* Android Projects
* iOS Projects
* PCL Projects

**Export Project to Template**
* Run the TemplateExporter Project: a new Xamarin Studio instance will be started.
* Open an existing project or create a new one via File > New > Solution...
* In Xamarin Studio Menu select File > Export Template (if Export Template option is disabled, make sure you have selected the solution in the solution explorer)
* The template is now exported and an .mpack file is generated in the root directory of your solution
* Also a new solution folder is added to your solution containing two .xml. Here you can change the temaplate name, version, etc.

**Install Template in Xamarin Studio**
* In Xamarin Studio menu select "Xamarin Studio" > Add-in Manager...
* Click "Install from file..."
* Navigate to the generated .mpack file and install it
* Restart Xamarin Studio and your template should be available under File > New > Solution...
