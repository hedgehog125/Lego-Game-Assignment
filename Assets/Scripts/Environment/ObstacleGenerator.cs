using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Environment {
	[RequireComponent(typeof(MeshCollider))]
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(ObstaclesRenderer))]
	public class ObstacleGenerator : MonoBehaviour {
		[SerializeField] private List<ObstacleData> obstacles;
		[SerializeField] private int chunkLength;

		private Queue<Chunk> futureChunks = new Queue<Chunk>(); // Sometimes some extra chunks will need to be partially generated if something from a generated chunk extends into them
		private Chunk lastChunk;

		private ObstaclesRenderer ren;
		private void Awake() {
			ren = GetComponent<ObstaclesRenderer>();
			ren.Init(obstacles, chunkLength);

			lastChunk = new Chunk(chunkLength);

			
			ren.Render(new Chunk[] { GenerateChunk() });
		}

		private Chunk GenerateChunk() {
			Chunk chunk = futureChunks.Count == 0?
				new Chunk(chunkLength)
				: futureChunks.Dequeue()
			;

			chunk.tiles[GetTileIndex(1, 0)] = 1;

			lastChunk = chunk;
			return chunk;
		}
		private int[] GetFreeLanes(Chunk currentChunk, int startY, int lengthBack) {
			// The ground is viewed in 2D top-down. Y: 0 is the top of the current chunk, Y: ((chunkLength * 2) -  1) is the bottom of the last chunk
			int freeCount = 0;
			int[] freeLanes = new int[3];

			int endY = startY + lengthBack;
			for (int x = 0; x < 3; x++) {
				bool isClear = true;
				for (int y = startY; y < endY; y++) {
					Chunk chunk = y >= chunkLength? lastChunk : currentChunk;
					int index = GetTileIndex(x, y);
					if (GetTile(chunk, index) != -1) {
						isClear = false;
						break;
					}
				}
				if (isClear) {
					freeLanes[freeCount] = x;
					freeCount++;
				}
			}

			Array.Resize(ref freeLanes, freeCount);
			return freeLanes;
		}
		public static int GetTileIndex(int x, int y) {
			return (y * 3) + x;
		}
		private int GetTile(Chunk chunk, int x, int y) {
			return GetTile(chunk, GetTileIndex(x, y));
		}
		private int GetTile(Chunk chunk, int index) {
			int? tile = chunk.tiles[index];
			return tile == null? -1 : (int)tile;
		}

		public void LoopPosition(Vector3 moveAmount) {
			Debug.Log("TODO");
		}
	}

	public class Chunk {
		public int[] tiles; // -1 is empty, 0 is taken but not the origin of the obstacle, and the rest are the obstacle ids + 1
		public Chunk(int length) {
			tiles = new int[length * 3];
			Array.Fill(tiles, -1);
		}
	}
}
