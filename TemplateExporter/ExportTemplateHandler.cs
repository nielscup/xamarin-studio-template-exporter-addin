using System;
using System.Text;
using MonoDevelop.Components.Commands;
using MonoDevelop.Projects;
using MonoDevelop.Ide;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Xml;

namespace TemplateExporter
{
	class ExportTemplateHandler:CommandHandler
	{
		Solution solution;
		Project proj;
		List<SolutionItem> projects;
		string rootDir;
		string xptFile;
		string addinFile;
		XmlDocument addinXmlDoc;

		protected override void Run ()
		{
			try {			
			
				var exporterDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

				StringBuilder runtimeXml = new StringBuilder();
				StringBuilder projectsXml = new StringBuilder();

				var templateDir = Path.Combine (rootDir, "ProjectTemplate");

				// Cleanup: Delete template directory if it exists
				if(Directory.Exists(templateDir))
					Directory.Delete (templateDir, true);

				// Create template directory
				Directory.CreateDirectory (templateDir);

				foreach (var project in projects) {
					switch (project.GetType().FullName) {
						case "MonoDevelop.MonoDroid.MonoDroidProject":
							//proj = (Project)project;
							projectsXml.Append(File.ReadAllText(Path.Combine(exporterDir, "Xml", "ProjectAndroid.xml")));
							break;
						case "MonoDevelop.Projects.PortableDotNetProject":
							//proj = (Project)project;
							projectsXml.Append(File.ReadAllText(Path.Combine(exporterDir, "Xml", "ProjectPcl.xml")));
							break;
						case "MonoDevelop.IPhone.XamarinIOSProject":
							//proj = (Project)project;
							projectsXml.Append(File.ReadAllText(Path.Combine(exporterDir, "Xml", "ProjectiOS.xml")));
							break;
						default:
							continue;
							break;
					}

					proj = (Project)project;
					var files = proj.Files;

					if (!files.Any ())
						return;
					
					StringBuilder filesXml = new StringBuilder();

					string packages = "";
					StringBuilder references = new StringBuilder();

					foreach (var reference in ((MonoDevelop.Projects.DotNetProject)proj).References) {
						if(reference.ReferenceType != ReferenceType.Assembly)
							references.Append(string.Format("<Reference type=\"{0}\" refto=\"{1}\"/>\n\t\t\t\t", reference.ReferenceType.ToString(), reference.Reference.Replace(solution.Name, "${ProjectName}")));							
					}

					runtimeXml.Append("<Import file=\"ProjectTemplate.xpt.xml\" />");

					int i = -1;
					foreach (var file in files) 
					{
						if(file.FilePath.ToString().ToLower().EndsWith("packages.config"))
						{
							// Get packages from packages.config
							packages = GetPackages(file);
							continue;
						}

						i++;
						// Exclude files from template
						if(file.FilePath.ToString().ToLower().Contains("/bin/") || 
							file.FilePath.ToString().ToLower().Contains("/projecttemplate/") ||
							file.FilePath.ToString().ToLower().Contains("/packages/") ||
							file.FilePath.ToString().ToLower().EndsWith(".xpt.xml") ||
							file.FilePath.ToString().ToLower().EndsWith(".addin.xml") ||
							file.FilePath.Extension == string.Empty)
						{	
							Console.WriteLine("{0} Export Template SKIP: {1}", i, file.FilePath);
							continue;
						}

						// Copy or Create files from project to ProjectTemplate directory
						var dir = Path.GetDirectoryName(file.FilePath).Replace(rootDir, templateDir); //Path.Combine(rootDir, projectTemplate));
						Directory.CreateDirectory (dir);
						var templateFilePath = Path.Combine(dir, file.ProjectVirtualPath.FileName);
						Console.WriteLine("{0} Export Template Copy: {1}", i, file.FilePath);

						if(file.ProjectVirtualPath.Extension.ToLower() == ".png")
						{
							// copy file
							File.Copy(file.FilePath, templateFilePath, true);
						}
						else
						{
							// create new file, so we can replace namespaces and projectname					
							var content = File.ReadAllText(file.FilePath);
							content = content.Replace(proj.Name, "${Namespace}");
							content = content.Replace(solution.Name, "${SolutionName}");
							CreateFile(templateFilePath, content, true);
						}

						runtimeXml.Append(string.Format("\n\t\t<Import file=\"{0}/{1}\" />", proj.Name, file.ProjectVirtualPath));
						AppendFile(ref filesXml, file, proj.Name);
					}

					// Replace placeholders
					projectsXml = projectsXml.Replace("[FILES]", filesXml.ToString());
					projectsXml = projectsXml.Replace("[PACKAGES]", packages);
					projectsXml = projectsXml.Replace("[DIRECTORY]", proj.Name);
					projectsXml = projectsXml.Replace("[REFERENCES]", references.ToString());
					projectsXml = projectsXml.Replace("[PROJECTTYPE]", proj.ProjectType);

				}

				LoadXmlFiles();

//				// Set template xml file paths
//				xptFile = Path.Combine(rootDir, "ProjectTemplate.xpt.xml");
//				addinFile = Path.Combine(rootDir, solution.Name + ".addin.xml");
//
//				addinXmlDoc = new XmlDocument ();			
//				addinXmlDoc.Load (addinFile);

				// Get templated xml				
				var xptTemplateXml = File.ReadAllText(Path.Combine(exporterDir, "Xml", "Xpt.xml"));
				var addInTemplateXml = File.ReadAllText(Path.Combine(exporterDir, "Xml", "AddIn.xml"));

				// Creates the template files if not exists for target project
				CreateProjectFile (xptFile, xptTemplateXml);
				CreateProjectFile (addinFile, addInTemplateXml);

				// Get xml from template files
				var xptXml = File.ReadAllText(xptFile);
				var addInXml = File.ReadAllText(addinFile);

				var version = GetAddInAttr("Addin", "version");
				var templateName = GetAddInAttr("Addin", "name");
				var templateDescription = GetAddInAttr("Addin", "description");
				var category = GetCategory();

				// Replace placeholders
				addInXml = addInXml.Replace("[PROJECTNAME]", solution.Name);
				addInXml = addInXml.Replace("[RUNTIME]", runtimeXml.ToString());
				xptXml = xptXml.Replace("[VERSION]", string.Format ("v{0}", version));
				xptXml = xptXml.Replace("[PROJECTS]", projectsXml.ToString());
				xptXml = xptXml.Replace("[TEMPLATENAME]", templateName);
				xptXml = xptXml.Replace("[TEMPLATEDESCRIPTION]", templateDescription);
				xptXml = xptXml.Replace("[CATEGORY]", category);

				// Write template files
				File.WriteAllText (xptFile.Replace(rootDir, templateDir), xptXml);				
				File.WriteAllText (addinFile.Replace(rootDir, templateDir), addInXml);

				// create .mpack
				if(!RunMDTool(templateDir, string.Format("-v setup pack {0}.addin.xml", solution.Name)))
				{
					Console.WriteLine("Export Template ERROR: Unable to generate .mpack");
					return;
				}

				var mpack = string.Format("MonoDevelop.{0}_{1}.mpack", solution.Name, version);
				File.Copy(Path.Combine(templateDir, mpack), Path.Combine(rootDir, mpack), true);

				// install .mpack: INSTALL NOT WORKING...
//				if(!RunMDTool(rootDir, string.Format("-v setup install -y {0}", mpack)))
//				{
//					Console.WriteLine("Export Template ERROR: Unable to install .mpack");
//					return;
//				}

				Console.WriteLine("Export Template SUCCESS.");
			} 
			catch (Exception ex) 
			{
				// Log exception
				Console.WriteLine ("Export Template EXCEPTION: {0}", ex.Message);
			}
		}

