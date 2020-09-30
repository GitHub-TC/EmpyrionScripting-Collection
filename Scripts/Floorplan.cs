using System.Text;
using System;
using System.Collections.Generic;
using System.IO;

public class ModMain
{
    static int[] blocks;
    static int[] devices;
    static int[] weapons;

    public static void Main (IScriptModData rootObject)
    {
        if (!(rootObject is IScriptSaveGameRootData root)) return;
        if (root.E.Faction.Id == 0) return;

        root.CsRoot.GetBlockDevices<ILcd> (root.E.S, "Floor*")
            .ForEach (l =>
            {
                var output = new StringBuilder();

                try
                {
                    if (DateTime.TryParseExact(l.D.GetText().Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries).LastOrDefault(), 
                        "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out var lastUpdate) &&
                        (DateTime.Now - lastUpdate).TotalSeconds < 60) return;
                }
                catch { }

                try
                {
                    var posOff = l.B.CustomName.Split(':').Last().Split(';');
                    var lcdX = l.B.Position.x + GetInt(posOff, 0);
                    var lcdY = l.B.Position.y + GetInt(posOff, 1);
                    var lcdZ = l.B.Position.z + GetInt(posOff, 2);
                    var size = Get(posOff, 3, "50%");
                    var flip = Get(posOff, 4) == "r";
                    var cspace = Get(posOff, 5);

                    if (l.B.Id == 1400)
                    {
                        output.Append($"<mspace=3.8m>");
                    }
                    else
                    {
                        output.Append($"<mspace=1.8m>");
                    }
                    if (!string.IsNullOrEmpty(cspace)) output.Append($"<cspace={cspace}>");
                    output.Append($"<size={size}>");
                    output.Append("<line-height=70%>");

                    var min = root.E.S.MinPos;
                    var max = root.E.S.MaxPos;
                    if(blocks  == null) blocks  = ToIntArray(root.Ids["BlockL" ]);
                    if(devices == null) devices = ToIntArray(root.Ids["DeviceL"]);
                    if(weapons == null) weapons = ToIntArray(root.Ids["WeaponHV"] + root.Ids["WeaponSV"] + root.Ids["WeaponCV"] + root.Ids["WeaponBA"]);
                    var color    = "";
                    var oldColor = "";
                    var blkChar  = "";
                    for (int x = flip ? max.x : min.x; flip ? x >= min.x : x <= max.x; x += flip ? -1 : +1)
                    {
                        for (int z = flip ? max.z : min.z; flip ? z >= min.z : z <= max.z; z += flip ? -1 : +1)
                        {
                            var b = root.CsRoot.Block (root.E.S, x, lcdY, z);
                            var bCfg = root.CsRoot.ConfigById(b.Id);
                            var tn = bCfg?.Attr
                                .FirstOrDefault(N => N.Name == "Id")?
                                .AddOns["Name"].ToString() ?? string.Empty;
                            var t = bCfg?.Attr
                                .FirstOrDefault (N => N.Name == "TemplateRoot") ?
                                .Value?.ToString () ?? string.Empty;
                            var tg = bCfg?.Attr
                                .FirstOrDefault(N => N.Name == "Group")?
                                .Value?.ToString() ?? string.Empty;

                            if (x == lcdX && z == lcdZ)
                            {
                                blkChar = "☻";
                                color = "red";
                            }
                            else if (tn.Contains("Controller"))
                            {
                                blkChar = "▣";
                                color = "green";
                            }
                            else if (tn.Contains("Constructor") || tn.Contains("Food"))
                            {
                                blkChar = "▣";
                                color = "#666600";
                            }
                            else if (tn.Contains("OxygenStation"))
                            {
                                blkChar = "❂";
                                color = "blue";
                            }
                            else if (t.Contains("Medical"))
                            {
                                blkChar = "♥";
                                color = "red";
                            }
                            else if (t.Contains("Thrus"))
                            {
                                blkChar = "⛔";
                                color = "#606000";
                            }
                            else if (t.Contains("Stairs"))
                            {
                                blkChar = "☷";
                                color = "#C0C0C0";
                            }
                            else if (t.Contains("Door"))
                            {
                                blkChar = "♓";
                                color = "green";
                            }
                            else if (t.Contains("Cockpit"))
                            {
                                blkChar = "☸";
                                color = "#101080";
                            }
                            else if (t.Contains("Light") ||
                                t.Contains("Cons") ||
                                t.Contains("Force") ||
                                t.Contains("Door")) blkChar = " ";
                            else if (b.Id == 0) blkChar = " ";
                            else if (weapons.Contains(b.Id) || t.Contains("Turret"))
                            {
                                blkChar = "▣";
                                color = "#7f00ff";
                            }
                            else if (devices.Contains(b.Id))
                            {
                                blkChar = "▣";
                                color = "#99ccff";
                            }
                            else if (blocks.Contains(b.Id))
                            {
                                var hitPoints = b.HitPoints;
                                var damage = b.Damage;
                                color = "#606060";
                                if (damage > 0) color = "yellow";
                                if (damage > (hitPoints * 9 / 10)) color = "red";
                                blkChar = "■";
                            }
                            else
                            {
                                blkChar = "▪";
                                color = "grey";
                            }
                            if (oldColor != color) output.Append ($"</color><color={oldColor = color}>");
                            output.Append (blkChar);
                        }
                        output.AppendLine ();
                    }
                    output.AppendLine($"<color=white><color=red>☻</color> = You are here X:{lcdX} Y:{lcdY} Z:{lcdZ}");
                    output.AppendLine($"<color=#99ccff>▣</color> = Device");
                    output.AppendLine($"<color=grey>▪</color> = Unkown");
                    output.AppendLine($"■ = Block");
                    output.AppendLine();
                    output.Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                }
                catch (Exception e)
                {
                    output.Append (e);
                }
                l.D.SetText (output.ToString ());
            });

    }

    private static int[] ToIntArray(string ids) => ids.Split(',').Select(i => int.TryParse(i, out var ii) ? ii : 0).ToArray();
    private static string Get (string[] a, int pos, string def = "") => a != null && a.Length <= pos ? def : a[pos];
    private static int GetInt (string[] a, int pos) => int.TryParse (Get(a, pos), out var i) ? i : 0;
}