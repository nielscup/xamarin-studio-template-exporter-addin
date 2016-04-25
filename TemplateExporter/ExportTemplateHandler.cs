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
using System.Threading.Tasks;

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
		bool isSolutionTemplateCreated;
		MonoDevelop.Ide.Gui.Dialogs.ProgressDialog progressDialog;

		protected override void Run ()
		{			
			progressDialog = new MonoDevelop.Ide.Gui.Dialogs.ProgressDialog(false, true);
			progressDialog.Title = "Export Template";
			Log ("0");

			try {
				progressDialog.Progress = 0;
				progressDialog.Show();

				//Log ("1");

				//var exporterDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

				//Log ("2");

				StringBuilder runtimeXml = new StringBuilder();
				StringBuilder projectsXml = new StringBuilder();

				var templateDir = Path.Combine (rootDir, "ProjectTemplate");

				//Log ("3");

				// Cleanup: Delete template directory if it exists
				if(Directory.Exists(templateDir))
					Directory.Delete (templateDir, true);

				//Log ("4");

				// Create template directory
				Directory.CreateDirectory (templateDir);

				//Log ("5");

				foreach (var project in projects) {
					//Log ("6");
					switch (project.GetType().FullName) {
						case "MonoDevelop.MonoDroid.MonoDroidProject":
							//projectsXml.Append(File.ReadAllText(Path.Combine(exporterDir, "Xml", "ProjectAndroid.xml")));
							projectsXml.Append(Xml.AndroidXml);
							break;
						case "MonoDevelop.Projects.PortableDotNetProject":
							//projectsXml.Append(File.ReadAllText(Path.Combine(exporterDir, "Xml", "ProjectPcl.xml")));
							projectsXml.Append(Xml.PclXml);
							break;
						case "MonoDevelop.IPhone.XamarinIOSProject":
							projectsXml.Append(Xml.iOSXml);
							//projectsXml.Append(File.ReadAllText(Path.Combine(exporterDir, "Xml", "ProjectiOS.xml")));
							break;
//						case "MonoDevelop.Projects.DotNetAssemblyProject":
//							if(project.Name.ToLower() == "templatefinalizer")
//								projectsXml.Append(File.ReadAllText(Path.Combine(exporterDir, "Xml", "ProjectTemplateFinalizer.xml")));
//							else
//								projectsXml.Append(File.ReadAllText(Path.Combine(exporterDir, "Xml", "ProjectDotNetAssembly.xml")));						
//							break;
						default:
							continue;
					}

					progressDialog.Progress = 100;

					//Log ("7");
					proj = (Project)project;
					AddOriginalProjectFile(proj);
					AddOriginalSolutionFile(proj);

					var files = proj.Files;

					//Log ("8");
					if (!files.Any ())
						return;
					
					StringBuilder filesXml = new StringBuilder();
					StringBuilder referencesXml = new StringBuilder();
					string packagesXml = "";

					//Log ("9");
					foreach (var reference in ((MonoDevelop.Projects.DotNetProject)proj).References) {
						if(reference.ReferenceType != ReferenceType.Assembly)
							referencesXml.Append(string.Format("<Reference type=\"{0}\" refto=\"{1}\"/>\n\t\t\t\t", reference.ReferenceType.ToString(), reference.Reference.Replace(solution.Name, "${ProjectName}")));							
					}

					//Log ("10");
					runtimeXml.Append("<Import file=\"ProjectTemplate.xpt.xml\" />");

					int i = -1;
					foreach (var file in files) 
					{
						if(file.FilePath.ToString().ToLower().EndsWith("packages.config"))
						{
							// Get packages from packages.config
							packagesXml = GetPackages(file);
							continue;
						}

						i++;

						// Exclude unnecassary files from template
						if(ExcludeFile(file, i)) continue;
							
//						// Exclude files from template
//						if(file.FilePath.ToString().ToLower().Contains("/bin/") || 
//							file.FilePath.ToString().ToLower().Contains("/projecttemplate/") ||
//							file.FilePath.ToString().ToLower().Contains("/packages/") ||
//							file.FilePath.ToString().ToLower().EndsWith(".xpt.xml") ||
//							file.FilePath.ToString().ToLower().EndsWith(".addin.xml") ||
//							file.FilePath.Extension == string.Empty)
//						{	
//							Console.WriteLine("{0} Export Template SKIP: {1}", i, file.FilePath);
//							continue;
//						}

						// Copy or Create files from project to ProjectTemplate directory
						var dir = Path.GetDirectoryName(file.FilePath).Replace(rootDir, templateDir);
						Directory.CreateDirectory (dir);
						var templateFilePath = Path.Combine(dir, file.ProjectVirtualPath.FileName);
						Log(string.Format("{0} Export Template Copy: {1}", i, file.FilePath));
						//Console.WriteLine("{0} Export Template Copy: {1}", i, file.FilePath);

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
					projectsXml = projectsXml.Replace("[PACKAGES]", packagesXml);
					projectsXml = projectsXml.Replace("[REFERENCES]", referencesXml.ToString());
					projectsXml = projectsXml.Replace("[PROJECTTYPE]", proj.ProjectType);
					projectsXml = projectsXml.Replace("[DIRECTORY]", proj.Name);

				}

				LoadTemplateFiles();

				// Get templated xml				
				//var xptTemplateXml = File.ReadAllText(Path.Combine(exporterDir, "Xml", "Xpt.xml"));
				//var addInTemplateXml = File.ReadAllText(Path.Combine(exporterDir, "Xml", "AddIn.xml"));

				// Creates the template files (addin.xml and xpt.xml) if not exists for target project
				AddSolutionFile (xptFile, Xml.XptXml, "Template");
				AddSolutionFile (addinFile, Xml.AddinXml, "Template");
//				AddSolutionFile (xptFile, xptTemplateXml, "Template");
//				AddSolutionFile (addinFile, addInTemplateXml, "Template");

				// Get xml from template files
				var xptXml = File.ReadAllText(xptFile);
				var addInXml = File.ReadAllText(addinFile);

				// Get attributes from addin.xml
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
					Log(string.Format("Export Template ERROR: Unable to generate .mpack"));
					//Console.WriteLine("Export Template ERROR: Unable to generate .mpack");
					return;
				}

				var mpack = string.Format("MonoDevelop.{0}_{1}.mpack", solution.Name, version);
				var mpackPath = Path.Combine(rootDir, mpack);
				File.Copy(Path.Combine(templateDir, mpack), mpackPath, true);

				// install .mpack: INSTALL NOT WORKING :-(
//				if(!RunMDTool(rootDir, string.Format("-v setup install -y {0}", mpack)))
//				{
//					Console.WriteLine("Export Template ERROR: Unable to install .mpack");
//					return;
//				}

				//Log(string.Format("Export Template SUCCESS {0}", mpack));
				//Console.WriteLine("Export Template SUCCESS.");

				// Display Success message:
				progressDialog.ShowDone(false, false);
				Log(string.Format("Template successfully exported to: {0}", mpackPath));
				//progressDialog.WriteText("Template successfully exported to: "+ mpackPath);
				progressDialog.Message = "Export Template SUCCESS: " + mpack;

			} 
			catch (Exception ex) 
			{
				// Log exception
				//Console.WriteLine ("Export Template EXCEPTION: {0}", ex.Message);
				Log(string.Format("Export Template EXCEPTION: {0}", ex.Message));

				// Display Error message:
				progressDialog.ShowDone(false, true);
				//progressDialog.WriteText (ex.Message);
				progressDialog.WriteText (ex.InnerException.Message);
			}
		}

		private void AddOriginalProjectFile(Project project)
		{
			var projectFilePath = Path.Combine(project.BaseDirectory.ToString(), project.Name + ".csproj");
			var destinationPath = Path.Combine(project.BaseDirectory.ToString(), project.Name.Replace(project.Name, "TEMPLATE") + ".csproj.txt");

			if(!File.Exists(projectFilePath))
				return;

			var projectFileContent = File.ReadAllText(projectFilePath);
			projectFileContent = projectFileContent.Replace(project.Name, "[PROJECTNAME]");
			AddProjectFile (destinationPath, projectFileContent, true);
		}

		private void AddOriginalSolutionFile(Project project)
		{
			if (isSolutionTemplateCreated)
				return;
			
			var solutionFilePath = Path.Combine(solution.BaseDirectory.ToString(), solution.Name + ".sln");
			var destinationPath = Path.Combine(project.BaseDirectory.ToString(), solution.Name.Replace(solution.Name, "TEMPLATE") + ".sln.txt");

			if(!File.Exists(solutionFilePath))
				return;

			var solutionFileContent = File.ReadAllText(solutionFilePath);
			solutionFileContent = solutionFileContent.Replace(solution.Name, "[SOLUTIONNAME]");

			AddProjectFile (destinationPath, solutionFileContent, true);
			isSolutionTemplateCreated = true;
		}

		private bool ExcludeFile(ProjectFile file, int i)
		{
			// Exclude unnecassary files from template
			if(file.FilePath.ToString().ToLower().Contains("/bin/") || 
				file.FilePath.ToString().ToLower().Contains("/projecttemplate/") ||
				file.FilePath.ToString().ToLower().Contains("/packages/") ||
				file.FilePath.ToString().ToLower().EndsWith(".xpt.xml") ||
				file.FilePath.ToString().ToLower().EndsWith(".addin.xml") ||
				file.FilePath.Extension == string.Empty)
			{	
				//Console.WriteLine("{0} Export Template SKIP: {1}", i, file.FilePath);
				Log(string.Format("{0} Export Template SKIP: {1}", i, file.FilePath));
				return true;
			}

			return false;
		}

		private void LoadTemplateFiles()
		{
			// Set template xml file paths
			xptFile = Path.Combine(rootDir, "ProjectTemplate.xpt.xml");
			addinFile = Path.Combine(rootDir, solution.Name + ".addin.xml");

			addinXmlDoc = new XmlDocument ();			
			addinXmlDoc.Load (addinFile);

		}

		/// <summary>
		/// Update the "Export Template" menu item based on if the solution is selected.
		/// </summary>
		/// <param name="info">Info.</param>
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

		/// <summary>
		/// Appends file file to the filesXml.
		/// </summary>
		/// <param name="filesXml">Files xml.</param>
		/// <param name="file">File.</param>
		/// <param name="projectDir">Project dir.</param>
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

		/// <summary>
		/// Gets the packages from the packages.config file.
		/// </summary>
		/// <returns>The packages xml.</returns>
		/// <param name="file">File.</param>
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

		/// <summary>
		/// Gets the specified attribute from the addin.xml file.
		/// </summary>
		/// <returns>The add in attr.</returns>
		/// <param name="node">Node.</param>
		/// <param name="attr">Attr.</param>
		private string GetAddInAttr(string node, string attr)
		{	
			var addinNode = addinXmlDoc.SelectSingleNode(node);
			return addinNode.Attributes.GetNamedItem(attr).Value;
		}

		/// <summary>
		/// Runs the MD tool.
		/// </summary>
		/// <returns><c>true</c>, if MD tool was run, <c>false</c> otherwise.</returns>
		/// <param name="rootDir">Root dir.</param>
		/// <param name="arguments">Arguments.</param>
		private bool RunMDTool(string workingDir, string arguments)
		{	
			//Console.WriteLine("Running mdtool: " + arguments);
			Log(string.Format("Running mdtool: {0}", arguments));
			var processStartInfo = new ProcessStartInfo
			{
				FileName = "/Applications/Xamarin Studio.app/Contents/MacOS/mdtool",
				UseShellExecute = false,
				Arguments = arguments,
				WorkingDirectory = workingDir,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				RedirectStandardInput = true,
			};

			var process = Process.Start(processStartInfo);
			var error = process.StandardError.ReadToEnd ();
			var output = process.StandardError.ReadToEnd ();

			process.WaitForExit();

			//Log("output: " + output);
			//Log("error: " + error);
			Log("exitCode: " + process.ExitCode);

//			Console.WriteLine("output: " + output);
//			Console.WriteLine("error: " + error);
//			Console.WriteLine("exitCode: " + process.ExitCode);

			if (process.ExitCode != 0) {
				//Console.WriteLine("mdtool EXCEPTION: exitCode: {0}", process.ExitCode);
				Log(string.Format("mdtool EXCEPTION: exitCode: {0}", process.ExitCode));
				return false;
			}

			return true;
		}

		private void AddSolutionFile(string path, string content, string solutionFolderName, bool overwriteIfExists = false)
		{
			if(CreateFile(path, content, overwriteIfExists))
			{		
				var templateFolder = solution.GetAllSolutionItems<SolutionFolder>().FirstOrDefault (x => x.Name == solutionFolderName);
				if (templateFolder == null) {
					// Create Template solution folder
					templateFolder = new SolutionFolder { Name = "Template" };
					solution.RootFolder.AddItem (templateFolder);
				}

				IdeApp.ProjectOperations.AddFilesToSolutionFolder (templateFolder, new string[] { path });
				solution.Save (new MonoDevelop.Core.ProgressMonitoring.NullProgressMonitor());
			}
		}

		private void AddProjectFile(string path, string content, bool overwriteIfExists = false)
		{
			if(CreateFile(path, content, overwriteIfExists))
			{		
				proj.AddFile (path);
				proj.Save(new MonoDevelop.Core.ProgressMonitoring.NullProgressMonitor());
			}
		}

		private bool CreateFile(string path, string content, bool overwriteIfExists = false)
		{
			if (!overwriteIfExists && File.Exists (path))
				return false;

			File.WriteAllText (path, content);

			return true;
		}

		private void Log(string log)
		{
			try {
				progressDialog.WriteText(log + Environment.NewLine);
				Console.WriteLine (log);
			} catch (Exception ex) {
				progressDialog.WriteText("LOG ERROR: {0}" + ex.Message);
			}
		}
	}
}