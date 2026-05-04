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

                // Find all layout entries
                var items = root.Descendants("Layout")
                    .ToList();

                foreach (var item in items)
                {
                    var layoutNameElem = item.Element("layoutName");
                    if (layoutNameElem?.Value == null || string.IsNullOrWhiteSpace(layoutNameElem.Value))
                        continue;

                    var layout = new LayoutData
                    {
                        LayoutName = layoutNameElem.Value,
                        OriginalElement = item
                    };

                    // Parse category if available
                    var categoryElem = item.Element("category");
                    if (categoryElem != null)
                        layout.Category = categoryElem.Value;

                    // Parse seats
                    var seatsElem = item.Element("Seats");
                    if (seatsElem != null)
                    {
                        foreach (var seat in seatsElem.Elements("Seat"))
                        {
                            if (int.TryParse(seat.Attribute("id")?.Value, out var seatId))
                            {
                                var seatName = seat.Attribute("name")?.Value ?? $"Seat_{seatId}";
                                layout.AddSeat(seatId, seatName);
                            }
                        }
                    }

                    // Parse doors
                    var doorsElem = item.Element("Doors");
                    if (doorsElem != null)
                    {
                        foreach (var door in doorsElem.Elements("Door"))
                        {
                            if (int.TryParse(door.Attribute("id")?.Value, out var doorId))
                            {
                                var doorName = door.Attribute("name")?.Value ?? $"Door_{doorId}";
                                layout.AddDoor(doorId, doorName);
                            }
                        }
                    }

                    // Parse roof/trunk/hood flags
                    if (bool.TryParse(item.Element("HasRoof")?.Value, out var hasRoof))
                        layout.HasRoof = hasRoof;
                    if (bool.TryParse(item.Element("HasTrunk")?.Value, out var hasTrunk))
                        layout.HasTrunk = hasTrunk;
                    if (bool.TryParse(item.Element("HasHood")?.Value, out var hasHood))
                        layout.HasHood = hasHood;

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
        /// Save layout data back to XML file
        /// </summary>
        public (bool Success, string? Error) SaveLayoutsMeta(string filePath, List<LayoutData> layoutList)
        {
            try
            {
                // Create backup before saving
                var backupPath = filePath + ".backup";
                if (File.Exists(filePath))
                    File.Copy(filePath, backupPath, true);

                var doc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement("CVehicleLayouts",
                        layoutList.Select(l => CreateLayoutElement(l))
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

        /// <summary>
        /// Create XML element from LayoutData object
        /// </summary>
        private XElement CreateLayoutElement(LayoutData layout)
        {
            var seatsElement = new XElement("Seats",
                layout.Seats.Select(s => new XElement("Seat",
                    new XAttribute("id", s.Key),
                    new XAttribute("name", s.Value)
                ))
            );

            var doorsElement = new XElement("Doors",
                layout.Doors.Select(d => new XElement("Door",
                    new XAttribute("id", d.Key),
                    new XAttribute("name", d.Value)
                ))
            );

            var elem = new XElement("Layout",
                new XElement("layoutName", layout.LayoutName),
                new XElement("category", layout.Category),
                seatsElement,
                doorsElement,
                new XElement("HasRoof", layout.HasRoof),
                new XElement("HasTrunk", layout.HasTrunk),
                new XElement("HasHood", layout.HasHood)
            );

            return elem;
        }
    }
}
