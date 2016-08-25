using System;
namespace TemplateExporter
{
	public static class Logging
	{
		public static MonoDevelop.Ide.Gui.Dialogs.ProgressDialog ProgressDialog;

		public static void Log(string message)
		{
			try
			{
				ProgressDialog.WriteText(message + Environment.NewLine);
				Console.WriteLine(message);
			}
			catch (Exception ex)
			{
				ProgressDialog.WriteText("LOG ERROR: {0}" + ex.Message);
			}
		}
	}
}

