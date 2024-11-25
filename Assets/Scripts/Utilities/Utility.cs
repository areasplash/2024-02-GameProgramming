using UnityEngine;

using System;
using System.Collections.Generic;



// ====================================================================================================
// Utility
// ====================================================================================================

public static class Utility {

	// Methods

	public static int   ToInt  (float value) => (int  )(value * 10000.0f);
	public static float ToFloat(int   value) => (float)(value *  0.0001f);

	public static int  ToInt (bool value) => value ? 1 : 0;
	public static bool ToBool(int  value) => value != 0;

	public static int ToInt <T>(T   value) where T : Enum => Convert.ToInt32 (value);
	public static T   ToEnum<T>(int value) where T : Enum => (T)Enum.ToObject(typeof(T), value);

	public static int ToInt(Vector3 value, bool highPrecision = true) {
		int x, y, z;
		if (highPrecision) {
			x = (int)((Mathf.Clamp(value.x, -63.9375f, 63.9375f) + 63.9375f) * 16);
			y = (int)((Mathf.Clamp(value.y, -31.9375f, 31.9375f) + 31.9375f) * 16);
			z = (int)((Mathf.Clamp(value.z, -63.9375f, 63.9375f) + 63.9375f) * 16);
		}
		else {
			x = (int)((Mathf.Clamp(value.x, -255.75f, 255.75f) + 255.75f) * 4);
			y = (int)((Mathf.Clamp(value.y, -127.75f, 127.75f) + 127.75f) * 4);
			z = (int)((Mathf.Clamp(value.z, -255.75f, 255.75f) + 255.75f) * 4);
		}
		return (x << 21) | (y << 11) | (z <<  0);
	}
	public static Vector3 ToVector3(int value, bool highPrecision = true) {
		float x, y, z;
		if (highPrecision) {
			x = (((value >> 21) & 0x07FF) * 0.0625f) - 63.9375f;
			y = (((value >> 11) & 0x03FF) * 0.0625f) - 31.9375f;
			z = (((value >>  0) & 0x07FF) * 0.0625f) - 63.9375f;
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
		int x = (int)(Mathf.Repeat(angle.x, 360f) * 5.68889f);
		int y = (int)(Mathf.Repeat(angle.y, 360f) * 2.84444f);
		int z = (int)(Mathf.Repeat(angle.z, 360f) * 5.68889f);
		return (x << 21) | (y << 11) | (z <<  0);
	}
	public static Quaternion ToQuaternion(int value) {
		float x = ((value >> 21) & 0x07FF) * 0.175781f;
		float y = ((value >> 11) & 0x03FF) * 0.351562f;
		float z = ((value >>  0) & 0x07FF) * 0.175781f;
		return Quaternion.Euler(x, y, z);
	}

	public static int ToInt(Color value) {
		int r = (int)(Mathf.Clamp01(value.r) * 255);
		int g = (int)(Mathf.Clamp01(value.g) * 255);
		int b = (int)(Mathf.Clamp01(value.b) * 255);
		int a = (int)(Mathf.Clamp01(value.a) * 255);
		return (r << 24) | (g << 16) | (b <<  8) | (a <<  0); 
	}
	public static Color ToColor(int value) {
		float r = ((value >> 24) & 0xFF) * 0.00392157f;
		float g = ((value >> 16) & 0xFF) * 0.00392157f;
		float b = ((value >>  8) & 0xFF) * 0.00392157f;
		float a = ((value >>  0) & 0xFF) * 0.00392157f;
		return new Color(r, g, b, a);
	}

	public static int ToInt(Effect value) {
		int strength = (int)(Mathf.Clamp(value.strength, 0, 2047.9375f) * 16);
		int duration = (int)(Mathf.Clamp(value.duration, 0, 2047.9375f) * 16);
		int immunity = (int)value.immunity;
		return (strength << 17) | (duration <<  2) | (immunity <<  0);
	}
	public static Effect ToEffect(int value) {
		float        strength = ((value >> 17) & 0x7FFF) * 0.0625f;
		float        duration = ((value >>  2) & 0x7FFF) * 0.0625f;
		ImmunityType immunity = (ImmunityType)((value >> 0) & 0x3);
		return new Effect(strength, duration, immunity);
	}

	public static int ToInt(Status value) {
		int limit = (int)(Mathf.Clamp(value.limit, 0, 4095.9375f) * 16);
		int scale = (int)(Mathf.Clamp(value.value, 0, 4095.9375f) * 16);
		return (limit << 16) | (scale <<  0);
	}
	public static Status ToStatus(int value) {
		float limit = ((value >> 16) & 0xFFFF) * 0.0625f;
		float scale = ((value >>  0) & 0xFFFF) * 0.0625f;
		return new Status(limit, scale);
	}



	public static bool TryGetComponentInParent<T>(Transform transform,
	out T component) where T : Component {
		component = null;
		while (transform != null) {
			if (transform.TryGetComponent(out component)) return true;
			transform = transform.parent;
		}
		return false;
	}

	static bool IsIncluded(Transform source, Transform target) {
		if (source == target) return true;
		if (source) for (int i = 0; i < source.childCount; i++) {
			if (IsIncluded(source.GetChild(i), target)) return true;
		}
		return false;
	}



	static RaycastHit[] hits = new RaycastHit[256];

	static Collider collider;

	public static int GetLayerAtPoint(Vector3 point, Transform ignore = null) {
		int result = 0;
		int length = Physics.SphereCastNonAlloc(point, 0.5f, Vector3.up, hits, 0.0f);
		for (int i = 0; i < length; i++) {
			collider = hits[i].collider;
			if (collider.isTrigger) {
				if (ignore && IsIncluded(ignore, collider.transform)) continue;
				int layer = collider.gameObject.layer;
				if (result < layer) result = layer;
			}
		}
		return result;
	}

	public static int GetLayerMaskAtPoint(Vector3 point, Transform ignore = null) {
		int result = 0;
		int length = Physics.SphereCastNonAlloc(point, 0.5f, Vector3.up, hits, 0.0f);
		for (int i = 0; i < length; i++) {
			collider = hits[i].collider;
			if (collider.isTrigger) {
				if (ignore && IsIncluded(ignore, collider.transform)) continue;
				int layer = collider.gameObject.layer;
				result |= 1 << layer;
			}
		}
		return result;
	}

	static Creature creature;

	public static bool GetMatched(Vector3 point, float range, Predicate<Creature> match,
	ref Creature result) {
		result = null;
		float distance = float.MaxValue;
		int length = Physics.SphereCastNonAlloc(point, range, Vector3.up, hits, 0.0f);
		for (int i = 0; i < length; i++) if (!hits[i].collider.isTrigger) {
			if (hits[i].distance < distance) {
				if (TryGetComponentInParent(hits[i].collider.transform, out creature)) {
					if (match(creature)) {
						distance = hits[i].distance;
						result = creature;
					}
				}
			}
		}
		return distance < float.MaxValue;
	}

	public static int GetAllMatched(Vector3 point, float range, Predicate<Creature> match,
	ref List<Creature> result) {
		int count = 0;
		int length = Physics.SphereCastNonAlloc(point, range, Vector3.up, hits, 0.0f);
		for (int i = 0; i < length; i++) if (!hits[i].collider.isTrigger) {
			if (TryGetComponentInParent(hits[i].collider.transform, out creature)) {
				if (match(creature)) {
					result.Add(creature);
					count++;
				}
			}
		}
		return count;
	}

	static Structure structure;

	public static bool GetMatched(Vector3 point, float range, Predicate<Structure> match,
	ref Structure result) {
		result = null;
		float distance = float.MaxValue;
		int length = Physics.SphereCastNonAlloc(point, range, Vector3.up, hits, 0.0f);
		for (int i = 0; i < length; i++) if (!hits[i].collider.isTrigger) {
			if (hits[i].distance < distance) {
				if (TryGetComponentInParent(hits[i].collider.transform, out structure)) {
					if (match(structure)) {
						distance = hits[i].distance;
						result = structure;
					}
				}
			}
		}
		return distance < float.MaxValue;
	}

	public static int GetAllMatched(Vector3 point, float range, Predicate<Structure> match,
	ref List<Structure> result) {
		int count = 0;
		int length = Physics.SphereCastNonAlloc(point, range, Vector3.up, hits, 0.0f);
		for (int i = 0; i < length; i++) if (!hits[i].collider.isTrigger) {
			if (TryGetComponentInParent(hits[i].collider.transform, out structure)) {
				if (match(structure)) {
					result.Add(structure);
					count++;
				}
			}
		}
		return count;
	}
}
