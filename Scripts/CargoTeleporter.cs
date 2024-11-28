using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eleon.Modding;
using EmpyrionScripting.CustomHelpers;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using Newtonsoft.Json;

public class ModMain
{
    public static void Main (IScriptModData rootObject)
    {
        if (!(rootObject is IScriptSaveGameRootData root)) return;
        if (root.E.Faction.Id == 0) return;

        root.E.S
            .AllCustomDeviceNames
            .GetUniqueNames ("*CargoOut@*")
            .ForEach (cargoOutContainerName =>
            {
                var container = root.CsRoot.Devices (root.E.S, cargoOutContainerName).Where(C => C.Device is ContainerData).FirstOrDefault ();
                if (container == null) return;

                var nameParts = cargoOutContainerName.Split('#');
                var ignoreIDs = new HashSet<int>();
                if (nameParts.Length > 1)
                {
                    ignoreIDs = nameParts[1]
                        .Split(',', ';')
                        .Select(i => int.TryParse(i, out int no) ? no : 0)
                        .Where(n => n != 0)
                        .ToHashSet();
                    cargoOutContainerName = nameParts[0];
                }

                if (!int.TryParse (cargoOutContainerName.Substring (cargoOutContainerName.IndexOf('@') + 1), out var targetEntityId))
                {
                    WriteTo (root, $"CargoOut@[ID] id is not a number", "CargoOutInfo*");
                    return;
                }

                var cargoTargetFileName = Path.Combine (root.MainScriptPath, "..", "CargoTeleport", root.E.Faction.Id.ToString (), $"Cargo-{targetEntityId}.json");

                if (!File.Exists (cargoTargetFileName))
                {
                    WriteTo (root, $"CargoIn in [ID] not ready", "CargoOutInfo*");
                    return;
                }

                root.CsRoot.WithLockedDevice (root.E.S, container, () =>
                {
                    var nativeContainer = ((ContainerData)container.Device).GetContainer() as IContainer;
                
                    var items = nativeContainer.GetContent ();
                    var failedItems = new List<ItemStack> ();
                    items.ForEach (i =>
                    {
                        if (ignoreIDs.Contains(i.id))
                        {
                            failedItems.Add(i);
                            WriteTo(root, $"ignored: [{i.id}] {i.count}: {root.CsRoot.I18n(i.id)}", "CargoOutInfo*");
                        }
                        else
                        {
                            try
                            {
                                File.AppendAllText(cargoTargetFileName, JsonConvert.SerializeObject(i) + "\n");
                                WriteTo(root, $"[{i.id}] {i.count}: {root.CsRoot.I18n(i.id)}", "CargoOutInfo*");
                            }
                            catch
                            {
                                failedItems.Add(i);
                                WriteTo(root, $"failed: [{i.id}] {i.count}: {root.CsRoot.I18n(i.id)}", "CargoOutInfo*");
                            }
                        }
                    });
                    nativeContainer.SetContent (failedItems.UniqueSlots());
                });
            });

        root.E.S
            .AllCustomDeviceNames
            .GetUniqueNames ("*CargoIn*")
            .ForEach (cargoInContainerName =>
            {
                var container = root.CsRoot.Devices(root.E.S, cargoInContainerName).Where(C => C.Device is ContainerData).FirstOrDefault();
                if (container == null) return;

                var nativeContainer = ((ContainerData)container.Device).GetContainer() as IContainer;
                if (nativeContainer == null) return;

                var cargoTargetFileName = Path.Combine (root.MainScriptPath, "..", "CargoTeleport", root.E.Faction.Id.ToString (), $"Cargo-{root.E.Id}.json");
                Directory.CreateDirectory(Path.GetDirectoryName(cargoTargetFileName));

                root.CsRoot.WithLockedDevice (root.E.S, container, () =>
                {
                    var items = nativeContainer.GetContent() ?? new List<ItemStack>();
                    bool itemsAdded = false;
                    using (var lockFile = File.Open (cargoTargetFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        using (var file = new StreamReader (lockFile))
                        {
                            var remainLines = new List<string> ();

                            while (true)
                            {
                                var itemLine = file.ReadLine ();
                                if(string.IsNullOrEmpty(itemLine)) break;
                                else if (items.Count < root.GetMaxSlots(container.Id))
                                {
                                    var item = JsonConvert.DeserializeObject<ItemStack> (itemLine);
                                    items.Add (item);
                                    itemsAdded = true;
                                    WriteTo (root, $"[{item.id}] {item.count}: {root.CsRoot.I18n(item.id)}", "CargoInInfo*");
                                }
                                else remainLines.Add (itemLine);
                            }

                            lockFile.Seek (0, SeekOrigin.Begin);
                            lockFile.SetLength (0);

                            using (var fileWrite = new StreamWriter (lockFile))
                            {
                                remainLines.ForEach (l => fileWrite.WriteLine (l));
                            }
                        }
                    }
                    if (items.Count > 0 && itemsAdded)
                    {
                        byte index = 0;
                        nativeContainer.SetContent(items
                            .Select(i => new ItemStack(i.id, i.count) { slotIdx = index++, decay = i.decay, ammo = i.ammo }).ToList());
                    }
                });
            });

    }

    private static void WriteTo (IScriptSaveGameRootData root, string text, string lcdNames)
    {
        root.CsRoot.Devices (root.E.S, lcdNames).ForEach(D => {
            root.CsRoot.GetDevices<ILcd>(D).ForEach(L => {
                var oldText = L.GetText();

                var attrPos = D.CustomName.IndexOf('[');
                if(attrPos > 0){
                    var attrEndPos = D.CustomName.IndexOfAny(new[] { ']', ';' }, attrPos);
                    if (attrEndPos > 0) text = $"<size={D.CustomName.Substring(attrPos + 1, attrEndPos - attrPos - 1)}>{text}</size>"; 
                }

                L.SetText ($"{text}\n{oldText.Substring(0, Math.Min(2000, oldText.Length))}");
            });
        });
    }
}