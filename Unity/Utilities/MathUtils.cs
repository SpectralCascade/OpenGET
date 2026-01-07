using System.Collections;
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
		/// Map a value in a range to a different target range.
		/// </summary>
		public static float MapRange(float value, float min, float max, float target_min, float target_max)
        {
			float fraction = (Mathf.Clamp(value, min, max) - min) / (max - min);
			return target_min + (fraction * (target_max - target_min));
		}

		/// <summary>
		/// Returns the largest value by absolute comparison i.e. furthest from zero.
		/// </summary>
		public static float ExtremeMax(float a, float b)
		{
			return Mathf.Abs(a) >= Mathf.Abs(b) ? a : b;
		}
		
		/// <summary>
		/// Returns the smallest value by absolute comparison i.e. closest to zero.
		/// </summary>
		public static float ExtremeMin(float a, float b)
		{
			return Mathf.Abs(a) <= Mathf.Abs(b) ? a : b;
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
		/// Binary search. Returns the index of the item found in the array, or -1 if not found.
		/// Also outputs the "nearest" (last evaluated) index even if not found.
		/// Note: The array you provide MUST be sorted in ascending order.
		/// Optionally specify the low and high indices of the array to search between.
		/// If high is < 0, the last index of the array is used.
		/// </summary>
		public static int BinarySearch<T>(T[] sortedData, T target, out int nearest, int low = 0, int high = -1) where T : System.IComparable
		{
			if (high < 0)
			{
				// Consider the entire array
				high = sortedData.Length - 1;
				if (high < 0)
				{
					// Array is empty
					nearest = -1;
					return -1;
				}
			}

			if (high >= low)
			{
				int mid = low + (high - low) / 2;

				if (sortedData[mid].Equals(target))
				{
					nearest = mid;
					return mid;
				}

				// Account for edge cases
				if (high == low || mid == low)
				{
					// End of the line, mid point is best bet
					nearest = mid;
					return -1;
				}

				if (sortedData[mid].CompareTo(target) > 0)
				{
					return BinarySearch(sortedData, target, out nearest, low, mid - 1);
				}

				return BinarySearch(sortedData, target, out nearest, mid + 1, high);
			}

			nearest = low;
			return -1;
		}

		/// <summary>
		/// Combine bounds together by encapsulation.
		/// </summary>
		public static Bounds CombineBounds(Bounds[] bounds)
        {
			Bounds combined = new Bounds(bounds[0].center, Vector3.zero);
			for (int i = 0, counti = bounds.Length; i < counti; i++)
			{
				combined.Encapsulate(bounds[i]);
			}
			return combined;
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

			public static float InExponential(float x)
            {
				return x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10);
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
	/// Additional Bounds maths convenience methods.
	/// </summary>
	public static class BoundsExtensions
    {
		/// <summary>
		/// Calculate the vector difference to make two overlapping bounds no longer overlap, based on the centre.
		/// Returns Vector2.zero if the bounds are not overlapping.
		/// </summary>
		public static Vector2 GetOverlapDelta(this Bounds bounds, Bounds overlapped)
		{
			Vector2 delta = Vector2.zero;
			bool isLeftwards = bounds.center.x < overlapped.center.x;
			if ((overlapped.min.x >= bounds.min.x && overlapped.min.x <= bounds.max.x) || (bounds.min.x >= overlapped.min.x && bounds.min.x <= overlapped.max.x))
            {
				delta.x = isLeftwards ?
					overlapped.min.x - bounds.max.x : overlapped.max.x - bounds.min.x;
			}
			else if ((overlapped.max.x <= bounds.max.x && overlapped.max.x >= bounds.min.x) || (bounds.max.x >= overlapped.min.x && bounds.max.x <= overlapped.max.x))
			{
				delta.x = isLeftwards ?
					overlapped.min.x - bounds.max.x : overlapped.max.x - bounds.min.x;
			}

			bool isDownwards = bounds.center.y < overlapped.center.y;
			if ((overlapped.min.y >= bounds.min.y && overlapped.min.y <= bounds.max.y) || (bounds.min.y >= overlapped.min.y && bounds.min.y <= overlapped.max.y))
            {
				delta.y = isDownwards ?
					overlapped.min.y - bounds.max.y : overlapped.max.y - bounds.min.y;
			}
			else if ((overlapped.max.y <= bounds.max.y && overlapped.max.y >= bounds.min.y) || (bounds.max.y >= overlapped.min.y && bounds.max.y <= overlapped.max.y))
			{
				delta.y = isDownwards ?
					overlapped.min.y - bounds.max.y : overlapped.max.y - bounds.min.y;
			}

			return delta;
		}

	}

	/// <summary>
	/// RectTransform helper extensions.
	/// </summary>
	public static class RectTransformExtensions
	{

		/// <summary>
		/// Move the pivot point without modifying the position of the RectTransform.
		/// Assumes a rect transform with identical anchors!
		/// </summary>
		public static void SetOrigin(this RectTransform rectTransform, Vector2 origin)
		{
			// Calculate the offset applied by changing the pivot
			Vector3 offset = rectTransform.rotation * (((rectTransform.pivot - origin) * rectTransform.rect.size) * rectTransform.localScale);

			rectTransform.pivot = origin;
			rectTransform.localPosition -= offset;
		}

		/// <summary>
		/// Return the true bounds of a RectTransform in global space.
		/// Assumes that the transform has been updated. Don't call on Awake or Start.
		/// </summary>
		public static Bounds GetTrueBounds(this RectTransform rt, bool autoScale = true)
        {
			Vector3[] corners = new Vector3[4];
			rt.GetWorldCorners(corners);
			Vector2 scale = rt.lossyScale;
			if (!autoScale)
			{
				scale = Vector2.one;
			}
			Vector2 size = (corners[2] - corners[0]);
			Vector2 globalSize = Vector2.Scale(size, new Vector2(1f / scale.x, 1f / scale.y));

			return new Bounds(
				((Vector2)corners[0] + (size * 0.5f)),
				globalSize
			);
        }

	}

}
