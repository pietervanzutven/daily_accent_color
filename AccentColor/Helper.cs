using OEMSharedFolderAccessLib;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.Web.Http;

namespace AccentColor
{
    public class Helper
    {
        public Helper()
        {
            InitializeRegistryAccess();
        }

        public enum RegistryHives
        {
            HKEY_CLASSES_ROOT = 0,
            HKEY_CURRENT_MACHINE = 1,
            HKEY_CURRENT_USER = 2,
            HKEY_CURRENT_CONFIG = 5
        }

        public enum RegistryType
        {
            REG_SZ = 1,
            REG_BINARY = 3,
            REG_DWORD = 4,
            REG_MULTI_SZ = 7
        }

        public static COEMSharedFolder lrpc = new COEMSharedFolder();

        public static bool InitializeRegistryAccess()
        {
            if (lrpc.RPC_Init() == 0)
                return false;
            else
                return true;
        }

        public static string GetRegistryValue(RegistryHives Hive, string Path, string Key, RegistryType Type)
        {
            try
            {
                return lrpc.rget(Convert.ToUInt32(Hive), Path, Key, Convert.ToUInt32(Type));
            }
            catch
            {
                return "";
            }
        }

        public static void SetRegistryValue(RegistryHives Hive, string Path, string Key, string Value, RegistryType Type)
        {
            lrpc.rset(Convert.ToUInt32(Hive), Path, Key, Convert.ToUInt32(Type), Value, 0);
        }

        private static void ChangePhoneAccent(string HexValue, string CurrentAccent)
        {
            for (int i = 0; i <= 1; i++)
            {
                SetRegistryValue(RegistryHives.HKEY_CURRENT_MACHINE, $@"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme\Themes\{i}\Accents\{CurrentAccent}", "Color", HexValue, RegistryType.REG_DWORD);
                SetRegistryValue(RegistryHives.HKEY_CURRENT_MACHINE, $@"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme\Themes\{i}\Accents\{CurrentAccent}", "ComplementaryColor", HexValue, RegistryType.REG_DWORD);
            }
        }

        public static void PhoneAccentSetter(byte Red, byte Blue, byte Green)
        {
            var col = Color.FromArgb(255, Red, Green, Blue);
            var CurrentAccentHex = GetRegistryValue(RegistryHives.HKEY_CURRENT_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "CurrentAccent", RegistryType.REG_DWORD);
            var CurrentAccent = int.Parse(CurrentAccentHex, NumberStyles.HexNumber);
            ChangePhoneAccent(col.ToString().Replace("#", string.Empty), CurrentAccent.ToString());
            var regvalue = GetRegistryValue(RegistryHives.HKEY_CURRENT_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "AccentPalette", RegistryType.REG_BINARY);
            var array = regvalue.ToCharArray();

            array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(0), 24);
            array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(1), 25);
            array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(2), 26);
            array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(3), 27);
            array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(4), 28);
            array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(5), 29);

            var newpalette = string.Join("", array);
            SetRegistryValue(RegistryHives.HKEY_CURRENT_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "AccentPalette", newpalette, RegistryType.REG_BINARY);
            var CurrentAccentstr = CurrentAccent.ToString();
            var len = CurrentAccentstr.Length;
            for (int i = 0; i < (8 - len); i++)
            {
                CurrentAccentstr = "0" + CurrentAccentstr;
            }
            if (CurrentAccent != 1)
            {
                SetRegistryValue(RegistryHives.HKEY_CURRENT_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "CurrentAccent", "00000001", RegistryType.REG_DWORD);
                SetRegistryValue(RegistryHives.HKEY_CURRENT_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "CurrentAccent", CurrentAccentstr, RegistryType.REG_DWORD);
            }
            else
            {
                SetRegistryValue(RegistryHives.HKEY_CURRENT_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "CurrentAccent", "00000002", RegistryType.REG_DWORD);
                SetRegistryValue(RegistryHives.HKEY_CURRENT_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "CurrentAccent", CurrentAccentstr, RegistryType.REG_DWORD);
            }
        }

        public async Task<Color> GetColor()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(new Uri("https://api.myjson.com/bins/15n2pq"));
                string responsetext = await response.Content.ReadAsStringAsync();
                string[] colortext = JsonObject.Parse(responsetext).GetNamedString("color").Split(',');
                return Color.FromArgb(0xff, Convert.ToByte(colortext[0]), Convert.ToByte(colortext[1]), Convert.ToByte(colortext[2]));
            }
            catch
            {
                return new UISettings().GetColorValue(UIColorType.Accent);
            }
        }

        public void SetColor(Color color)
        {
            PhoneAccentSetter(Convert.ToByte(color.R), Convert.ToByte(color.B), Convert.ToByte(color.G));
        }
    }
}
