using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class ModMain
{
    static int[] blocks;
    static int[] devices;

    public static void Main (IScriptModData rootObject)
    {
        if (!(rootObject is IScriptSaveGameRootData root)) return;
        if (root.E.Faction.Id == 0) return;

        root.CsRoot.GetBlockDevices<ILcd> (root.E.S, "Floor*")
            .ForEach (l =>
            {
                var output = new StringBuilder ();
                try
                {
                    var posOff = l.B.CustomName.Split(':').Last().Split(';');
                    var lcdX = l.B.Position.x + GetInt(posOff, 0);
                    var lcdY = l.B.Position.y + GetInt(posOff, 1);
                    var lcdZ = l.B.Position.z + GetInt(posOff, 2);
                    var size = Get(posOff, 3, "50%");
                    var flip = Get(posOff, 4) == "r";

                    output.Append ($"<mspace={(l.B.Id==1400 ? 3 : 1)}.8m>");
                    output.Append ($"<size={size}>");
                    output.Append ("<line-height=70%>");

                    var min = root.E.S.MinPos;
                    var max = root.E.S.MaxPos;
                    if(blocks  == null) blocks  = root.Ids["BlockL" ].Split (',').Select (i => int.TryParse (i, out var ii) ? ii : 0).ToArray();
                    if(devices == null) devices = root.Ids["DeviceL"].Split (',').Select (i => int.TryParse (i, out var ii) ? ii : 0).ToArray();
                    var color    = "";
                    var oldColor = "";
                    var blkChar  = "";
                    for (int x = flip ? max.x : min.x; flip ? x >= min.x : x <= max.x; x += flip ? -1 : +1)
                    {
                        for (int z = flip ? max.z : min.z; flip ? z >= min.z : z <= max.z; z += flip ? -1 : +1)
                        {
                            var b = root.CsRoot.Block (root.E.S, x, lcdY, z);
                            var t = root.CsRoot.ConfigById (b.Id)?.Attr
                                .FirstOrDefault (N => N.Name == "TemplateRoot") ?
                                .Value?.ToString () ?? "";

                            if (x == lcdX && z == lcdZ)
                            {
                                blkChar = "☻";
                                color = "red";
                            }
                            else if (t.Contains ("Thrus")) blkChar = "⛔";
                            else if (t.Contains ("Light") ||
                                t.Contains ("Cons") ||
                                t.Contains ("Force") ||
                                t.Contains ("Door")) blkChar = " ";
                            else if (b.Id == 0) blkChar = " ";
                            else if (devices.Contains (b.Id))
                            {
                                blkChar = "▣";
                                color = "#99ccff";
                            }
                            else if (blocks.Contains (b.Id))
                            {
                                var hitPoints = b.HitPoints;
                                var damage = b.Damage;
                                color = "grey";
                                if (damage > 0) color = "yellow";
                                if (damage > (hitPoints * 9 / 10)) color = "red";
                                blkChar = "■";
                            }
                            else
                            {
                                blkChar = "▪";
                                color = "grey";
                            }
                            if (oldColor != color) output.Append ($"<color={oldColor = color}>");
                            output.Append (blkChar);
                        }
                        output.AppendLine ();
                    }
                    output.AppendLine($"<color=red>☻</color> = You are here X:{lcdX} Y:{lcdY} Z:{lcdZ}");
                    output.AppendLine($"<color=#99ccff>▣</color> = Device");
                    output.AppendLine($"<color=grey>▪</color> = Unkown");
                    output.AppendLine($"■ = Block");
                }
                catch (Exception e)
                {
                    output.Append (e);
                }
                l.D.SetText (output.ToString ());
            });

    }

    private static string Get (string[] a, int pos, string def = "") => a != null && a.Length <= pos ? def : a[pos];
    private static int GetInt (string[] a, int pos) => int.TryParse (Get(a, pos), out var i) ? i : 0;
}