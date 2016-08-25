﻿using System;

namespace TemplateExporter
{
	/// <summary>
	/// The xml in a cs class. Unable to add xml files to an add-in. 
	/// Copy paste the xml from the xml files to these constants to make it available in the Add-in
	/// </summary>
	public static class Xml
	{
		// DO NOT DIRECTLY EDIT XML IN THIS CLASS, instead edit the xml in the corresponding .xml files for readability and copy it here...
		// TODO: Handle this in a build task

		public const string UnitTestXml = "\n\t\t<Project name=\"${ProjectName}[PROJECTNAMEEXTENSION]\" directory=\"./${ProjectName}[PROJECTNAMEEXTENSION]\" type=\"[PROJECTTYPE]\">\t\t\t\n\t\t\t<Options \n\t\t\t\tTarget=\"Library\"    \t\t\t\n    \t\t\tTargetFrameworkVersion = \"[TARGETFRAMEWORK]\" />\n\t\t\t<References>\n\t\t\t\t[REFERENCES]\n\t\t\t</References>\n\t\t\t<Packages>\n\t\t\t\t[PACKAGES]\n\t\t\t</Packages>\n\t\t\t<Files>\n\t\t\t\t[FILES]\n\t\t\t</Files>\n\t\t</Project>\n";
		public const string AddinXml = "<Addin name=\"My Company Template\"\n\t\tid=\"[PROJECTNAME]\"\n\t\tnamespace=\"MonoDevelop\"\n\t\tauthor=\"Niels Cup\"\n\t\tcopyright=\"MIT\"\n\t\turl=\"https://github.com/nielscup\"\n\t\tdescription=\"Creates a My Company Project\"\n\t\tcategory=\"IDE extensions\"\n\t\tversion=\"0.1\">\n\t<Runtime>\n\t\t[RUNTIME]\n\t</Runtime>\n\t<Dependencies>\n\t\t<Addin id=\"Core\" version=\"6.0\"/>\n\t\t<Addin id=\"Ide\" version=\"6.0\"/>\n\t</Dependencies>\n\t\n\t<Extension path=\"/MonoDevelop/Ide/ProjectTemplates\">\n\t\t<ProjectTemplate id=\"MyCompanyTemplate-Project\" file=\"ProjectTemplate.xpt.xml\" />\n\t</Extension>\n\n\t<!---<Extension path=\"/MonoDevelop/Ide/ProjectTemplatePackageRepositories\">\n\t\t<PackageRepository url=\"https://cupitcontent.blob.core.windows.net/nuget\"/>\n\t</Extension>-->\n\n\t<!--Custom Package repository, path is relative to template installation dir (/Users/YOURNAME/Library/Application Support/XamarinStudio-6.0/LocalInstall/Addins/MyTemplate/...)-->\n\t<Extension path=\"/MonoDevelop/Ide/ProjectTemplatePackageRepositories\">\n\t\t<PackageRepository path=\"../../../../../Documents/nuget_packages\"/>\n\t\t<PackageRepository path=\"../../../../../../Documents/nuget_packages\"/>\n\t\t<PackageRepository path=\"../../../../../../../Documents/nuget_packages\"/>\n\t</Extension>\n\n\t<Extension path=\"/MonoDevelop/Ide/ProjectTemplateCategories\">\n\t\t<Category id=\"MyCompany\" name=\"MyCompany\" icon=\"md-platform-cross-platform\" insertafter=\"other\">\n\t\t\t<Category id=\"android\" name=\"Android\">\n\t\t\t\t<Category id=\"general\" name=\"General\" />\n\t\t\t</Category>\n\t\t</Category>\n\t</Extension>\n</Addin>";
		public const string XptXml = "<?xml version=\"1.0\"?>\n<Template>\n\t<TemplateConfiguration>\n\t\t<_Name>[TEMPLATENAME] [VERSION]</_Name>\n\t\t<Category>[CATEGORY]</Category>\n\t\t<Icon>md-platform-cross-platform</Icon>\n\t\t<LanguageName>C#</LanguageName>\n\t\t<_Description>[TEMPLATEDESCRIPTION]</_Description>\n\t</TemplateConfiguration>\n\t<!--<Actions>\n\t\t<Open filename=\"YOUR_STARTUP_FILE.cs\"/>\n\t</Actions>-->\n\t<Combine name=\"${ProjectName}\" directory=\".\">\n\t\t<!---<Options>\n\t\t\t<StartupProject>${ProjectName}</StartupProject>\n\t\t</Options>-->\n[PROJECTS]\n\t</Combine>\n</Template>";
		public const string AndroidXml = "\t\t<Project name=\"${ProjectName}[PROJECTNAMEEXTENSION]\" directory=\"./${ProjectName}[PROJECTNAMEEXTENSION]\" type=\"[PROJECTTYPE]\">\n\t\t\t<Options\n\t\t\t\tAndroidApplication=\"true\"\n\t\t\t\tAndroidResgenFile=\"Resources/Resource.designer.cs\"\n\t\t\t\tAndroidResgenClass=\"Resource\" />\n\t\t\t<References>\t\t\t\t\n\t\t\t\t[REFERENCES]\n\t\t\t</References>\n\t\t\t<Packages>\n\t\t\t\t[PACKAGES]\n\t\t\t</Packages>\n\t\t\t<Files>\n\t\t\t\t[FILES]\n\t\t\t</Files>\n\t\t</Project>\n";
		public const string iOSXml = "\t\t<Project name=\"${ProjectName}[PROJECTNAMEEXTENSION]\" directory=\"./${ProjectName}[PROJECTNAMEEXTENSION]\" type=\"[PROJECTTYPE]\">\n\t\t\t<Options />\n\t\t\t<References>\t\t\t\t\n\t\t\t\t[REFERENCES]\n\t\t\t</References>\n\t\t\t<Packages>\n\t\t\t\t[PACKAGES]\n\t\t\t</Packages>\n\t\t\t<Files>\n\t\t\t\t[FILES]\n\t\t\t</Files>\n\t\t</Project>\n";
		public const string PclXml = "\t\t<Project name=\"${ProjectName}[PROJECTNAMEEXTENSION]\" directory=\"./${ProjectName}[PROJECTNAMEEXTENSION]\" type=\"[PROJECTTYPE]\">\t\t\t\n\t\t\t<Options \n\t\t\t\tTarget=\"Library\"    \t\t\t\n\t\t\t\tTargetFrameworkVersion = \"[TARGETFRAMEWORK]\" />\n\t\t\t<References>\n\t\t\t\t[REFERENCES]\n\t\t\t</References>\n\t\t\t<Packages>\n\t\t\t\t[PACKAGES]\n\t\t\t</Packages>\n\t\t\t<Files>\n\t\t\t\t[FILES]\n\t\t\t</Files>\n\t\t</Project>\n";
	}
}