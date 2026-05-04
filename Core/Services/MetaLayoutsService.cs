using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FiveMVehicleMetaEditorWPF.Core.Models;

namespace FiveMVehicleMetaEditorWPF.Core.Services
{
    /// <summary>
    /// Service for loading, parsing, and saving vehiclelayouts.meta files
    /// Real GTA format: CVehicleMetadataMgr > AnimRateSets > Item type="CAnimRateSet" > Name
    /// </summary>
    public class MetaLayoutsService
    {
        /// <summary>
        /// Load vehiclelayouts.meta file and extract layout entries
        /// </summary>
        public (bool Success, List<LayoutData>? Layouts, string? Error) LoadLayoutsMeta(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return (false, null, $"File not found: {filePath}");

                var content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                var doc = XDocument.Parse(content);
                var root = doc.Root;

                if (root == null)
                    return (false, null, "Invalid XML: No root element");

                var layoutList = new List<LayoutData>();

                // Real GTA vehiclelayouts.meta: <CVehicleMetadataMgr><AnimRateSets><Item type="CAnimRateSet"><Name>
                var items = root.Descendants("AnimRateSets")
                    .Elements("Item")
                    .ToList();

                // Fallback: try Layouts/Item (custom format)
                if (items.Count == 0)
                    items = root.Descendants("Layouts").Elements("Item").ToList();

                // Fallback: any Item with a Name child
                if (items.Count == 0)
                    items = root.Descendants("Item")
                        .Where(i => i.Element("Name") != null || i.Element("layoutName") != null)
                        .ToList();

                foreach (var item in items)
                {
                    var nameElem = item.Element("Name") ?? item.Element("layoutName");
                    if (nameElem?.Value == null || string.IsNullOrWhiteSpace(nameElem.Value))
                        continue;

                    var layout = new LayoutData
                    {
                        LayoutName = nameElem.Value,
                        Category = item.Attribute("type")?.Value ?? "AnimRateSet",
                        OriginalElement = item
                    };

                    layoutList.Add(layout);
                }

                return (true, layoutList, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error loading vehiclelayouts.meta: {ex.Message}");
            }
        }

        /// <summary>
        /// Save layout data back to XML file (preserves original if OriginalElement is available)
        /// </summary>
        public (bool Success, string? Error) SaveLayoutsMeta(string filePath, List<LayoutData> layoutList)
        {
            try
            {
                var backupPath = filePath + ".backup";
                if (File.Exists(filePath))
                    File.Copy(filePath, backupPath, true);

                var doc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement("CVehicleMetadataMgr",
                        new XElement("AnimRateSets",
                            layoutList.Select(l => l.OriginalElement ?? new XElement("Item",
                                new XAttribute("type", l.Category),
                                new XElement("Name", l.LayoutName)
                            ))
                        )
                    )
                );

                doc.Save(filePath, SaveOptions.OmitDuplicateNamespaces);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error saving vehiclelayouts.meta: {ex.Message}");
            }
        }

        /// <summary>
        /// Filter layouts by search term
        /// </summary>
        public List<LayoutData> FilterLayouts(List<LayoutData> layoutList, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return layoutList;

            return layoutList
                .Where(l => l.LayoutName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           l.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
