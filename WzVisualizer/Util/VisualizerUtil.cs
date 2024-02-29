using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

using WzVisualizer.GUI;
using WzVisualizer.GUI.Controls;

namespace WzVisualizer.Util {
    public class VisualizerUtil {

        #region enums
        private enum RootVTab {
            Equips = 0,
            Use = 1,
            Setup = 2,
            Etc = 3,
            Cash = 4,
            Map = 5,
            Mob = 6,
            Skills = 7,
            NPC = 8,
            Pets = 9,
            Reactors = 10
        }

        private enum EquipTab {
            Hair = 0,
            Face = 1,
            Weapon = 2,
            Accessory = 3,
            Cap = 4,
            Longcoat = 5,
            Coat = 6,
            Pants = 7,
            Shoes = 8,
            Cape = 9,
            Glove = 10,
            Ring = 11,
            Shield = 12,
            TamingMob = 13
        }
        #endregion

        public static void ProcessTab(int nRootTab, MainForm app) {
            SuspendLayouts(app);
            var rootTab = (RootVTab)nRootTab;
            switch (rootTab) {
                case RootVTab.Equips:
                    ParseEquips(app);
                    break;
                case RootVTab.Use:
                    ProcessGeneric(app, rootTab);
                    break;
                case RootVTab.Setup:
                    ProcessGeneric(app, rootTab);
                    break;
                case RootVTab.Etc:
                    ProcessGeneric(app, rootTab);
                    break;
                case RootVTab.Cash:
                    ProcessGeneric(app, rootTab);
                    break;
                case RootVTab.Map:
                    ParseMap(app);
                    break;
                case RootVTab.Mob:
                    ParseMob(app);
                    break;
                case RootVTab.Skills:
                    ParseSkills(app);
                    break;
                case RootVTab.NPC:
                    ParseNpc(app);
                    break;
                case RootVTab.Pets:
                    ProcessGeneric(app, rootTab);
                    break;
                case RootVTab.Reactors:
                    ParseReactors(app);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ResumeLayouts(app);
        }

        private static void SuspendLayouts(MainForm app) {
            foreach (TabPage tab in app.TabControlMain.TabPages) {
                foreach (var control in tab.Controls) {
                    var grid = control as DataGridView;
                    var sub = control as TabPage;
                    if (grid != null) grid.SuspendLayout();
                    if (sub != null) {
                        foreach (var subcontrol in sub.Controls) {
                            ((Control)subcontrol).SuspendLayout();
                        }
                    }
                }
            }
        }

        private static void ResumeLayouts(MainForm app) {
            foreach (TabPage tab in app.TabControlMain.TabPages) {
                foreach (var control in tab.Controls) {
                    var grid = control as DataGridView;
                    var sub = control as TabPage;
                    if (grid != null) grid.ResumeLayout();
                    if (sub != null) {
                        foreach (var subcontrol in sub.Controls) {
                            ((Control)subcontrol).ResumeLayout();
                        }
                    }
                }
            }
        }

        #region character
        private static void ParseEquips(MainForm app) {
            var files = Wz.Character.GetFiles();

            foreach (var file in files) {
                // 64-bit client support
                if (!file.Name.StartsWith("Character.wz")) {
                    var name = Regex.Replace(Path.GetFileNameWithoutExtension(file.Name), "[^a-zA-Z]", "");
                    if (!Enum.TryParse<EquipTab>(name, out var tabName)) continue;
                    if (!file.Name.StartsWith(tabName.ToString())) continue;
                }


                Stopwatch timer = new Stopwatch();
                timer.Start();

                file.ParseWzFile();
                var root = file.WzDirectory;
                var images = root.WzImages;

                // classic
                if (root.WzDirectories.Count > 0) {
                    images = new List<WzImage>(images);
                    images.AddRange(CollectAllNodes(root));
                }

                timer.Stop();
                Debug.WriteLine($"[VisualizerUtil] {file.Name} loaded {images.Count} entries in {timer.Elapsed}ms");

                foreach (var img in images) {
                    if (!int.TryParse(Path.GetFileNameWithoutExtension(img.Name), out var itemId)) continue;

                    Bitmap image = null;

                    DataViewport dv;

                    switch (itemId / 10000) {
                        case 3 or 4 or 6:
                            dv = app.EquipHairsView;
                            break;
                        case 2 or 5:
                            dv = app.EquipFacesView;
                            break;
                        case >= 130 and <= 170:
                            dv = app.EquipWeaponsView;
                            break;
                        case (>= 101 and <= 103) or (>= 112 and <= 114):
                            dv = app.EquipAccessoryView;
                            break;
                        case 100:
                            dv = app.EquipCapsView;
                            break;
                        case 105:
                            dv = app.EquipOverallsView;
                            break;
                        case 104:
                            dv = app.EquipTopsView;
                            break;
                        case 106:
                            dv = app.EquipPantsView;
                            break;
                        case 107:
                            dv = app.EquipShoesView;
                            break;
                        case 108:
                            dv = app.EquipGlovesView;
                            break;
                        case 109:
                            dv = app.EquipShieldsView;
                            break;
                        case 110:
                            dv = app.EquipCapesView;
                            break;
                        case 111:
                            dv = app.EquipRingsView;
                            break;
                        case 190 or 191 or 193:
                            dv = app.EquipMountsView;
                            break;
                        default: continue;
                    }


                    if (app.GetCurrentDataViewport() == dv) {
                        var name = StringWz.GetEqp(itemId);
                        var properties = GetAllProperties(img);

                        switch (itemId / 10000) {
                            case 3 or 4 or 6: {
                                    image = (img.GetFromPath("default/hairOverHead") ?? img.GetFromPath("default/hair"))
                                        ?.GetLinkedWzImageProperty().GetBitmap();
                                    var hairBelowBody = (img.GetFromPath("default/hairBelowBody") as WzCanvasProperty)
                                        ?.GetLinkedWzImageProperty().GetBitmap();

                                    // combine front and back hair
                                    if (image != null && hairBelowBody != null) {
                                        var merge = new Bitmap(Math.Max(image.Width, hairBelowBody.Width),
                                            Math.Max(image.Height, hairBelowBody.Height));
                                        using (var g = Graphics.FromImage(merge)) {
                                            g.DrawImage(hairBelowBody, Point.Empty);
                                            g.DrawImage(image, Point.Empty);
                                        }

                                        image = merge;
                                    }

                                    break;
                                }
                            case 2 or 5:
                                image = (img.GetFromPath("default/face") ?? img.GetFromPath("blink/0/face"))
                                    ?.GetLinkedWzImageProperty().GetBitmap();
                                break;
                            default:
                                var icon = img.GetFromPath("info/icon");
                                if (icon != null) image = icon?.GetLinkedWzImageProperty().GetBitmap();
                                break;
                        }

                        image = DuplicateBitmap(image);
                        dv.GridView.Rows.Add(itemId, image, name, properties.Substring(0, Math.Min(properties.Length, 50)));
                        dv.Data.Add(new BinData(itemId, image, name, properties));
                    }
                }
                file.Dispose();
            }

            Wz.Character.Dispose();
        }
        #endregion

        #region generic
        private static void ProcessGeneric(MainForm app, RootVTab rootTab) {
            void AddNewRows(WzImage wz, DataViewport dv) {
                wz.WzProperties.ForEach(img => ParseGeneric(dv.GridView, img));
            }

            // loop through types of Wz enum

            foreach (Wz wz in Enum.GetValues(typeof(Wz))) {
                List<WzFile> files = wz.GetFiles();

                foreach (var file in files) {

                    // 64-bit parsing optimization due to having so many partitioned files 
                    // maplestory is now 30GB+ thus .wz files are now separated.
                    // i only want to parse necessary files that contain the items needed for rendering
                    switch (rootTab) {
                        case RootVTab.Use:
                            // 64-bit client
                            if (!file.Name.StartsWith("Consume") &&
                                // classic
                                !file.Name.StartsWith("Item")) continue;
                            break;

                        case RootVTab.Setup:
                            if (!file.Name.StartsWith("Item")) continue;
                            break;
                        case RootVTab.Etc:
                            if (!file.Name.StartsWith("Item")) continue;
                            break;
                        case RootVTab.Cash:
                            if (!file.Name.StartsWith("Cash")
                                && !file.Name.StartsWith(("Item"))) continue;
                            break;
                        case RootVTab.Pets:
                            // 64-bit client
                            if (!file.Name.StartsWith("Pet") &&
                                // classic
                                !file.Name.StartsWith("Item")) continue;
                            break;
                        case RootVTab.Map:
                        case RootVTab.Mob:
                        case RootVTab.Skills:
                        case RootVTab.NPC:
                        case RootVTab.Equips:
                        case RootVTab.Reactors:
                        default: continue;
                    }

                    Console.WriteLine($"[VisualizerUtil] Loading {file.Name} via common structure");
                    file.ParseWzFile();
                    var root = file.WzDirectory;
                    var images = root.WzImages;

                    // classic
                    if (images.Count == 0 && root.WzDirectories.Count > 0) {
                        images = new List<WzImage>();
                        foreach (var dir in root.WzDirectories) {
                            images.AddRange(dir.WzImages);
                        }
                    }

                    // 64-bit client support
                    foreach (var img in images) {
                        if (!int.TryParse(Path.GetFileNameWithoutExtension(img.Name), out var itemId)) continue;

                        switch (rootTab) {
                            case RootVTab.Use when itemId / 100 == 2:
                                switch (itemId) {
                                    case <= 203:
                                        AddNewRows(img, app.UseConsumeView);
                                        break;
                                    case 204:
                                    case 234:
                                        AddNewRows(img, app.UseScrollsView);
                                        break;
                                    case 206:
                                    case 207:
                                    case 233:
                                        AddNewRows(img, app.UseProjectileView);
                                        break;
                                }

                                break;
                            case RootVTab.Setup when itemId is 301 or 399 or >= 3010 and <= 30157:
                                // 301 ~ 399: classic
                                // 3010 ~ 3014: 64-bit client
                                var chair = itemId == 301 || itemId >= 3010 && itemId <= 3014;
                                AddNewRows(img, chair ? app.SetupChairsView : app.SetupOthersView);
                                break;
                            case RootVTab.Etc when ItemConstants.IsEtc(itemId):
                                AddNewRows(img, app.EtcView);
                                break;
                            case RootVTab.Cash when ItemConstants.IsCash(itemId):
                                AddNewRows(img, app.CashView);
                                break;
                            case RootVTab.Pets when ItemConstants.IsPet(itemId):
                                // AddNewRows(img, app.PetsView);
                                ParseGeneric(app.PetsView.GridView, img);
                                break;
                        }
                    }
                }
            }
        }

        private static void ParseGeneric(DataGridView grid, object wzObject) {
            int id;
            string name = null;

            WzImageProperty icon;

            switch (wzObject) {
                default: return;
                case WzImage img: {
                        id = int.Parse(Path.GetFileNameWithoutExtension(img.Name));
                        var link = img.GetFromPath("info/link");
                        if (link is WzStringProperty) {
                            var linkName = ((WzStringProperty)link).Value;
                            img = ((WzDirectory)img.Parent).WzImages.Find(p => p.Name.Equals(linkName + ".img"));
                            if (img == null) return;
                        }

                        icon = img.GetFromPath("stand/0");
                        if (img.WzFileParent.Name.StartsWith("Npc")) {
                            // icon path like: '{ID}/stand/0'
                            name = StringWz.GetNpc(id);
                            icon ??= img.GetFromPath("default");
                        } else if (img.WzFileParent.Name.StartsWith("Mob")) {
                            // icon path like: '{ID}/(move|stand|fly)/0'
                            name = StringWz.GetMob(id);
                            icon ??= img.GetFromPath("fly/0") ??
                                     img.GetFromPath("move/0");
                        } else if (img.WzFileParent.Name.StartsWith("Reactor")) {
                            name = img.GetFromPath("action")?.GetString();
                            icon = img.GetFromPath("0/0");
                        } else if (img.WzFileParent.Name.StartsWith("Map")) {
                            name = StringWz.GetFieldFullName(id);
                            icon = img.GetFromPath("miniMap/canvas"); ;
                        } else {
                            // for icon path like: '{ID}.img/info/icon'
                            if (ItemConstants.IsEquip(id)) name = StringWz.GetEqp(id);
                            else if (ItemConstants.IsPet(id)) name = StringWz.GetPet(id);
                            icon = img.GetFromPath("info/icon");
                        }

                        break;
                    }
                case WzSubProperty subProperty: {
                        id = int.Parse(subProperty.Name);

                        if (subProperty.WzFileParent.Name.StartsWith("Skill")) {
                            name = StringWz.GetSkill(subProperty.Name);
                            icon = subProperty.GetFromPath("icon");
                            break;
                        }

                        // for path like: 'category.img/{ID}/info/icon' (Etc.wz)
                        id = int.Parse(subProperty.Name);
                        icon = subProperty.GetFromPath("info/icon")
                               ?? subProperty.GetFromPath("info/iconRaw");
                        if (ItemConstants.IsEtc(id)) name = StringWz.GetEtc(id);
                        else if (ItemConstants.IsCash(id)) name = StringWz.GetCash(id);
                        else if (ItemConstants.IsChair(id)) name = StringWz.GetChair(id);
                        else if (ItemConstants.IsConsume(id)) name = StringWz.GetConsume(id);
                        break;
                    }
            }

            var properties = GetAllProperties(wzObject);
            Bitmap image = null;
            try {
                image = icon?.GetLinkedWzImageProperty().GetBitmap();
            } catch (NullReferenceException e) {
                // null UOL property while parsing v238 NPC.wz...
                image = null;
            }
            image = DuplicateBitmap(image);
            grid.Rows.Add(id, image, name, properties.Substring(0, Math.Min(properties.Length, 50)));
            ((DataViewport)grid.Parent).Data.Add(new BinData(id, image, name, properties));

        }
        #endregion

        private static void ParseMap(MainForm app) {
            var files = Wz.Map.GetFiles();

            // var children = file.WzDirectory.GetChildImages();
            // children.Sort((a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));

            foreach (var file in files) {
                Console.WriteLine($"[VisualizerUtil] Loading {file.Name}");
                file.ParseWzFile();
                var root = file.WzDirectory;
                WzDirectory maps = root.GetDirectoryByName("Map");
                maps.WzDirectories.ForEach(dir => {
                    Match match = Regex.Match(dir.Name, "^Map\\d+$");
                    if (match.Success) {
                        dir.WzImages.ForEach(img => ParseGeneric(app.MapsView.GridView, img));
                    }
                });
            }

            Wz.Map.Dispose();
        }

        private static void ParseMob(MainForm app) {
            var files = Wz.Mob.GetFiles();

            foreach (var file in files) {
                Console.WriteLine($"[VisualizerUtil] Loading {file.Name}");
                file.ParseWzFile();
                var root = file.WzDirectory;

                foreach (var img in root.WzImages) {
                    ParseGeneric(app.MobsView.GridView, img);
                }
            }

            Wz.Mob.Dispose();
        }

        private static void ParseSkills(MainForm app) {
            var files = Wz.Skill.GetFiles();

            foreach (var file in files) {
                Console.WriteLine($"[VisualizerUtil] Loading {file.Name}");
                file.ParseWzFile();
                var root = file.WzDirectory;

                foreach (var img in root.WzImages) {
                    if (!int.TryParse(Path.GetFileNameWithoutExtension(img.Name), out _)) continue;

                    var tree = img.GetFromPath("skill");

                    if (!(tree is WzSubProperty)) continue;

                    tree.WzProperties.ForEach(img => ParseGeneric(app.SkillsView.GridView, img));
                    var skills = tree.WzProperties;
                }
            }

            Wz.Skill.Dispose();
        }

        private static void ParseNpc(MainForm app) {
            var files = Wz.Npc.GetFiles();

            foreach (var file in files) {
                Console.WriteLine($"[VisualizerUtil] Loading {file.Name}");
                file.ParseWzFile();
                var root = file.WzDirectory;

                foreach (var img in root.WzImages) {
                    ParseGeneric(app.NPCView.GridView, img);
                }
            }

            Wz.Npc.Dispose();
        }

        private static void ParseReactors(MainForm app) {
            var files = Wz.Reactor.GetFiles();

            foreach (var file in files) {
                Console.WriteLine($"[VisualizerUtil] Loading {file.Name}");
                file.ParseWzFile();
                var root = file.WzDirectory;

                foreach (var img in root.WzImages) {
                    ParseGeneric(app.ReactorView.GridView, img);
                }
            }

            Wz.Reactor.Dispose();
        }

        /// <summary>
        /// Add a row data to the specified grid view using parsed bin data
        /// </summary>
        /// <param name="grid">the grid view to add a row to</param>
        /// <param name="bin">bin data (wz files that were parsed then saved as a bin file type)</param>
        public static void AddNewRow(MainForm app, DataGridView grid, BinData bin) {
            var query = app.SearchQuery;
            if (!string.IsNullOrEmpty(query)) {
                var queries = query.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var arg in queries) {
                    // all queries must match
                    if (bin.Search(arg)) {
                        goto VALIDATED;
                    }
                }
                return;
            }

        VALIDATED:
            var properties = "";
            foreach (var p in bin.Properties) {
                properties += p + "\r\n";
            }

            grid.Rows.Add(bin.ID, bin.Image, bin.Name, properties);
        }


