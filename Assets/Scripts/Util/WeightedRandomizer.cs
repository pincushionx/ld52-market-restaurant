using System.Collections.Generic;
using System;

namespace Pincushion.LD52 {
	public class WeightedRandomizer<T> {

		private List<WeightedRandomizerSegment<T>> weightTable = new List<WeightedRandomizerSegment<T>> ();
		private Random random;

		public WeightedRandomizer() {
			random = new Random();
		}

		public void AddSegment(T entry, int weight) {
            WeightedRandomizerSegment<T> segment = new WeightedRandomizerSegment<T>(entry, weight);

			for (int i = 0; i < weight; i++) {
				weightTable.Add(segment);
			}
		}

		public T GetRandomEntry() {
			int randomNumber = random.Next(0, weightTable.Count);
            WeightedRandomizerSegment<T> segment = weightTable[randomNumber];
			return segment.entry;
		}

		private class WeightedRandomizerSegment<T>
        {
			public T entry { get; set; }
			public int weight { get; set; }
		
			public WeightedRandomizerSegment (T entry, int weight) {
				this.entry = entry;
				this.weight = weight;
			}
		}
	}
}