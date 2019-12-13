using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using The_Long_Dark_Save_Editor_2.Game_data;
using The_Long_Dark_Save_Editor_2.Helpers;
using The_Long_Dark_Save_Editor_2.Serialization;

namespace The_Long_Dark_Save_Editor_2
{
    public class GameSave
    {
        public static int MAX_BACKUPS = 20;

        public long LastSaved { get; set; }
        private DynamicSerializable<BootSaveGameFormat> dynamicBoot;
        public BootSaveGameFormat Boot { get { return dynamicBoot.Obj; } }

        private DynamicSerializable<GlobalSaveGameFormat> dynamicGlobal;
        public GlobalSaveGameFormat Global { get { return dynamicGlobal.Obj; } }

        public AfflictionsContainer Afflictions { get; set; }

        private DynamicSerializable<SlotData> dynamicSlotData;
        public SlotData SlotData { get { return dynamicSlotData.Obj; } }

        public string OriginalRegion { get; set; }

        public string path;

        public void LoadSave(string path)
        {
            this.path = path;
            string slotJson = EncryptString.Decompress(File.ReadAllBytes(path));
            dynamicSlotData = new DynamicSerializable<SlotData>(slotJson);

            var bootJson = EncryptString.Decompress(SlotData.m_Dict["boot"]);
            dynamicBoot = new DynamicSerializable<BootSaveGameFormat>(bootJson);
            OriginalRegion = Boot.m_SceneName.Value;

            var globalJson = EncryptString.Decompress(SlotData.m_Dict["global"]);
            dynamicGlobal = new DynamicSerializable<GlobalSaveGameFormat>(globalJson);

            Afflictions = new AfflictionsContainer(Global);
            /*
            byte[] array = File.ReadAllBytes(@"C:\temp\TLD\resources.assets");
            string str = "";
            //byte[] array1;
            for (int i = 0; i < array.Length; i++)
            {
                if ((array[i] >= 32) && (array[i] < 127)) {
                    //str = str + array[i].ToString();
                }
                else { array[i] = 32; }
            }
            */
            /*
            var str = File.ReadAllText(@"C:\temp\TLD\resources.txt");
            string str1 = "";
            var str2 = Regex.Replace(str, @"(GEAR_[0-9A-Za-z_-]*)", delegate (Match match)
            {
                str1 += match.Groups[1].ToString() + "\r\n";
                return "";
            });
            */
            //File.WriteAllBytes(@"C:\temp\TLD\resources.txt",array);
            //File.WriteAllText(@"C:\temp\TLD\resources1.txt", str1);
        }

        public void Save()
        {
            Backup();

            LastSaved = DateTime.Now.Ticks;
            var bootSerialized = dynamicBoot.Serialize();
            SlotData.m_Dict["boot"] = EncryptString.Compress(bootSerialized);

            var greyJson = EncryptString.Decompress(SlotData.m_Dict["GreyMothersHouseA"]);
            System.IO.File.WriteAllText(@"C:\temp\TLD\GreyMotherHouseA.txt", greyJson);
            greyJson = System.IO.File.ReadAllText(@"C:\temp\TLD\GreyMotherHouseA.txt");
            SlotData.m_Dict["GreyMothersHouseA"] = EncryptString.Compress(greyJson);

            if (Boot.m_SceneName.Value != OriginalRegion)
            {
                Global.GameManagerData.SceneTransition.m_ForceNextSceneLoadTriggerScene = null;
            }
            Global.GameManagerData.SceneTransition.m_SceneSaveFilenameCurrent = Boot.m_SceneName.Value;
            Global.GameManagerData.SceneTransition.m_SceneSaveFilenameNextLoad = Boot.m_SceneName.Value;
            Global.PlayerManager.m_CheatsUsed = true;
            Afflictions.SerializeTo(Global);

            var globalSerialized = dynamicGlobal.Serialize();
            SlotData.m_Dict["global"] = EncryptString.Compress(globalSerialized);

            SlotData.m_Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var slotDataSerialized = dynamicSlotData.Serialize();
            File.WriteAllBytes(this.path, EncryptString.Compress(slotDataSerialized));
        }

        private void Backup()
        {
            var backupDirectory = Path.Combine(Path.GetDirectoryName(this.path), "backups");
            Directory.CreateDirectory(backupDirectory);

            var oldBackups = new DirectoryInfo(backupDirectory).GetFiles().OrderByDescending(x => x.LastWriteTime).Skip(MAX_BACKUPS);
            foreach (var file in oldBackups)
            {
                File.Delete(file.FullName);
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss", CultureInfo.InvariantCulture);
            var i = 1;
            var backupPath = Path.Combine(backupDirectory, timestamp + "-" + Path.GetFileName(this.path) + ".backup");
            while (File.Exists(backupPath))
            {
                backupPath = Path.Combine(backupDirectory, timestamp + "-" + Path.GetFileName(this.path) + "(" + i++ + ")" + ".backup");
            }
            File.Copy(this.path, backupPath);
        }
    }
}
