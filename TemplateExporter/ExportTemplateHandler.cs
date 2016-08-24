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

		protected override async void Run ()
		{			
			progressDialog = new MonoDevelop.Ide.Gui.Dialogs.ProgressDialog(false, true);
			progressDialog.Title = "Export Template";
			progressDialog.Message = "Exporting " + solution.Name + "...";

			Log ("0");

			try {
				progressDialog.Progress = 0;
				progressDialog.Show();

				StringBuilder runtimeXml = new StringBuilder();
				StringBuilder projectsXml = new StringBuilder();

				var templateDir = Path.Combine (rootDir, "ProjectTemplate");

				// Cleanup: Delete template directory if it exists
				if(Directory.Exists(templateDir))
					Directory.Delete (templateDir, true);

				// Create template directory
				Directory.CreateDirectory (templateDir);

				double progressInterval = 1.0 / projects.Count();

				// List of project type guids http://www.codeproject.com/Reference/720512/List-of-Visual-Studio-Project-Type-GUIDs
				// since XS6 all exposed projecttype guids are general C# guids, 
				// but we need the specific project type guids which can be found in the .csproj <ProjectTypeGuids> node.
				foreach (var project in projects) {
					var projType = ((DotNetProject)project).TargetFramework.Id.Identifier;
					switch (projType)
					{
						case "MonoAndroid":
							projectsXml.Append(Xml.AndroidXml);
							projectsXml = projectsXml.Replace("[PROJECTTYPE]", "{EFBA0AD7-5A72-4C68-AF49-83D382785DCF}");
							break;
						case ".NETPortable":
							projectsXml.Append(Xml.PclXml);
							projectsXml = projectsXml.Replace("[PROJECTTYPE]", "{786C830F-07A1-408B-BD7F-6EE04809D6DB}");
							break;
						case "Xamarin.iOS":
							projectsXml.Append(Xml.iOSXml);
							projectsXml = projectsXml.Replace("[PROJECTTYPE]", "{FEACFBD2-3405-455C-9665-78FE426C6842}");
							break;
						//case ".NETFramework":
						//	// UnitTest project DOES NOT WORK: unable to generate mpack...
						//	projectsXml.Append(Xml.UnitTestXml);
						//	projectsXml = projectsXml.Replace("[PROJECTTYPE]", "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
						//	break;
						default:
							continue;
					}

					proj = (Project)project;
					//var dotNetProj = (DotNetProject)project;
					//dotNetProj.TargetFramework.
					await AddOriginalProjectFile(proj);
					await AddOriginalSolutionFile(proj);

					var files = proj.Files;

					if (!files.Any ())
						return;
					
					StringBuilder filesXml = new StringBuilder();
					StringBuilder referencesXml = new StringBuilder();
					string packagesXml = "";

					foreach (var reference in ((MonoDevelop.Projects.DotNetProject)proj).References) {
						if(reference.ReferenceType != ReferenceType.Assembly)
							referencesXml.Append(string.Format("<Reference type=\"{0}\" refto=\"{1}\"/>\n\t\t\t\t", reference.ReferenceType.ToString(), reference.Reference.Replace(solution.Name, "${ProjectName}")));							
					}

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

						// Copy or Create files from project to ProjectTemplate directory
						var dir = Path.GetDirectoryName(file.FilePath).Replace(rootDir, templateDir);
						Directory.CreateDirectory (dir);
						var templateFilePath = Path.Combine(dir, file.ProjectVirtualPath.FileName);
						Log(string.Format("{0} Export Template Copy: {1}", i, file.FilePath));

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
					//projectsXml = projectsXml.Replace("[PROJECTTYPE]", proj.TypeGuid);
					//projectsXml = projectsXml.Replace("[PROJECTTYPE]", "{786C830F-07A1-408B-BD7F-6EE04809D6DB}");
					projectsXml = projectsXml.Replace("[DIRECTORY]", proj.Name);
					projectsXml = projectsXml.Replace("[TARGETFRAMEWORK]", ((DotNetProject)project).TargetFramework.Id.ToString());

					progressDialog.Progress += progressInterval;
				}

				LoadTemplateFiles();

				// Creates the template files (addin.xml and xpt.xml) if not exists for target project
				await AddSolutionFile (xptFile, Xml.XptXml, "Template");
				await AddSolutionFile (addinFile, Xml.AddinXml, "Template");

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
					// Display Error message:
					progressDialog.ShowDone(false, true);
					Log(string.Format("Export Template ERROR: Unable to generate .mpack"));

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

				// Display Success message:
				progressDialog.ShowDone(false, false);
				Log(string.Format("Template successfully exported to: {0}", mpackPath));
				progressDialog.Message = "Export Template SUCCESS: " + mpack;

			} 
			catch (Exception ex) 
			{				
				// Display Error message:
				progressDialog.ShowDone(false, true);

				// Log exception
				Log(string.Format("Export Template EXCEPTION: {0}", ex.Message));
			}
		}

		private async Task AddOriginalProjectFile(Project project)
		{
			var projectFilePath = Path.Combine(project.BaseDirectory.ToString(), project.Name + ".csproj");
			var destinationPath = Path.Combine(project.BaseDirectory.ToString(), project.Name.Replace(project.Name, "TEMPLATE") + ".csproj.txt");

			if(!File.Exists(projectFilePath))
				return;

			var projectFileContent = File.ReadAllText(projectFilePath);
			projectFileContent = projectFileContent.Replace(project.Name, "[PROJECTNAME]");
			await AddProjectFile (destinationPath, projectFileContent, true);
		}

		private async Task AddOriginalSolutionFile(Project project)
		{
			if (isSolutionTemplateCreated)
				return;
			
			var solutionFilePath = Path.Combine(solution.BaseDirectory.ToString(), solution.Name + ".sln");
			var destinationPath = Path.Combine(project.BaseDirectory.ToString(), solution.Name.Replace(solution.Name, "TEMPLATE") + ".sln.txt");

			if(!File.Exists(solutionFilePath))
				return;

			var solutionFileContent = File.ReadAllText(solutionFilePath);
			solutionFileContent = solutionFileContent.Replace(solution.Name, "[SOLUTIONNAME]");

			await AddProjectFile (destinationPath, solutionFileContent, true);
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

			if (process.ExitCode != 0) {
				Log(string.Format("mdtool EXCEPTION: exitCode: {0}", process.ExitCode));
				return false;
			}

			return true;
		}

		private async Task AddSolutionFile(string path, string content, string solutionFolderName, bool overwriteIfExists = false)
		{
			if(CreateFile(path, content, overwriteIfExists))
			{
				//var templateFolder = solution.GetAllSolutionItems<SolutionFolder>().FirstOrDefault(x => x.Name == solutionFolderName);
				var templateFolder = solution.GetAllItems<SolutionFolder>().FirstOrDefault (x => x.Name == solutionFolderName);
				if (templateFolder == null) {
					// Create Template solution folder
					templateFolder = new SolutionFolder { Name = "Template" };
					solution.RootFolder.AddItem (templateFolder);
				}

				IdeApp.ProjectOperations.AddFilesToSolutionFolder (templateFolder, new string[] { path });
				await solution.SaveAsync (new MonoDevelop.Core.ProgressMonitoring.ConsoleProgressMonitor());
			}
		}

		private async Task AddProjectFile(string path, string content, bool overwriteIfExists = false)
		{
			if(CreateFile(path, content, overwriteIfExists))
			{		
				proj.AddFile (path);
				await proj.SaveAsync(new MonoDevelop.Core.ProgressMonitoring.ConsoleProgressMonitor());
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