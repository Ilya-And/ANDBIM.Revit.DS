using System;
using System.IO;
using System.Windows.Forms;
using WixSharp;
using WixSharp.UI.WPF;

namespace ANDBIM.Installer
{
    public class Program
    {
        private static string projectName = "ANDBIM DS";
        private static string version = "1.0.0"; // don't forget to change
        static void Main()
        {
            var path = FindFolder();

            var project = new ManagedProject();//"MyProduct",
                              //new Dir(@"%ProgramFiles%\My Company\My Product",
                                  //new WixSharp.File("Program.cs"));
            project.Name = projectName;

            project.GUID = new Guid("6493d1d8-ee55-4a60-a124-ee9c825c886f");

            // project.ManagedUI = ManagedUI.DefaultWpf; // all stock UI dialogs

            //custom set of UI WPF dialogs
            project.ManagedUI = new ManagedUI();

            project.ManagedUI.InstallDialogs.Add<ANDBIM.Installer.WelcomeDialog>()
                                            //.Add<ANDBIM.Installer.LicenceDialog>()
                                            //.Add<ANDBIM.Installer.FeaturesDialog>()
                                            //.Add<ANDBIM.Installer.InstallDirDialog>()
                                            .Add<ANDBIM.Installer.ProgressDialog>()
                                            .Add<ANDBIM.Installer.ExitDialog>();

            project.ManagedUI.ModifyDialogs
                                           //.Add<ANDBIM.Installer.MaintenanceTypeDialog>()
                                           //.Add<ANDBIM.Installer.FeaturesDialog>()
                                           .Add<ANDBIM.Installer.WelcomeDialog>()
                                           .Add<ANDBIM.Installer.ProgressDialog>()
                                           .Add<ANDBIM.Installer.ExitDialog>();

            project.OutDir = "output";
            project.MajorUpgrade = MajorUpgrade.Default;

            project.ControlPanelInfo.Manufacturer = "Andreev Ilya (ANDBIM)";
            project.Dirs = new Dir[]
                {
                    new InstallDir($@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Autodesk\Revit\Addins\2021",
                        new WixSharp.File($@"{path}\ANDBIM.Revit.DS.addin\ANDBIM.Revit.DS.addin"),
                        new Dir(@"ANDBIM.Revit.DS",
                        new DirFiles($@"{path}\ANDBIM.Revit.DS\bin\R2021\*.*"))),
                    new InstallDir($@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Autodesk\Revit\Addins\2022",
                        new WixSharp.File($@"{path}\ANDBIM.Revit.DS.addin\ANDBIM.Revit.DS.addin"),
                        new Dir(@"ANDBIM.Revit.DS",
                        new DirFiles($@"{path}\ANDBIM.Revit.DS\bin\R2022\*.*"))),
                    new InstallDir($@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Autodesk\Revit\Addins\2023",
                        new WixSharp.File($@"{path}\ANDBIM.Revit.DS.addin\ANDBIM.Revit.DS.addin"),
                        new Dir(@"ANDBIM.Revit.DS",
                        new DirFiles($@"{path}\ANDBIM.Revit.DS\bin\R2023\*.*"))),
                    new InstallDir($@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Autodesk\Revit\Addins\2024",
                        new WixSharp.File($@"{path}\ANDBIM.Revit.DS.addin\ANDBIM.Revit.DS.addin"),
                        new Dir(@"ANDBIM.Revit.DS",
                        new DirFiles($@"{path}\ANDBIM.Revit.DS\bin\R2024\*.*")))
                };
            
            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";

            project.BuildMsi();
        }
        private static string FindFolder()
        {
            string path = System.Reflection.Assembly.GetEntryAssembly().Location;
            for (int i = 0; i < 4; i++)
            {
                path = Directory.GetParent(path)?.FullName;
            }
            return path;
        }
    }
}