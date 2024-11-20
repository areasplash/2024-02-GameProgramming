using UnityEngine;
using System;



// ====================================================================================================
// Utility
// ====================================================================================================

public static class Utility {

	// Methods

	/*
	float, bool, Enum, Vector3, Quaternion, Color <-> int 변환 함수

	float:      0.0001f 단위로 변환
	bool:       0 또는 1로 변환
	Enum:       int 값으로 변환
	Vector3:    1/4 또는 1/32 단위로 변환
	Quaternion: Euler 각도 변환 후 360/(2048-1) 또는 360/(1024-1) 단위로 변환
	Color:      1/128 단위로 변환
	*/

	public static int   ToInt  (float value) => (int  )(value * 10000.0f);
	public static float ToFloat(int   value) => (float)(value *  0.0001f);

	public static int  ToInt (bool value) => value ? 1 : 0;
	public static bool ToBool(int  value) => value != 0;

	public static int ToInt <T>(T   value) where T : Enum => Convert.ToInt32(value);
	public static T   ToEnum<T>(int value) where T : Enum => (T)Enum.ToObject(typeof(T), value);

	public static int ToInt(Vector3 value, bool highPrecision = false) {
		int x, y, z;
		if (highPrecision) {
			x = (int)((Mathf.Clamp(value.x, -31.96875f, 31.96875f) + 31.96875f) * 32);
			y = (int)((Mathf.Clamp(value.y, -15.96875f, 15.96875f) + 15.96875f) * 32);
			z = (int)((Mathf.Clamp(value.z, -31.96875f, 31.96875f) + 31.96875f) * 32);
		}
		else {
			x = (int)((Mathf.Clamp(value.x, -255.75f, 255.75f) + 255.75f) * 4);
			y = (int)((Mathf.Clamp(value.y, -127.75f, 127.75f) + 127.75f) * 4);
			z = (int)((Mathf.Clamp(value.z, -255.75f, 255.75f) + 255.75f) * 4);
		}
		return (x << 21) | (y << 11) | (z <<  0);
	}
	public static Vector3 ToVector3(int value, bool highPrecision = false) {
		float x, y, z;
		if (highPrecision) {
			x = (((value >> 21) & 0x07FF) * 0.03125f) - 31.96875f;
			y = (((value >> 11) & 0x03FF) * 0.03125f) - 15.96875f;
			z = (((value >>  0) & 0x07FF) * 0.03125f) - 31.96875f;
		}
		else {
			x = (((value >> 21) & 0x07FF) * 0.25f) - 255.75f;
			y = (((value >> 11) & 0x03FF) * 0.25f) - 127.75f;
			z = (((value >>  0) & 0x07FF) * 0.25f) - 255.75f;
		}
		return new Vector3(x, y, z);
	}

	public static int ToInt(Quaternion value) {
		Vector3 angle = value.eulerAngles;
		int x = (int)(Mathf.Repeat(angle.x, 360f) * 5.686112f);
		int y = (int)(Mathf.Repeat(angle.y, 360f) * 2.841667f);
		int z = (int)(Mathf.Repeat(angle.z, 360f) * 5.686112f);
		return (x << 21) | (y << 11) | (z <<  0);
	}
	public static Quaternion ToQuaternion(int value) {
		float x = ((value >> 21) & 0x07FF) * 0.1758671f;
		float y = ((value >> 11) & 0x03FF) * 0.3519062f;
		float z = ((value >>  0) & 0x07FF) * 0.1758671f;
		return Quaternion.Euler(x, y, z);
	}

	public static int ToInt(Color color) {
		int r = (int)(Mathf.Clamp01(color.r) * 128);
		int g = (int)(Mathf.Clamp01(color.g) * 128);
		int b = (int)(Mathf.Clamp01(color.b) * 128);
		int a = (int)(Mathf.Clamp01(color.a) * 128);
		return (r << 24) | (g << 16) | (b <<  8) | (a <<  0); 
	}
	public static Color ToColor(int value) {
		float r = ((value >> 24) & 0xFF) * 0.0078125f;
		float g = ((value >> 16) & 0xFF) * 0.0078125f;
		float b = ((value >>  8) & 0xFF) * 0.0078125f;
		float a = ((value >>  0) & 0xFF) * 0.0078125f;
		return new Color(r, g, b, a);
	}

	/*
	public static int ToInt(EffectData value) {
		int immunity = value.Immunity ? 1 : 0;
		int strength = Mathf.RoundToInt(Mathf.Clamp(value.Strength, 0, EffectData.Max) / 0.03125f);
		int duration = Mathf.RoundToInt(Mathf.Clamp(value.Duration, 0, EffectData.Max) / 0.03125f);
		return (immunity << 30) | (strength << 15) | (duration <<  0);
	}
	public static EffectData ToEffectData(int value) {
		bool  immunity = ((value >> 30) & 0x0003) != 0;
		float strength = ((value >> 15) & 0x7FFF) * 0.03125f;
		float duration = ((value >>  0) & 0x7FFF) * 0.03125f;
		return new EffectData(immunity, strength, duration);
	}

	public static int ToInt(StatusData value) {
		int strength = Mathf.RoundToInt(Mathf.Clamp(value.Value, 0, StatusData.Max) / 0.03125f);
		int duration = Mathf.RoundToInt(Mathf.Clamp(value.Limit, 0, StatusData.Max) / 0.03125f);
		return (strength << 16) | (duration <<  0);
	}
	public static StatusData ToStatusData(int value) {
		float strength = ((value >> 16) & 0xFFFF) * 0.03125f;
		float duration = ((value >>  0) & 0xFFFF) * 0.03125f;
		return new StatusData(strength, duration);
	}
	*/
}
