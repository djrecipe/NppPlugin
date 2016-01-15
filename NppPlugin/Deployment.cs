using System.Runtime.InteropServices;
using NppPlugin.DllExport;
using System.IO;
using Microsoft.Win32;
using System;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NppPlugin
{
    /// <summary>
    /// Exposes methods to external callers (i.e. installer) to assist in the installation/deployment process.
    /// </summary>
    public abstract class Deployment
    {
        /// <summary>
        /// File name where Notepad++ stores custom user languages.
        /// </summary>
        private const string FILE_USERLANGS = @"userDefineLang.xml";
        /// <summary>
        /// Registry path where Notepad++ installation location is stored.
        /// </summary>
        private const string KEY_NPPINSTALL = @"CLSID\{B298D29A-A6ED-11DE-BA8C-A68E55D89593}\Settings";
        /// <summary>
        /// Name attributes of existing XML nodes which will be replaced. 
        /// </summary>
        private static readonly List<string> defaultAttributes = new List<string>() { "Logs-Light", "Logs-Dark" };
        /// <summary>
        /// Exposed method which returns Notepad++ install path, as gathered from Windows registry.
        /// </summary>
        /// <param name="strout">Install path</param>
        /// <returns>Integer: 0 if success.</returns>
        [DllExport("GetNppInstallPath", CallingConvention = CallingConvention.StdCall)]
        public static int GetNppInstallPath([MarshalAs(UnmanagedType.BStr)] out string strout)
        {
            strout = Deployment.GetNotepadInstallPathFromRegistry();
            return strout == null ? 1 : 0;
        }
        /// <summary>
        /// Exposed method which attempts to inject XML into a Notepad++ config file in order to add a few
        /// user-defined languages to Notepad++.
        /// </summary>
        /// <returns>Integer: 0 if success.</returns>
        [DllExport("InstallCustomNppLanguages", CallingConvention = CallingConvention.StdCall)]
        public static int InstallCustomNppLanguages()
        {
            //
            string appdata_directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"\Notepad++\");
            string program_directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"\Notepad++\");
            string appdata_path = Path.Combine(appdata_directory, Deployment.FILE_USERLANGS);
            string program_path = Path.Combine(program_directory, Deployment.FILE_USERLANGS);
            //
            try
            {
                if (Directory.Exists(appdata_directory) && File.Exists(appdata_path))
                    Deployment.ModifyCustomLanguagesFile(appdata_path);
            }
            catch (Exception e)
            {
                // TODO: 12/02/15 implement exception handling
            }
            try
            {
                if (Directory.Exists(program_directory) && File.Exists(program_path))
                    Deployment.ModifyCustomLanguagesFile(program_path);
            }
            catch (Exception e)
            {
                // TODO: 12/02/15 implement exception handling
            }
            return 0;
        }
        /// <summary>
        /// Retrieves Notepad++ install path from Windows registry.
        /// </summary>
        /// <returns>String representing Notepad++ install directory.</returns>
        private static string GetNotepadInstallPathFromRegistry()
        {
            RegistryKey base_key = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64);
            RegistryKey key = base_key?.OpenSubKey(Deployment.KEY_NPPINSTALL, false);
            if (key == null)
            {
                base_key = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry32);
                key = base_key?.OpenSubKey(Deployment.KEY_NPPINSTALL, false);
            }
            string temp_path = key?.GetValue("Path", null) as string;
            base_key?.Close();
            key?.Close();
            return temp_path != null && File.Exists(temp_path) ? Path.GetDirectoryName(temp_path) : null;
        }
        /// <summary>
        /// Retrieves contents of a resource embedded within this assembly.
        /// </summary>
        /// <param name="filename">The name of the file/resource to retrieve.</param>
        /// <returns>The contents of the retrieved file, or null.</returns>
        private static string GetResourceTextFile(string filename)
        {
            string result = string.Empty;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NppPlugin." + filename))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }
        /// <summary>
        /// Attempts to modify a Notepad++ configuration file using XML embedded within this assembly.
        /// </summary>
        /// <param name="path">The path to the Notepad++ configuration file.</param>
        private static void ModifyCustomLanguagesFile(string path)
        {
            XElement doc = XElement.Load(path);
            //
            try
            {
                IEnumerable<XElement> elements = doc.Elements().Where(e =>
                    Deployment.defaultAttributes.Contains(e.Attribute("name").Value)).Reverse();
                foreach (XElement element in elements)
                    element.Remove();
            }
            catch (Exception e)
            {
                // TODO: 12/02/15 implement exception handling
            }
            //
            try
            {
                XElement logs_dark = XElement.Parse(Deployment.GetResourceTextFile("Logs-Dark.xml"));
                doc.Add(logs_dark.Element("UserLang"));
            }
            catch (Exception e)
            {
                // TODO: 12/02/15 implement exception handling
            }
            //
            try
            {
                XElement logs_light = XElement.Parse(Deployment.GetResourceTextFile("Logs-Light.xml"));
                doc.Add(logs_light.Element("UserLang"));
            }
            catch (Exception e)
            {
                // TODO: 12/02/15 implement exception handling
            }
            //
            UTF8Encoding encoder = new System.Text.UTF8Encoding(false);
            using (StreamWriter stream = new StreamWriter(path, false, encoder))
            {
                stream.Write(doc.ToString());
            }
            return;
        }
    }
}