		private void LoadXmlFiles()
		{
			// Set template xml file paths
			xptFile = Path.Combine(rootDir, "ProjectTemplate.xpt.xml");
			addinFile = Path.Combine(rootDir, solution.Name + ".addin.xml");

			addinXmlDoc = new XmlDocument ();			
			addinXmlDoc.Load (addinFile);

		}

		protected override void Update (CommandInfo info)
		{		
			if (IdeApp.ProjectOperations.CurrentSelectedItem is MonoDevelop.Projects.Solution) {
				info.Enabled = true;

				solution = (MonoDevelop.Projects.Solution)IdeApp.ProjectOperations.CurrentSelectedItem;
				rootDir = solution.ItemDirectory.ToString ();
				projects = solution.Items.ToList();
			}
			else
			{
				info.Enabled = false;
				projects = null;
			}
		}

		private void AppendFile(ref StringBuilder filesXml, ProjectFile file, string projectDir)
		{
			var path = file.ProjectVirtualPath.ToString().Replace(file.ProjectVirtualPath.FileName, "");
			var subDirs = path.Split('/').ToList();
			subDirs.RemoveAll (x => x == "");

			foreach (var dir in subDirs) {
				// add directory node
				filesXml.Append(string.Format("\n\t\t\t\t<Directory name=\"{0}\">", dir));
			}

			// append file
			if(IsRawFile(file))
				filesXml.Append(string.Format("\n\t\t\t\t<RawFile name=\"{0}\" src=\"{1}\" />", file.ProjectVirtualPath.FileName, Path.Combine(projectDir, file.ProjectVirtualPath)));
			else
				filesXml.Append(string.Format("\n\t\t\t\t<File name=\"{0}\" src=\"{1}\" />", file.ProjectVirtualPath.FileName, Path.Combine(projectDir, file.ProjectVirtualPath)));

			for (int i = 0; i < subDirs.Count(); i++) {
				// close directory node
				filesXml.Append("\n\t\t\t\t</Directory>");
			}
		}

