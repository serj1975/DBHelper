using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Configuration;

namespace Base.Config
{
    public class WebConfigManager
    {
        //static Configuration configuration = WebConfigurationManager.OpenWebConfiguration("~");
        //public static ConfigurationSection GetSection(string sectionName)
        //{

        //    ConfigurationSection section = configuration.GetSection(sectionName);
        //    return section;
        //}


        public static bool AddSection(string sectionName)
        {
            System.Configuration.Configuration config =
            ConfigurationManager.OpenExeConfiguration(
            ConfigurationUserLevel.None);
            //            // If the section does not exist in the configuration
            //// file, create it and save it to the file.
            //if (config.Sections[sectionName] == null)
            //{
            //    ConfigurationSection custSecti
            //    config.Sections.Add(customSectionName, custSection);
            //    custSection =
            //        config.GetSection(customSectionName) as CustomSection;
            //custSection.SectionInformation.ForceSave = true;
            config.Save(ConfigurationSaveMode.Full);
            return true;

        }


        public static AuthenticationMode GetAuthMode()
        {
            AuthenticationSection section = (AuthenticationSection)WebConfigurationManager.GetSection("system.web/authentication");
            return section.Mode;

            //string user = System.Web.HttpContext.Current.Request.ServerVariables["LOGON_USER"];
            //string mode = "";
            //if(System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            //    mode = System.Web.HttpContext.Current.User.Identity.AuthenticationType;
            //if (mode == "Forms")
            //    return AuthenticationMode.Forms;
            //if (mode == "Windows")
            //    return AuthenticationMode.Windows;

            //return AuthenticationMode.None;
        
            //AuthenticationSection section = GetSection("system.web/authentication") as AuthenticationSection;
            //return section.Mode;
        }

        public static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static bool DebugMode
        {
            get
            {
                string Debug = "";
                if (WebConfigurationManager.AppSettings["DebugMode"] != null)
                    Debug = WebConfigurationManager.AppSettings["DebugMode"];
                if (Debug.ToLower() == "true") return true;
                else return false;
            }

        }       
    }
}
