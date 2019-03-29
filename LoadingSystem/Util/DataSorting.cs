using System.Collections.Generic;
using System.Linq;

namespace LoadingSystem.Util
{
	public class DataSorting
	{
		public DataSorting()
		{

		}



		public static List<int> MergeSort(List<int> unsorted)
		{
			if (unsorted.Count <= 1)
			{
				return unsorted;
			}

			var left = new List<int>();
			var right = new List<int>();

			var middle = unsorted.Count / 2;

			// Divide to left side
			for (int i = 0; i < middle; ++i)
			{
				left.Add(unsorted[i]);
			}

			// Divide to right side
			for (int i = middle; i < unsorted.Count; ++i)
			{
				right.Add(unsorted[i]);
			}

			left = MergeSort(left);
			right = MergeSort(right);

			return Merge(left, right);
		}



		private static List<int> Merge(List<int> left, List<int> right)
		{
			var result = new List<int>();

			do
			{
				if (left.Count > 0 && right.Count > 0)
				{
					if (left.First() <= right.First())
					{
						result.Add(left.First());
						left.Remove(left.First());
					}
					else
					{
						result.Add(right.First());
						right.Remove(right.First());
					}
				}
				else if (left.Count > 0)
				{
					result.Add(left.First());
					left.Remove(left.First());
				}
				else if (right.Count > 0)
				{
					result.Add(right.First());
					right.Remove(right.First());
				}
			}
			while (left.Count > 0 || right.Count > 0);

			return result;
		}
	}
}
