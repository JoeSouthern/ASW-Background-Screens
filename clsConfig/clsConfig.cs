using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Windows.Forms;

public static class clsConfig
{
    private static DataSet DSoptions;
    private static string mConfigFileName;

    public static string ConfigFileName => mConfigFileName;

    public static void Initialize(string configFile)
    {
        mConfigFileName = configFile;
        DSoptions = new DataSet("Config");
        bool bStartTable = false;

        if (File.Exists(configFile))
        {
            DSoptions.ReadXml(configFile);
            if (DSoptions.Tables.Count == 0)
                bStartTable = true;
            else
                bStartTable = false;
        }

        if (bStartTable)
        {
            DataTable dt = new DataTable("ConfigValues");
            dt.Columns.Add("OptionName", typeof(string)).ColumnMapping = MappingType.Attribute;
            dt.Columns.Add("OptionValue", typeof(string)).ColumnMapping = MappingType.SimpleContent;
            DSoptions.Tables.Add(dt);
        }
    }

    public static void Store()
    {
        Store(mConfigFileName);
    }

    public static void OpenIniFile()
    {
        System.Diagnostics.Process.Start("Notepad.exe", ConfigFileName);
    }
    public static void Store(string configFile)
    {
        mConfigFileName = configFile;
        DSoptions.WriteXml(configFile);
    }

    public static T GetOptionAny<T>(string key, T defaultValue)
    {
        string val = GetOption(key);

        if (string.IsNullOrEmpty(val))
            return defaultValue;

        try
        {
            if (typeof(T) == typeof(bool))
            {
                return (T)(object)string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
            }
            if (typeof(T) == typeof(int))
            {
                return int.TryParse(val, out int i) ? (T)(object)i : defaultValue;
            }
            if (typeof(T) == typeof(float))
            {
                return float.TryParse(val, out float f) ? (T)(object)f : defaultValue;
            }
            if (typeof(T) == typeof(double))
            {
                return double.TryParse(val, out double d) ? (T)(object)d : defaultValue;
            }
            // Add other types as needed (DateTime, etc.)
        }
        catch
        {
            return defaultValue;
        }

        // Fallback for string or unhandled types
        return (T)(object)val;
    }
    public static bool GetOptionAsBool(string key, bool defaultValue = false)
    {
        var val = GetOption(key);
        if (string.IsNullOrEmpty(val)) return defaultValue;
        return string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
    }

    public static string GetOptionOrDefault(string key, string defaultValue)
    {
        var val = GetOption(key);
        return string.IsNullOrEmpty(val) ? defaultValue : val;
    }

    public static string GetOption(string optionName)
    {
        if (DSoptions.Tables.Count == 0)
        {
            return "";
        }

        DataView dv = DSoptions.Tables["ConfigValues"].DefaultView;
        dv.RowFilter = $"OptionName='{optionName}'";

        if (dv.Count > 0 && dv[0].Row.Table.Columns.Count > 1)
        {
            return dv[0][1].ToString();
        }
        return string.Empty;
    }

    public static void SetOption(string optionName, string optionValue)
    {
        DataView dv = DSoptions.Tables["ConfigValues"].DefaultView;
        dv.RowFilter = $"OptionName='{optionName}'";

        if (dv.Count > 0 && dv[0].Row.Table.Columns.Count > 1)
        {
            dv[0][1] = optionValue;
        }
        else
        {
            DataRow dr = DSoptions.Tables["ConfigValues"].NewRow();
            dr["OptionName"] = optionName;
            dr[1] = optionValue;
            DSoptions.Tables["ConfigValues"].Rows.Add(dr);
        }
    }

    public static void FillViaAdd(ComboBox yourAddObject, string baseName, int maxLook = 50)
    {
        yourAddObject.Items.Clear();
        for (int i = 0; i < maxLook; i++)
        {
            string tempData = GetOption(baseName + i.ToString().Trim());
            if (string.IsNullOrEmpty(tempData)) break;
            yourAddObject.Items.Add(tempData);
        }
        yourAddObject.Text = GetOption("Last" + baseName);
    }

    public static void AddViaCount(string yourText, ComboBox yourAddObject, string baseName, int maxLook = 50)
    {
        for (int i = 0; i < maxLook; i++)
        {
            string tempData = GetOption(baseName + i.ToString().Trim());
            if (string.IsNullOrEmpty(tempData))
            {
                SetOption(baseName + i.ToString().Trim(), yourText);
                Store();
                FillViaAdd(yourAddObject, baseName);
                return;
            }
        }
        MessageBox.Show($"Unable to place config data {yourText}", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static void SaveNonFormSettings(string strNoneFormName, string top,
        string left, string height, string width)
    {
        // Restore position
        SetOption(strNoneFormName + "_top", top);
        SetOption(strNoneFormName + "_left", left);

        SetOption(strNoneFormName + "_height", height);
        SetOption(strNoneFormName + "_width", width);
    }

    public static string[] GetNonFormSettings(string strNoneFormName)
    {
        string[] result = { "", "", "", "" };
        result[0] = GetOption(strNoneFormName + "_top");
        result[1] = GetOption(strNoneFormName + "_left");
        result[2] = GetOption(strNoneFormName + "_height");
        result[3] = GetOption(strNoneFormName + "_width");

        return result;
    }



    public static void SaveFormSettings(Form frm)
    {
        // Restore position
        SetOption(frm.Name + "_top", frm.Top.ToString());
        SetOption(frm.Name + "_left", frm.Left.ToString());

        SetOption(frm.Name + "_Height", frm.Height.ToString());
        SetOption(frm.Name + "_Width", frm.Width.ToString());
    }


    private static void DeleteRow(string optionName)
    {
        DataRow[] rowsToDelete = DSoptions.Tables["ConfigValues"]
            .Select($"OptionName = '{optionName}'");

        foreach (DataRow row in rowsToDelete)
        {
            row.Delete();
        }
    }

    public static void RemoveSettingsForForm(Form frm)
    {
        DeleteRow(frm.Name + "_top");
        DeleteRow(frm.Name + "_left");
        DeleteRow(frm.Name + "_Height");
        DeleteRow(frm.Name + "_Width");
    }

    public static void GetFormSettings(Form frm)
    {
        // Restore position
        string top = GetOption(frm.Name + "_top");
        string left = GetOption(frm.Name + "_left");

        if (int.TryParse(top, out int t))
            frm.Top = t;
        if (int.TryParse(left, out int l))
            frm.Left = l;

        // size
        string Height = GetOption(frm.Name + "_Height");
        string Width = GetOption(frm.Name + "_Width");

        if (int.TryParse(Height, out int h))
            frm.Height = h;
        if (int.TryParse(Width, out int w))
            frm.Width = w;
    }
}