        /// <summary>
        /// Concatenate all properties excluding image and sound properties
        /// </summary>
        /// <param name="obj">a WzSubProperty or WzImage</param>
        /// <returns></returns>
        private static string GetAllProperties(object obj) {
            var append = "";

            void AppendLine(string blockName, WzImageProperty property) {
                if (property == null) return;

                if (append.Length > 0) append += "\r\n";
                append += blockName + AppendProperties(property, "");
            }

            switch (obj) {
                default:
                    throw new Exception($"unhandled parameter type '{nameof(obj)}': {obj}");
                case WzSubProperty sub:
                    AppendLine("[info]", sub.GetFromPath("info"));
                    AppendLine("[levels]", sub.GetFromPath("level")); // skills with defined level stats
                    AppendLine("[common]", sub.GetFromPath("common")); // skills with scaling level stats
                    break;
                case WzImage img:
                    AppendLine("[info]", img.GetFromPath("info"));
                    break;
            }

            return append;
        }

        private static string AppendProperties(WzImageProperty parent, string prefix) {
            if (parent?.WzProperties == null) return "";

            var properties = "";

            foreach (WzImageProperty sub in parent.WzProperties) {
                switch (sub.PropertyType) {
                    default:
                        // append all properties
                        properties += $"\r\n{prefix}{sub.Name}={sub.WzValue}";
                        if (sub.PropertyType == WzPropertyType.SubProperty) {
                            // increase sub-properties indent by 1 tab character
                            properties += AppendProperties(sub, "\t" + prefix);
                        }

                        break;
                    case WzPropertyType.Canvas:
                    case WzPropertyType.PNG:
                    case WzPropertyType.Sound:
                        break;
                }
            }

            return properties + "\r\n";
        }

        /// <summary>
        /// old game files have WzDirectory objects contained in .wz files
        /// but in newer versions, .wz files are replaced with directories, and those directories contain .wz files 
        /// this little bit of code recursively collects .img objects from WzDirectories.
        /// yes it's inefficient but it doesn't feel too slow...
        /// </summary>
        /// <param name="root">directory to start recursively collecting nodes from</param>
        /// <returns></returns>
        private static IEnumerable<WzImage> CollectAllNodes(WzDirectory root) {
            var list = new List<WzImage>(root.WzImages);

            foreach (var dir in root.WzDirectories) {
                list.AddRange(CollectAllNodes(dir));
            }
            return list;
        }

        /**
         * Copy the specified bitmap into a new bitmap object as is, default options
         */
        private static Bitmap DuplicateBitmap(Bitmap bmp) {
            // for some reason, wpf crashes if we don't copy the bitmap?????????? bruh moment
            if (bmp == null || bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.DontCare) return null;

            var image = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(image);
            g.DrawImage(bmp, Point.Empty);
            return image;
        }
    }
}