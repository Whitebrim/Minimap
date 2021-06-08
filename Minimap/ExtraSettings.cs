using UnityEngine;

namespace Whitebrim.Minimap
{
	internal static class ExtraSettings
	{
		/// <summary>
		/// Use to get the selected index from a Combobox type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <returns></returns>
		public static int GetComboboxSelectedIndex(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				return Minimap.ExtraSettingsAPI_Traverse.Method("getComboboxSelectedIndex", new object[] { Minimap.Instance, SettingName }).GetValue<int>();
			return -1;
		}

		/// <summary>
		/// Use to get the selected item name from a Combobox type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <returns></returns>
		public static string GetComboboxSelectedItem(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				return Minimap.ExtraSettingsAPI_Traverse.Method("getComboboxSelectedItem", new object[] { Minimap.Instance, SettingName }).GetValue<string>();
			return "";
		}

		/// <summary>
		/// Use to get the list of items in a Combobox type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <returns></returns>
		public static string[] GetComboboxContent(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				return Minimap.ExtraSettingsAPI_Traverse.Method("getComboboxContent", new object[] { Minimap.Instance, SettingName }).GetValue<string[]>();
			return new string[0];
		}

		/// <summary>
		/// Use to get the current state of a Checkbox type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <returns></returns>
		public static bool GetCheckboxState(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				return Minimap.ExtraSettingsAPI_Traverse.Method("getCheckboxState", new object[] { Minimap.Instance, SettingName }).GetValue<bool>();
			return false;
		}

		/// <summary>
		/// Use to get the current value from a Slider type setting
		/// Minimap.Instance method returns the value of the slider rounded according to the mod's setting configuration
		/// </summary>
		/// <param name="SettingName"></param>
		/// <returns></returns>
		public static float GetSliderValue(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				return Minimap.ExtraSettingsAPI_Traverse.Method("getSliderValue", new object[] { Minimap.Instance, SettingName }).GetValue<float>();
			return 0;
		}

		/// <summary>
		/// Use to get the current value from a Slider type setting
		/// Minimap.Instance method returns the non-rounded value of the slider
		/// </summary>
		/// <param name="SettingName"></param>
		/// <returns></returns>
		public static float GetSliderRealValue(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				return Minimap.ExtraSettingsAPI_Traverse.Method("getSliderRealValue", new object[] { Minimap.Instance, SettingName }).GetValue<float>();
			return 0;
		}

		/// <summary>
		/// Use to get the keybind name for a Keybind type setting
		/// The returned name can be used with the MyInput functions to detect keypresses
		/// </summary>
		/// <param name="SettingName"></param>
		/// <returns></returns>
		public static string GetKeybindName(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				return Minimap.ExtraSettingsAPI_Traverse.Method("getKeybindName", new object[] { Minimap.Instance, SettingName }).GetValue<string>();
			return "";
		}

		/// <summary>
		/// Use to get the raw keybind for a Keybind type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <returns></returns>
		public static Keybind GetKeybind(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				return Minimap.ExtraSettingsAPI_Traverse.Method("getKeybind", new object[] { Minimap.Instance, SettingName }).GetValue<Keybind>();
			return null;
		}

		/// <summary>
		/// Use to get the main key for a Keybind type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <returns></returns>
		public static KeyCode GetKeybindMain(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				return Minimap.ExtraSettingsAPI_Traverse.Method("getKeybind_main", new object[] { Minimap.Instance, SettingName }).GetValue<KeyCode>();
			return KeyCode.None;
		}

		/// <summary>
		/// Use to get the alternate key for a Keybind type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <returns></returns>
		public static KeyCode GetKeybindAlt(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				return Minimap.ExtraSettingsAPI_Traverse.Method("getKeybind_alt", new object[] { Minimap.Instance, SettingName }).GetValue<KeyCode>();
			return KeyCode.None;
		}

		/// <summary>
		/// Use to get the text label of a setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <returns></returns>
		public static string GetSettingText(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				return Minimap.ExtraSettingsAPI_Traverse.Method("getSettingText", new object[] { Minimap.Instance, SettingName }).GetValue<string>();
			return "";
		}

