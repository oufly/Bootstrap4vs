using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SnippetsListing
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var slnDir = TryGetSolutionDirectoryInfo().FullName;
                if (slnDir == null) throw new DirectoryNotFoundException(".sln directory not found");

                var snippetsRootDir = Path.Combine(slnDir, "Bootstrap4vs", "Snippets");
                var componentsFilepath = Path.Combine(snippetsRootDir, "bootstrap-components.xml");
                var snippetsDir = Path.Combine(snippetsRootDir, "Bootstrap4vs");

                Console.WriteLine($"********** Processing Files **********");

                var components = BootstrapComponent.GetComponentFromFile(componentsFilepath);

                int snippetsCount = 0;
                foreach (var component in components)
                {
                    Console.WriteLine($"---------- {component.Name} ----------");
                    var componentDir = Path.Combine(snippetsDir, component.Directory);
                    foreach (string snippetFilepath in Directory.EnumerateFiles(componentDir, "*.snippet", SearchOption.AllDirectories))
                    {
                        Console.WriteLine($"{Path.GetFileName(snippetFilepath)}");
                        component.Snippets.Add(Snippet.GetSnippetFromFile(snippetFilepath));
                        snippetsCount++;
                    }
                }
                Console.WriteLine($"\n{snippetsCount} snippets");

                Console.WriteLine("\n********** Extarting html **********");
                var pageHeading = "## Bootstrap4vs snippets list\n\n";
                var html = pageHeading + SnippetsListToHtmlTable(components);

                Console.WriteLine("********** Writing html to file **********");
                var path = $"{slnDir}\\SnippetsListing.md";
                File.WriteAllText(path, html);

                Console.WriteLine("\n\n********** Done **********");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                Console.ReadKey();
            }


        }
        public static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
        {
            var directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory;
        }
       
       
        public static string SnippetsListToHtmlTable(List<BootstrapComponent> components)
        {
            string newLine = Environment.NewLine;
            string table = "<table>" + newLine;
            table += "<tr><th>Component</th><th>Snippet</th><th>Shortcut</th><th>Description</th></tr>" + newLine;
            foreach (var c in components)
            {
                int i = 0;
                foreach (var s in c.Snippets)
                {
                    table += "<tr>";
                    if (i == 0)
                        table += $"<td rowspan=\"{c.Snippets.Count}\"><a href=\"{c.Url}\">{c.Name}</a></td>";

                    table += $"<td align=\"center\">{s.Title}</td>";
                    table += $"<td nowrap align=\"center\"><code>{s.Shortcut}</code></td>";
                    table += $"<td><i>{s.Description}</i></td></tr>" + newLine;
                    i++;
                }
            }
            table += "</table>";
            return table;

        }




    }
    public class Snippet
    {
        public string Title { get; set; }
        public string Shortcut { get; set; }
        public string Description { get; set; }
        public static Snippet GetSnippetFromFile(string snippetFilepath)
        {
            var snippet = new Snippet();
            using (XmlReader reader = XmlReader.Create(snippetFilepath))
            {

                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name.ToString())
                        {
                            case "Title":
                                snippet.Title = reader.ReadString();
                                break;
                            case "Shortcut":
                                snippet.Shortcut = reader.ReadString();
                                break;
                            case "Description":
                                snippet.Description = reader.ReadString();
                                break;
                        }
                    }
                }
            }
            return snippet;
        }
    }
    public class BootstrapComponent
    {
        public string Name { get; set; }
        public string Directory { get; set; }
        public string Url { get; set; }
        public int SnippetsCount { get; set; }
        public List<Snippet> Snippets { get; set; } = new List<Snippet>();
        
        public static List<BootstrapComponent> GetComponentFromFile(string componentsFilepath)
        {
            var componentsDoc = XElement.Load(componentsFilepath);
            return componentsDoc.Elements("Component")
                 .Select(c => new BootstrapComponent
                 {
                     Name = (string)c.Attribute("Name"),
                     Directory = (string)c.Attribute("Directory"),
                     Url = (string)c.Attribute("Url")
                 })
                 .ToList<BootstrapComponent>();
        }
    }
}
