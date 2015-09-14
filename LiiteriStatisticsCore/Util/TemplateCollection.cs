using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace LiiteriStatisticsCore.Util
{
    public class TemplateCollection
    {
        private IDictionary<string, string> Templates;

        private string GetTemplateDirectory(string category)
        {
            object dataDirectory =
                AppDomain.CurrentDomain.GetData("DataDirectory");

            // Check BaseDirectory in case we are running UnitTests
            // BaseDirectory is probably bin\Debug\
            string baseDirectory =
                AppDomain.CurrentDomain.BaseDirectory;

            if (dataDirectory == null) {
                dataDirectory = baseDirectory;
            } else {
                dataDirectory = dataDirectory.ToString();
            }
            if (dataDirectory == null) {
                throw new DirectoryNotFoundException(
                    "Unable to figure out Data Directory");
            }
            dataDirectory = Path.Combine(
                (string) dataDirectory, "templates", category);

            return (string) dataDirectory;
        }

        public TemplateCollection(string category)
        {
            string directory = this.GetTemplateDirectory(category);
            if (!Directory.Exists(directory)) {
                throw new DirectoryNotFoundException(
                    "Template directory not found: " + directory);
            }

            this.Templates = new Dictionary<string, string>();

            foreach (string fileName in Directory.GetFiles(directory)) {
                if (!fileName.EndsWith(".tmpl")) {
                    continue;
                }
                string filePath = Path.Combine(
                    directory, Path.GetFileName(fileName));
                string templateName = Path.GetFileNameWithoutExtension(fileName);
                using (StreamReader r = new StreamReader(filePath)) {
                    this.Templates[templateName.ToLower()] = r.ReadToEnd();
                }
            }
        }

        public string Get(string key)
        {
            return this.Templates[key.ToLower()];
        }
    }
}