		string GetPackages(ProjectFile file)
		{
			var packages = File.ReadAllText(file.FilePath);
			packages = packages.Replace ("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
			packages = packages.Replace ("<packages>", "");
			packages = packages.Replace ("</packages>", "");

			return packages.Trim();
		}

		private bool IsRawFile(ProjectFile file)
		{
			return file.ProjectVirtualPath.Extension.ToLower () == ".png"
				|| file.ProjectVirtualPath.Extension.ToLower () == ".txt";
		}

		/// <summary>
		/// Gets the category from the addin.xml and converts it to the category path needed in xpt.xml, This way we only need to set the category once in the addin.xml.
		/// </summary>
		/// <returns>The category.</returns>
		private string GetCategory()
		{
			var extensions = addinXmlDoc.SelectNodes ("/Addin/Extension");

			XmlNode categoryExtension = null;
			foreach (XmlNode extension in extensions) {
				if(extension.Attributes.GetNamedItem("path").Value == "/MonoDevelop/Ide/ProjectTemplateCategories")
				{
					categoryExtension = extension;
					break;
				}
			}

			var cat1 = categoryExtension.SelectSingleNode ("Category");
			var cat2 = cat1.SelectSingleNode ("Category");
			var cat3 = cat2.SelectSingleNode ("Category");

			var category = Path.Combine (cat1.Attributes.GetNamedItem ("id").Value, cat2.Attributes.GetNamedItem ("id").Value, cat3.Attributes.GetNamedItem ("id").Value);

			return category;
		}

		private string GetAddInAttr(string node, string attr)
		{	
			var addinNode = addinXmlDoc.SelectSingleNode(node);
			return addinNode.Attributes.GetNamedItem(attr).Value;
		}

		private bool RunMDTool(string rootDir, string arguments)
		{	
			Console.WriteLine("Running mdtool: " + arguments);
			var processStartInfo = new ProcessStartInfo
			{
				FileName = "/Applications/Xamarin Studio.app/Contents/MacOS/mdtool",
				UseShellExecute = false,
				Arguments = arguments,
				WorkingDirectory = rootDir,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				RedirectStandardInput = true,
			};

			var process = Process.Start(processStartInfo);
			var error = process.StandardError.ReadToEnd ();
			var output = process.StandardError.ReadToEnd ();

			process.WaitForExit();

			Console.WriteLine("output: " + output);
			Console.WriteLine("error: " + error);
			Console.WriteLine("exitCode: " + process.ExitCode);

			if (process.ExitCode != 0) {
				Console.WriteLine("mdtool EXCEPTION: exitCode: {0}", process.ExitCode);
				return false;
			}

			return true;
		}

		private void CreateProjectFile(string path, string content, bool overwriteIfExists = false)
		{
			if(CreateFile(path, content, overwriteIfExists))
			{		
				var templateFolder = solution.GetAllSolutionItems<SolutionFolder>().FirstOrDefault (x => x.Name == "Template");
				//var templateFolder = solutionFolders.FirstOrDefault (x => x.Name == "Template");
				if (templateFolder == null) {
					// Create Template solution folder
					templateFolder = new SolutionFolder { Name = "Template" };
					solution.RootFolder.AddItem (templateFolder);
				}

				IdeApp.ProjectOperations.AddFilesToSolutionFolder (templateFolder, new string[] { path });
				solution.Save (new MonoDevelop.Core.ProgressMonitoring.NullProgressMonitor());

				//projectTemplateFolder.AddItem(

				//IdeApp.ProjectOperations.AddSolutionItem (solution.RootFolder);
				//solutionFolder.Name = "ProjectTemplate";

				//IdeApp.ProjectOperations.AddFilesToSolutionFolder (solutionFolder);
				//IdeApp.ProjectOperations.AddFilesToSolutionFolder (solutionFolder, new string[] { path });

				// Add file to solution
				//proj.AddFile(path);
				//proj.Save (null);
			}
		}

		private bool CreateFile(string path, string content, bool overwriteIfExists = false)
		{
			if (!overwriteIfExists && File.Exists (path))
				return false;

			File.WriteAllText (path, content);

			return true;
		}
	}
}