using System;

namespace TemplateExporter
{
	public static class Constants
	{
		public const string AddInXmlDroid = 
			"<Addin " +
			"name=\"Empty MyCompany Android App\"\n\t\t" +
			"id=\"[PROJECTNAME]\"\n\t\t" +
			"namespace=\"MonoDevelop\"\n\t\t" +
			"author=\"Niels Cup\"\n\t\t" +
			"copyright=\"MIT\"\n\t\t" +
			"url=\"https://github.com/nielscup\"\n\t\t" +
			"description=\"Empty MyCompany Android App\"\n\t\t" +
			"category=\"IDE extensions\"\n\t\t" +
			"version=\"0.1\">\n\t\t" +
			"[RUNTIME_PLACEHOLDER]\n\t" +
			"<Dependencies>\n\t\t" +
			"<Addin id=\"Core\" version=\"5.0\"/>\n\t\t" +
			"<Addin id=\"Ide\" version=\"5.0\"/>\n\t</Dependencies>\n\t\n\t" +
			"<Extension path=\"/MonoDevelop/Ide/ProjectTemplates\">\n\t\t" +
			"<ProjectTemplate id=\"MyCompanyTemplateDroid-Project\" file=\"ProjectTemplate.xpt.xml\" />\n\t" +
			"</Extension>\n\n\t" +
			"<!---<Extension path=\"/MonoDevelop/Ide/ProjectTemplatePackageRepositories\">\n\t\t" +
			"<PackageRepository url=\"https://cupitcontent.blob.core.windows.net/nuget\"/>\n\t" +
			"</Extension>-->\n\n\t" +
			"<Extension path=\"/MonoDevelop/Ide/ProjectTemplatePackageRepositories\">\n\t\t" +
			"<PackageRepository path=\"../../../../../../Documents/nuget_packages\"/>\n\t" +
			"</Extension>\n\n\t" +
			"<Extension path=\"/MonoDevelop/Ide/ProjectTemplateCategories\">\n\t\t" +
			"<Category id=\"MyCompany\" name=\"MyCompany\" icon=\"md-platform-cross-platform\" insertafter=\"other\">\n\t\t\t" +
			"<Category id=\"android\" name=\"Android\">\n\t\t\t\t" +
			"<Category id=\"general\" name=\"General\" />\n\t\t\t" +
			"</Category>\n\t\t" +
			"</Category>\n\t" +
			"</Extension>\n" +
			"</Addin>";
		//"\t\n\t<Runtime>\n\t\t<Import file=\"ProjectTemplate.xpt.xml\" />\n\t\t<Import file = \"Assets/AboutAssets.txt\" />\n\t\t<Import file = \"Resources/AboutResources.txt\" />\n\t\t<Import file = \"MainActivity.cs\" />\n\t\t<Import file = \"Properties/AssemblyInfo.cs\" />\n\t\t<Import file = \"Resources/drawable/Icon.png\" />\t\t\n\t\t<Import file = \"Resources/layout/Main.axml\" />\n\t\t<Import file = \"Resources/Resource.Designer.cs\" />\n\t\t<Import file = \"Resources/values/Strings.xml\" />\n\t</Runtime>\n"

		public const string XptXmlAndroid = 
			"<?xml version=\"1.0\"?>\n" +
			"<Template>\n\t" +
			"<TemplateConfiguration>\n\t\t" +
			"<_Name>Empty MyCompany Android App [VERSION]</_Name>\n\t\t" +
			"<_Category>Empty MyCompany Android Project Template</_Category>\n\t\t" +
			"<Category>MyCompany/android/general</Category>\n\t\t" +
			"<Icon>monodroid-project</Icon>\n\t\t" +
			"<LanguageName>C#</LanguageName>\n\t\t" +
			"<_Description>Creates an empty MyCompany Android Application.</_Description>\n\t" +
			"</TemplateConfiguration>\n\t\n\t" +
			"<Actions>\n\t\t" +
			"<Open filename = \"MainActivity.cs\"/>\n\t" +
			"</Actions>\n\t\n\t" +
			"<Combine name = \"${ProjectName}\" directory = \".\">\n\t\t" +
			"<Options>\n\t\t\t" +
			"<StartupProject>${ProjectName}</StartupProject>\n\t\t" +
			"</Options>\n\t\t\n\t\t" +
			"<Project name = \"${ProjectName}\" directory = \".\" type = \"MonoDroid\">\n\t\t\t" +
			"<Options\n\t\t\t\tAndroidApplication=\"true\"\n\t\t\t\tAndroidResgenFile=\"Resources/Resource.designer.cs\"\n\t\t\t\tAndroidResgenClass=\"Resource\" />\n\t\t\t" +
			"<References>\n\t\t\t\t" +
			"<Reference type=\"Gac\" refto=\"System, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\" />\n\t\t\t\t" +
			"<Reference type=\"Gac\" refto=\"System.Xml, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\" />\n\t\t\t\t" +
			"<Reference type=\"Gac\" refto=\"System.Core, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\" />\n\t\t\t\t" +
			"<Reference type=\"Gac\" refto=\"Mono.Android\" />\n\t\t\t" +
			"</References>\n\t\t\t" +
			"<Packages>" +
			"[PACKAGES_PLACEHOLDER]" +
			"</Packages>\n\n\t\t" +
			"[FILES_PLACEHOLDER]\n\n\t\t" +
			"</Project>\n\t" +
			"</Combine>\n" +
			"</Template>";
		//"\n\t\t\t<Files>\n\t\t\t\t<File name=\"MainActivity.cs\" src=\"[PROJECTNAME]/MainActivity.cs\" />\n\t\t\t\t<Directory name=\"Resources\">\n\t\t\t\t\t<File name=\"Resource.Designer.cs\" src=\"Resources/Resource.Designer.cs\" />\n\t\t\t\t\t<RawFile name=\"AboutResources.txt\" src=\"Resources/AboutResources.txt\" />\n\t\t\t\t\t<Directory name=\"layout\">\n\t\t\t\t\t\t<File name=\"Main.axml\" src=\"Resources/layout/Main.axml\" />\n\t\t\t\t\t</Directory>\n\t\t\t\t\t<Directory name=\"values\">\n\t\t\t\t\t\t<File name=\"Strings.xml\" src=\"Resources/values/Strings.xml\"/>\n\t\t\t\t\t</Directory>\n\t\t\t\t\t<Directory name =\"drawable\">\n\t\t\t\t\t\t<RawFile name=\"Icon.png\" src=\"Resources/drawable/Icon.png\" />\n\t\t\t\t\t</Directory>\n\t\t\t\t</Directory>\n\t\t\t\t<Directory name=\"Properties\">\n\t\t\t\t\t<File name=\"AssemblyInfo.cs\" src=\"Properties/AssemblyInfo.cs\" />\n\t\t\t\t</Directory>\n\t\t\t\t<Directory name=\"Assets\">\n\t\t\t\t\t<RawFile name=\"AboutAssets.txt\" src=\"Assets/AboutAssets.txt\" BuildAction=\"None\" />\n\t\t\t\t</Directory>\n\t\t\t</Files>"

	}
}

