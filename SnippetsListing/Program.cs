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
            var slnDir = TryGetSolutionDirectoryInfo().FullName;
            var snippetsDir = Path.Combine(slnDir, "Bootstrap4vs", "Snippets");
            var componentsFilepath = Path.Combine(snippetsDir, "bootstrap-components.xml");
            snippetsDir = Path.Combine(snippetsDir, "Bootstrap4vs");

            var componentsDoc = XElement.Load(componentsFilepath);
            List<BootstrapComponent> components = componentsDoc.Elements("Component")
                 .Select(c => new BootstrapComponent
                 {
                     Name = (string)c.Attribute("Name"),
                     Directory = (string)c.Attribute("Directory"),
                     Url = (string)c.Attribute("Url")
                 })
                 .ToList<BootstrapComponent>();

            foreach (var component in components)
            {
                //Console.WriteLine($"Process {file}");
                var componentDir = Path.Combine(snippetsDir, component.Directory);
                foreach (string file in Directory.EnumerateFiles(componentDir, "*.snippet", SearchOption.AllDirectories))
                {
                    //var snippetDoc = XElement.Load(file);
                    //var s = snippetDoc.Elements().FirstOrDefault().Descendants("Header").ToList();
                         //.Select(h => new Snippet
                         //{
                         //    Title = (string)h.Element("Name").Value,
                         //    Shortcut = (string)h.Attribute("Directory"),
                         //    Description = (string)h.Attribute("Url")
                         //});


                    using (XmlReader reader = XmlReader.Create(file))
                    {
                        var snippet = new Snippet();
                        component.Snippets.Add(snippet);
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                //return only when you have START tag  
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
                }
            }
            var html = SnippetsListToHtmlTable(components);
            var path = $"{slnDir}\\SnippetsListing.md";
            File.WriteAllText(path, html);



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
    }
    public class BootstrapComponent
    {
        public string Name { get; set; }
        public string Directory { get; set; }
        public string Url { get; set; }
        public int SnippetsCount { get; set; }
        public List<Snippet> Snippets { get; set; } = new List<Snippet>();
    }
}
