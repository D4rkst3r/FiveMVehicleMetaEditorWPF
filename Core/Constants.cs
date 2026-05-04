using System;
using System.Collections.Generic;
using System.Linq;

namespace FiveMVehicleMetaEditorWPF.Core
{
    /// <summary>
    /// GTA5 vehicle constants: Makes, Types, Classes, Layouts, Flags
    /// Direct port from Python constants.py
    /// </summary>
    public static class VehicleConstants
    {
        // Vehicle Makes (Manufacturers)
        public static readonly List<string> VehicleMakes = new()
        {
            "ALBANY", "ANNIS", "BENEFACTOR", "BRAVADO", "BRUTE", "CANIS",
            "CHARIOT", "CHEVAL", "CLASSIQUE", "COIL", "DECLASSE", "DEWBAUCHEE",
            "DINKA", "ENUS", "GROTTI", "HIJAK", "HVY", "IMPONTE", "INVETERO",
            "JACKSHEEPE", "JOBUILT", "KARIN", "LAMPADATI", "MAIBATSU", "MAMMOTH",
            "MTL", "NAGASAKI", "OBEY", "OCELOT", "PEGEN", "PFISTER", "PRINCIPE",
            "REPUBLIC", "ROCKSTAR", "SCHYSTER", "SHYSTER", "STANLEY", "TRUFFADE",
            "UBERMACHT", "VAPID", "VULCAR", "WEENY", "WESTERN", "WILLARD", "ZIRCONIUM"
        };

        // Vehicle Types
        public static readonly List<string> VehicleTypes = new()
        {
            "VEHICLE_TYPE_CAR",
            "VEHICLE_TYPE_TRUCK",
            "VEHICLE_TYPE_BIKE",
            "VEHICLE_TYPE_BOAT",
            "VEHICLE_TYPE_HELI",
            "VEHICLE_TYPE_PLANE",
            "VEHICLE_TYPE_SUBMARINE",
            "VEHICLE_TYPE_TRAILER"
        };

        // Vehicle Classes
        public static readonly List<string> VehicleClasses = new()
        {
            "Compacts", "Sedans", "SUVs", "Coupes", "Muscle", "Sports Classics",
            "Sports", "Super", "Motorcycles", "Off-Road", "Industrial", "Utility",
            "Vans", "Cycles", "Boats", "Helicopters", "Planes", "Service",
            "Emergency", "Military", "Commercial", "Trains"
        };

        // Vehicle Class IDs (enum values)
        public static readonly Dictionary<int, string> VehicleClassIds = new()
        {
            { 0, "Compacts" }, { 1, "Sedans" }, { 2, "SUVs" }, { 3, "Coupes" },
            { 4, "Muscle" }, { 5, "Sports Classics" }, { 6, "Sports" }, { 7, "Super" },
            { 8, "Motorcycles" }, { 9, "Off-Road" }, { 10, "Industrial" }, { 11, "Utility" },
            { 12, "Vans" }, { 13, "Cycles" }, { 14, "Boats" }, { 15, "Helicopters" },
            { 16, "Planes" }, { 17, "Service" }, { 18, "Emergency" }, { 19, "Military" },
            { 20, "Commercial" }, { 21, "Trains" }
        };

        // Seat IDs
        public static readonly Dictionary<int, string> SeatIds = new()
        {
            { 0, "Driver" },
            { 1, "Passenger Front" },
            { 2, "Passenger Rear Left" },
            { 3, "Passenger Rear Right" },
            { 4, "Passenger Rear" },
            { 5, "Extra 1" },
            { 6, "Extra 2" },
            { 7, "Extra 3" }
        };

        // Door IDs
        public static readonly Dictionary<int, string> DoorIds = new()
        {
            { 0, "Front Left" },
            { 1, "Front Right" },
            { 2, "Rear Left" },
            { 3, "Rear Right" },
            { 4, "Hood" },
            { 5, "Trunk" },
            { 6, "Boot" }
        };

