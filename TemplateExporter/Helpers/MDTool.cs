using System;
using System.Diagnostics;

namespace TemplateExporter
{
	public static class MDTool
	{
		/// <summary>
		/// Runs the MD tool.
		/// </summary>
		/// <returns><c>true</c>, if MD tool was run, <c>false</c> otherwise.</returns>
		/// <param name="workingDir">Root dir.</param>
		/// <param name="arguments">Arguments.</param>
		public static bool Run(string workingDir, string arguments)
		{
			Logging.Log(string.Format("Running mdtool: {0}", arguments));
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
			var error = process.StandardError.ReadToEnd();
			var output = process.StandardError.ReadToEnd();

			process.WaitForExit();

			if (process.ExitCode != 0)
			{
				Logging.Log(string.Format("mdtool EXCEPTION: exitCode: {0}", process.ExitCode));

				if (process.ExitCode == 255)
				{
					// In case of a 255 error: Check if the paths in the Runtime node in ProjectTemplate/XamarinPlugin.addin.xml are correct. These should be relative to the XamarinPlugin.addin.xml file.
					Logging.Log("Possible solution: Check if the paths in the Runtime node in ProjectTemplate/XamarinPlugin.addin.xml are correct. These should be relative to the XamarinPlugin.addin.xml file. If not, log a bug at https://github.com/nielscup/xamarin-studio-template-exporter-addin");
				}

				return false;
			}

			return true;
		}
	}
}

