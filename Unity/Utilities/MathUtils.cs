﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

	/// <summary>
	/// Maths utility functions.
	/// </summary>
	public static class MathUtils
	{
		/// <summary>
		/// Takes in a value alongside a min+max and returns a normalized 0-1 value.
		/// EXAMPLE: (val:15, min:10, max:20) returns 0.5f
		/// </summary>
		public static float Normalize(float value, float min, float max)
		{
			float normalized = (value - min) / (max - min);
			return normalized;
		}

		/// <summary>
		/// Takes in a normalized value alongside a min-max and returns the unnormalized.
		/// EXAMPLE: (val:0.5f, min:10, max:20) returns 15
		/// </summary>
		public static float Denormalize(float normalized, float min, float max)
		{
			float unnormalized = normalized * (max - min) + min;
			return unnormalized;
		}

		/// <summary>
		/// Map a value in a range to a different range.
		/// </summary>
		public static float MapRange(float value, float min, float max, float target_min, float target_max)
        {
			float fraction = (Mathf.Clamp(value, min, max) - min) / (max - min);
			return target_min + (fraction * (target_max - target_min));
		}

		/// <summary>
		/// Returns the largest value by absolute comparison.
		/// </summary>
		public static float Extreme(float a, float b)
		{
			return Mathf.Abs(a) >= Mathf.Abs(b) ? a : b;
		}

		/// <summary>
		/// Compute the average point of a collection of points.
		/// Cheaper than computing the centroid, but weighted towards the largest group of points.
		/// </summary>
		public static Vector2 ComputeAveragePoint(Vector2[] points)
		{
			int totalPoints = points.Length;
			Vector2 combined = Vector2.zero;
			for (int i = 0; i < totalPoints; i++)
			{
				combined += points[i];
			}
			return combined / totalPoints;
		}

		/// <summary>
		/// Tween functions, useful for animations among other things.
		/// </summary>
		public static class Tween
		{
			public static float InOutCubic(float x)
			{
				return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
			}
		}

	}

	/// <summary>
	/// Additional Vector2 maths convenience methods.
	/// </summary>
	public static class Vector2Extensions
	{
		/// <summary>
		/// Clamp the individual X and Y components between the given min-max values.
		/// </summary>
		public static Vector2 Clamp(this Vector2 vec, float min = 0, float max = 1)
		{
			return new Vector2(Mathf.Clamp(vec.x, min, max), Mathf.Clamp(vec.y, min, max));
		}
	}

	/// <summary>
	/// RectTransform helper extensions.
	/// </summary>
	public static class RectTransformExtensions
	{

		/// <summary>
		/// Move the pivot point without modifying the position of the RectTransform
		/// </summary>
		public static void SetOrigin(this RectTransform rectTransform, Vector2 origin)
		{
			// Calculate the offset applied by changing the pivot
			Vector3 offset = rectTransform.rotation * (((rectTransform.pivot - origin) * rectTransform.rect.size) * rectTransform.localScale);

			rectTransform.pivot = origin;
			rectTransform.localPosition -= offset;
		}

	}

}
