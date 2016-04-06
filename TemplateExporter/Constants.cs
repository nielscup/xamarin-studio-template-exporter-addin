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
			"<Open filename=\"MainActivity.cs\"/>\n\t" +
			"</Actions>\n\t\n\t" +
			"<Combine name=\"${ProjectName}\" directory=\".\">\n\t\t" +
			"<Options>\n\t\t\t" +
			"<StartupProject>${ProjectName}</StartupProject>\n\t\t" +
			"</Options>\n\t\t\n\t\t" +
			"<Project name=\"${ProjectName}\" directory=\".\" type=\"MonoDroid\">\n\t\t\t" +
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
		
	}
}

