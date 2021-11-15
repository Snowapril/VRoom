using System.Reflection;
using UnityEngine.UI;

public static class UISetExtensions
{
	private static readonly MethodInfo toggleSetMethod;
	private static readonly MethodInfo sliderSetMethod;
	private static readonly MethodInfo scrollbarSetMethod;

	private static readonly FieldInfo dropdownValueField;
	//private static readonly MethodInfo dropdownRefreshMethod;  // Unity 5.2 <= only

	static UISetExtensions()
	{
		// Find the Toggle's set method
		toggleSetMethod = FindSetMethod(typeof (Toggle));

		// Find the Slider's set method
		sliderSetMethod = FindSetMethod(typeof (Slider));

		// Find the Scrollbar's set method
		scrollbarSetMethod = FindSetMethod(typeof (Scrollbar));

		// Find the Dropdown's value field and its' Refresh method
		dropdownValueField = (typeof (Dropdown)).GetField("m_Value", BindingFlags.NonPublic | BindingFlags.Instance);
		//dropdownRefreshMethod = (typeof (Dropdown)).GetMethod("Refresh", BindingFlags.NonPublic | BindingFlags.Instance);  // Unity 5.2 <= only
	}

	public static void Set(this Toggle instance, bool value, bool sendCallback = false)
	{
		toggleSetMethod.Invoke(instance, new object[] {value, sendCallback});
	}

	public static void Set(this Slider instance, float value, bool sendCallback = false)
	{
		sliderSetMethod.Invoke(instance, new object[] {value, sendCallback});
	}

	public static void Set(this Scrollbar instance, float value, bool sendCallback = false)
	{
		scrollbarSetMethod.Invoke(instance, new object[] {value, sendCallback});
	}

	public static void Set(this Dropdown instance, int value)
	{
		dropdownValueField.SetValue(instance, value);
		instance.RefreshShownValue();
	}

	private static MethodInfo FindSetMethod(System.Type objectType)
	{
		var methods = objectType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
		for (var i = 0; i < methods.Length; i++)
		{
			if (methods[i].Name == "Set" && methods[i].GetParameters().Length == 2)
			{
				return methods[i];
			}
		}

		return null;
	}
}