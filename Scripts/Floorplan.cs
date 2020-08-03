using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class ModMain
{
    public static void Main (IScriptModData rootObject)
    {
        if (!(rootObject is IScriptSaveGameRootData root)) return;
        if (root.E.Faction.Id == 0) return;

        root.CsRoot.GetBlockDevices<ILcd> (root.E.S, "Floor*")
            .ForEach (l =>
            {
                var o = new StringBuilder ();
                try
                {
                    var posOff = l.B.CustomName.Split(':').Last().Split(';');
                    var lcdX = l.B.Position.x + GetInt(posOff, 0);
                    var lcdY = l.B.Position.y + GetInt(posOff, 1);
                    var lcdZ = l.B.Position.z + GetInt(posOff, 2);
                    var size = Get(posOff, 3, "50%");
                    var flip = Get(posOff, 4) == "r";

                    o.Append ($"<mspace={(l.B.Id==1400 ? 3 : 1)}.8m>");
                    o.Append ($"<size={size}>");
                    o.Append ("<line-height=70%>");

                    var min = root.E.S.MinPos;
                    var max = root.E.S.MaxPos;
                    var blk = root.Ids["BlockL" ].Split (',').Select (i => int.TryParse (i, out var ii) ? ii : 0);
                    var eq  = root.Ids["DeviceL"].Split (',').Select (i => int.TryParse (i, out var ii) ? ii : 0);
                    var c = "";
                    var oc = "";
                    var s = "";
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
                                s = "☻";
                                c = "red";
                            }
                            else if (t.Contains ("Thrus")) s = "⛔";
                            else if (t.Contains ("Light") ||
                                t.Contains ("Cons") ||
                                t.Contains ("Force") ||
                                t.Contains ("Door")) s = " ";
                            else if (b.Id == 0) s = " ";
                            else if (eq.Contains (b.Id))
                            {
                                s = "▣";
                                c = "#99ccff";
                            }
                            else if (blk.Contains (b.Id))
                            {
                                int h = b.HitPoints;
                                var d = b.Damage;
                                c = "grey";
                                if (d > 0) c = "yellow";
                                if (d > (h * 9 / 10)) c = "red";
                                s = "■";
                            }
                            else
                            {
                                s = "▪";
                                c = "grey";
                            }
                            if (oc != c) o.Append ($"<color={oc=c}>");
                            o.Append (s);
                        }
                        o.AppendLine ();
                    }
                    o.AppendLine($"<color=red>☻</color> = You are here X:{lcdX} Y:{lcdY} Z:{lcdZ}");
                    o.AppendLine($"<color=#99ccff>▣</color> = Device");
                    o.AppendLine($"<color=grey>▪</color> = Unkown");
                    o.AppendLine($"■ = Block");
                }
                catch (Exception e)
                {
                    o.Append (e);
                }
                l.D.SetText (o.ToString ());
            });

    }

    private static string Get (string[] a, int pos, string def = "") => a != null && a.Length <= pos ? def : a[pos];
    private static int GetInt (string[] a, int pos) => int.TryParse (Get(a, pos), out var i) ? i : 0;
    
    private static void WriteTo (ILcd[] lcds, string text)
    {
        lcds.ForEach (L => L.SetText ($"{text}\n{L.GetText()}"));
    }
}