using System;
using System.Diagnostics;

namespace MpackCreator
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try {
				RunMDTool ("/Users/nielscup/Documents/Projects/TemplateExporter/TemplateExporter/bin/debug",
					"-v setup pack TemplateExporter.dll");
			} catch (Exception ex) {
				Console.WriteLine("ERROR: " + ex.Message);
			}
		}

		/// <summary>
		/// Runs the MD tool.
		/// </summary>
		/// <returns><c>true</c>, if MD tool was run, <c>false</c> otherwise.</returns>
		/// <param name="rootDir">Root dir.</param>
		/// <param name="arguments">Arguments.</param>
		private static bool RunMDTool(string rootDir, string arguments)
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
				Console.WriteLine ("mdtool EXCEPTION: exitCode: {0}", process.ExitCode);
				return false;
			} else {
				Console.WriteLine ("Successfully created mpack: {0}", rootDir);
			}

			return true;
		}
	}
}