		/// <summary>
		/// Use to set the selected index in a Combobox type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <param name="value"></param>
		public static void SetComboboxSelectedIndex(string SettingName, int value)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				Minimap.ExtraSettingsAPI_Traverse.Method("setComboboxSelectedIndex", new object[] { Minimap.Instance, SettingName, value }).GetValue();
		}

		/// <summary>
		/// Use to set the selected item in a Combobox type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <param name="value"></param>
		public static void SetComboboxSelectedItem(string SettingName, string value)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				Minimap.ExtraSettingsAPI_Traverse.Method("setComboboxSelectedItem", new object[] { Minimap.Instance, SettingName, value }).GetValue();
		}

		/// <summary>
		/// Use to set the items listed in a Combobox type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <param name="value"></param>
		public static void SetComboboxContent(string SettingName, string[] value)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				Minimap.ExtraSettingsAPI_Traverse.Method("setComboboxContent", new object[] { Minimap.Instance, SettingName, value }).GetValue();
		}

		/// <summary>
		/// Use to add an item to the items listed in a Combobox type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <param name="value"></param>
		public static void AddComboboxContent(string SettingName, string value)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				Minimap.ExtraSettingsAPI_Traverse.Method("addComboboxContent", new object[] { Minimap.Instance, SettingName, value }).GetValue();
		}

		/// <summary>
		/// Use to reset items listed in a Combobox type setting to the list set in the mod json
		/// </summary>
		/// <param name="SettingName"></param>
		public static void ResetComboboxContent(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				Minimap.ExtraSettingsAPI_Traverse.Method("resetComboboxContent", new object[] { Minimap.Instance, SettingName }).GetValue();
		}

		/// <summary>
		/// Use to set the current state of a Checkbox type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <param name="value"></param>
		public static void SetCheckboxState(string SettingName, bool value)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				Minimap.ExtraSettingsAPI_Traverse.Method("setCheckboxState", new object[] { Minimap.Instance, SettingName, value }).GetValue();
		}

		/// <summary>
		/// Use to set the value of a Slider type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <param name="value"></param>
		public static void SetSliderValue(string SettingName, float value)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				Minimap.ExtraSettingsAPI_Traverse.Method("setSliderValue", new object[] { Minimap.Instance, SettingName, value }).GetValue();
		}

		/// <summary>
		/// Use to set the current main keybinding for a Keybind type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <param name="value"></param>
		public static void SetKeybindMain(string SettingName, KeyCode value)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				Minimap.ExtraSettingsAPI_Traverse.Method("setKeybind_main", new object[] { Minimap.Instance, SettingName, value }).GetValue();
		}

		/// <summary>
		/// Use to set the current alternative keybinding for a Keybind type setting
		/// </summary>
		/// <param name="SettingName"></param>
		/// <param name="value"></param>
		public static void SetKeybindAlt(string SettingName, KeyCode value)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				Minimap.ExtraSettingsAPI_Traverse.Method("setKeybind_alt", new object[] { Minimap.Instance, SettingName, value }).GetValue();
		}

		/// <summary>
		/// Use to set the text label of a setting
		/// Minimap.Instance change is not stored in the save data
		/// </summary>
		/// <param name="SettingName"></param>
		/// <param name="value"></param>
		public static void SetText(string SettingName, string value)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				Minimap.ExtraSettingsAPI_Traverse.Method("setSettingsText", new object[] { Minimap.Instance, SettingName, value }).GetValue();
		}

		/// <summary>
		/// Use to reset a setting to its default value
		/// </summary>
		/// <param name="SettingName"></param>
		public static void ResetSetting(string SettingName)
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				Minimap.ExtraSettingsAPI_Traverse.Method("resetSetting", new object[] { Minimap.Instance, SettingName }).GetValue();
		}

		/// <summary>
		/// Use to reset all settings to their default values
		/// </summary>
		public static void ResetAllSettings()
		{
			if (Minimap.ExtraSettingsAPI_Loaded)
				Minimap.ExtraSettingsAPI_Traverse.Method("resetSettings", new object[] { Minimap.Instance }).GetValue();
		}
	}
}