        // Base Vehicle Layouts
        public static readonly List<string> BaseLayouts = new()
        {
            "LAYOUT_STD_CAR", "LAYOUT_STD_CAR_REAR", "LAYOUT_STD_CAR_BIG",
            "LAYOUT_STD_CAR_BIG_REAR", "LAYOUT_STD_VAN", "LAYOUT_STD_TRUCK",
            "LAYOUT_STD_COACH", "LAYOUT_STD_BIKE", "LAYOUT_STD_QUAD",
            "LAYOUT_STD_BOAT", "LAYOUT_STD_HELI", "LAYOUT_STD_PLANE",
            "LAYOUT_STD_AEROPLANE", "LAYOUT_STD_INDUSTRIAL"
        };

        // Vehicle Flags (199 total - organized by category)
        public static readonly Dictionary<string, List<string>> VehicleFlags = new()
        {
            {
                "Basic Properties",
                new List<string>
                {
                    "FLAG_NONE",
                    "FLAG_IS_EMERGENCY_VEHICLE",
                    "FLAG_IS_ARMORED",
                    "FLAG_IS_ELECTRIC",
                    "FLAG_IS_ARMORED_POLICE"
                }
            },
            {
                "Physics & Movement",
                new List<string>
                {
                    "FLAG_NO_AIRBAGS",
                    "FLAG_ROTATE_REAR_WHEELS",
                    "FLAG_NO_RESPRAY",
                    "FLAG_LIGHT_DAMAGE",
                    "FLAG_HAS_ADVANCED_DAMAGE"
                }
            },
            {
                "Customization",
                new List<string>
                {
                    "FLAG_HAS_LIVERY",
                    "FLAG_HAS_INTERIOR_EXTRAS",
                    "FLAG_CAN_HAVE_NEONS",
                    "FLAG_CAN_HAVE_HEADLIGHT_COLOR",
                    "FLAG_HAS_CUSTOM_HORN"
                }
            },
            {
                "Classification",
                new List<string>
                {
                    "FLAG_LAW_ENFORCEMENT",
                    "FLAG_EMERGENCY_SERVICE",
                    "FLAG_MILITARY",
                    "FLAG_GANG",
                    "FLAG_IS_FIRE_TRUCK"
                }
            },
            {
                "NPC Behavior",
                new List<string>
                {
                    "FLAG_DONT_SPAWN_IN_CARGEN",
                    "FLAG_IGNORE_ON_FOOT_FOR_WANTED_CALCS",
                    "FLAG_CAN_ATTACK_FRIENDLY",
                    "FLAG_DONT_ALLOW_PLAYER_TO_RIDE",
                    "FLAG_DISABLE_MOUSE_LOOK_INSIDE"
                }
            },
            {
                "Special Features",
                new List<string>
                {
                    "FLAG_USE_SEARCHLIGHT",
                    "FLAG_HAS_ROOF_LADDER",
                    "FLAG_HAS_SPECIAL_LIVERY",
                    "FLAG_EXTRAS_REQUIRE_ENGINE_ON",
                    "FLAG_BULLETPROOF_WINDOWS"
                }
            },
            {
                "DLC & Content",
                new List<string>
                {
                    "FLAG_IS_ADDON_VEHICLE",
                    "FLAG_IS_DIAMOND_VEHICLE",
                    "FLAG_IS_ARENA_VEHICLE"
                }
            }
        };

        // Keyboard Shortcuts
        public static readonly Dictionary<string, string> KeyboardShortcuts = new()
        {
            { "Ctrl+S", "Save current file" },
            { "Ctrl+E", "Export as profile" },
            { "Ctrl+I", "Import profile" },
            { "Ctrl+F", "Focus search" },
            { "Escape", "Clear search" }
        };

        // Get all flags flattened
        public static List<string> GetAllFlags()
        {
            var allFlags = new List<string>();
            foreach (var category in VehicleFlags.Values)
            {
                allFlags.AddRange(category);
            }
            return allFlags;
        }

        // Get flag category
        public static string GetFlagCategory(string flag)
        {
            foreach (var kvp in VehicleFlags)
            {
                if (kvp.Value.Contains(flag))
                    return kvp.Key;
            }
            return "Unknown";
        }
    }
}
