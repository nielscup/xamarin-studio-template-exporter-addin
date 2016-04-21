using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using System.IO;
using System.Linq;

namespace TemplateExporter
{
	class FinalizeTemplateHandler:CommandHandler
	{
		Solution solution;

		protected override void Run ()
		{
			var progressDialog = new MonoDevelop.Ide.Gui.Dialogs.ProgressDialog(false, true);
			progressDialog.Title = "Finalize Template";
			progressDialog.Show();

			try {		
				var solutionFolder = solution.BaseDirectory.ToString(); //Path.GetDirectoryName (Environment.GetCommandLineArgs()[0]); 

				if(!UpdateSolutionFile (solutionFolder))
					return;				

				UpdateCSProjFiles (solutionFolder);
				solution.NeedsReload = true;

				//DeleteTemplateFolder();
				Console.WriteLine ("Done.");

				// Display Success message:
				progressDialog.ShowDone(false, false);
				progressDialog.WriteText("Template successfully finalized. Restart Xamarin Studio for changes to take effect.");
				progressDialog.Message = "Finalize Template success. RESTART XAMARIN STUDIO.";

			} catch (Exception ex) {
				Console.WriteLine ("ERROR: {0}", ex.Message);

				// Display Error message:
				progressDialog.ShowDone(false, true);
				progressDialog.WriteText (ex.Message);
			}
		}

		/// <summary>
		/// Update the "Template Finalizer" menu item based on if the solution is selected.
		/// </summary>
		/// <param name="info">Info.</param>
		protected override void Update (CommandInfo info)
		{					
			if (IdeApp.ProjectOperations.CurrentSelectedItem is MonoDevelop.Projects.Solution) {
				solution = (MonoDevelop.Projects.Solution)IdeApp.ProjectOperations.CurrentSelectedItem;
				var slnTemplateFile = GetSolutionTemplateFile (solution.BaseDirectory.ToString());
				info.Enabled = !string.IsNullOrEmpty(slnTemplateFile);
			}
			else
			{
				info.Enabled = false;
			}
		}

		/// <summary>
		/// Updates the solution file.
		/// </summary>
		/// <returns><c>true</c>, if solution file was updated, <c>false</c> otherwise.</returns>
		/// <param name="solutionFolder">Solution folder.</param>
		private bool UpdateSolutionFile(string solutionFolder)
		{			
			var slnFile =  GetSlnFilePath(solutionFolder);

			if(!FileExists(slnFile))
				return false;

			var slnTemplatePath = GetSolutionTemplateFile (solutionFolder);

			if(string.IsNullOrEmpty(slnTemplatePath))
			{
				Console.WriteLine("ERROR: solution TEMPLATE file not found.");
				return false;
			}

			Console.WriteLine ("Updating solution file {0}...", slnFile);

			var slnFileContent = File.ReadAllText(slnTemplatePath);
			slnFileContent = slnFileContent.Replace("[SOLUTIONNAME]", solution.Name);
			File.WriteAllText (slnFile, slnFileContent);

			Console.WriteLine ("Solution file updated.");

			return true;
		}

		/// <summary>
		/// Gets the sln file path.
		/// </summary>
		/// <returns>The sln file path.</returns>
		/// <param name="solutionFolder">Solution folder.</param>
		private string GetSlnFilePath(string solutionFolder)
		{
			var slnFile = Directory.GetFiles (solutionFolder).FirstOrDefault (file => Path.GetExtension (file).ToLower () == ".sln");
			return slnFile;
		}

		/// <summary>
		/// Gets the solution template file.
		/// </summary>
		/// <returns>The solution template file.</returns>
		/// <param name="solutionFolder">Solution folder.</param>
		private string GetSolutionTemplateFile(string solutionFolder)
		{
			foreach (var directory in Directory.GetDirectories(solutionFolder)) {				

				var slnFile = Directory.GetFiles (directory).FirstOrDefault (file => Path.GetFileName(file).ToLower () == "template.sln.txt");

				if (slnFile == null)
					continue;
				else
					return slnFile;

			}

			return "";
		}

		/// <summary>
		/// Updates the CS proj files.
		/// </summary>
		/// <param name="solutionFolder">Solution folder.</param>
		private void UpdateCSProjFiles(string solutionFolder)
		{
			// enumerate directories and update all .csprojfiles with content from TEMPLATE.csproj.txt
			foreach (var directory in Directory.GetDirectories(solutionFolder)) {				

				var csprojFile = Directory.GetFiles (directory).FirstOrDefault (file => Path.GetExtension (file).ToLower () == ".csproj");

				if (csprojFile == null)
					continue;

				UpdateCSProjFile (csprojFile);
			}
		}

		/// <summary>
		/// Updates the CS proj file.
		/// </summary>
		/// <param name="csprojFile">Csproj file.</param>
		private void UpdateCSProjFile(string csprojFile)
		{
			if (!FileExists (csprojFile)) 
				return;			

			var csprojName = Path.GetFileName (csprojFile);
			var csprojTemplatePath = csprojFile.Replace (csprojName, "TEMPLATE.csproj.txt");

			if (!FileExists (csprojTemplatePath))
				return;			

			var projectName = Path.GetFileNameWithoutExtension(csprojFile);

			var csprojFileContent = File.ReadAllText(csprojTemplatePath);
			csprojFileContent = csprojFileContent.Replace("[PROJECTNAME]", projectName);
			csprojFileContent = csprojFileContent.Replace("${SolutionName}", solution.Name);

			Console.WriteLine ("Updating csproj file {0}...", csprojFile);
			File.WriteAllText (csprojFile, csprojFileContent);

			Console.WriteLine ("csproj file updated.");
		}

		private bool FileExists(string filePath)
		{
			if (!File.Exists (filePath)) {
				Console.WriteLine ("path does not exist: " + filePath);
				return false;
			}

			return true;
		}

		private void DeleteTemplateFolder()
		{				
			var templateFolder = solution.GetAllSolutionItems<SolutionFolder>().FirstOrDefault (x => x.Name == "Template");
			if (templateFolder != null) {
				IdeApp.ProjectOperations.RemoveSolutionItem (templateFolder);
				solution.Save (new MonoDevelop.Core.ProgressMonitoring.NullProgressMonitor());
			}
		}
	}
}

