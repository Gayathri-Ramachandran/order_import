using System.Configuration;


namespace Agr.Int.OrderImport.Common.Configurations
{
    //TODO: This is going to die off as part of SFTP Component.
    public class SftpLocationConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Gets or sets the SFTP folder location.
        /// </summary>
        /// <value>The folder.</value>
        [ConfigurationProperty("FolderPath", IsRequired = true)]
        public string Folder
        {
            get { return this["FolderPath"] as string; }
            set { this["FolderPath"] = value; }
        }
    }
}
