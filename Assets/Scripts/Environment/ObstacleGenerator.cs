using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Environment {
	[RequireComponent(typeof(MeshCollider))]
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(ObstaclesRenderer))]
	public class ObstacleGenerator : MonoBehaviour {
		[Header("Objects and references")]
		[SerializeField] private GameObject m_player;

		[Header("Chunk and obstacle info")]
		[SerializeField] private List<ObstacleData> obstacles;
		[SerializeField] private int m_chunkLength;
		[SerializeField] private float m_tileSize;

		[Header("Generation")]
		[SerializeField] private int m_visibleGenAhead;
		[SerializeField] private int m_physicsGenAhead;

		private Queue<Chunk> futureChunks = new Queue<Chunk>(); // Sometimes some extra chunks will need to be partially generated if something from a generated chunk extends into them
		private Chunk lastChunk;
		private int lastSolidChunkID = -1; // The id of the furthest away chunk with collision
		private int lastVisibleChunkID = -1;

		private ObstaclesRenderer ren;
		private Collider playerCol;
		private void Awake() {
			ren = GetComponent<ObstaclesRenderer>();
			playerCol = m_player.GetComponent<Collider>();
			ren.Init(obstacles, m_chunkLength, m_tileSize);

			lastChunk = new Chunk(m_chunkLength, -1);

			Tick();
		}

		private void FixedUpdate() {
			Tick();
		}


		private Chunk GenerateChunk(int chunkID) {
			Chunk chunk = futureChunks.Count == 0 ?
				new Chunk(m_chunkLength, chunkID)
				: futureChunks.Dequeue()
			;

			PlaceTile(chunk, 0, 0, 0);
			PlaceTile(chunk, 2, 0, 0);
			//GetFreeLanesAhead(chunk, 5, 6);

			lastChunk = chunk;
			return chunk;
		}


		private void Tick() {
			GenerateChunksTick();
		}
		private void GenerateChunksTick() {
			int playerChunkID = GetPlayerChunkID();
			bool colliderUpdateNeeded = playerChunkID + m_physicsGenAhead > lastSolidChunkID;

			int chunksNeeded = (playerChunkID - lastVisibleChunkID) + m_visibleGenAhead;
			if (chunksNeeded > 0) {
				Debug.Log(chunksNeeded);
				Chunk[] chunks = new Chunk[chunksNeeded];
				int generatingChunkID = lastVisibleChunkID;
				for (int i = 0; i < chunksNeeded; i++) {
					generatingChunkID++;
					chunks[i] = GenerateChunk(generatingChunkID);
				}
				ren.Render(chunks, lastVisibleChunkID + 1, colliderUpdateNeeded);
				lastVisibleChunkID = generatingChunkID;

				if (colliderUpdateNeeded) { // The collider was just updated
					lastSolidChunkID = lastVisibleChunkID;
				}
			}
			else if (colliderUpdateNeeded) {
				ren.UpdateCollider();
				lastSolidChunkID = lastVisibleChunkID;
			}
		}

		private int[] GetFreeLanesAhead(Chunk currentChunk, int startY, int lengthForward) {
			return GetFreeLanesBehind(currentChunk, startY, -lengthForward);
		}
		private int[] GetFreeLanesBehind(Chunk currentChunk, int startY, int lengthBack) {
			// The ground is viewed in 2D top-down. Y: 0 is the top of the current chunk, Y: ((chunkLength * 2) -  1) is the bottom of the last chunk
			int freeCount = 0;
			int[] freeLanes = new int[3];

			int dir = lengthBack > 0? 1 : -1;
			int endY = startY + lengthBack;
			for (int x = 0; x < 3; x++) {
				bool isClear = true;
				for (int y = startY; (dir == 1? y < endY : y > endY); y += dir) {
					Chunk chunk;
					if (y < 0) {
						chunk = futureChunks.ElementAt(Mathf.FloorToInt((-y - 1) / m_chunkLength)); // A little slow but there shouldn't be that many items
					}
					else if (y >= m_chunkLength) {
						chunk = lastChunk;
					}
					else {
						chunk = currentChunk;
					}

					if (GetTile(chunk, x, y) != -1) {
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
			if (chunk == null) return -1;
			if (index < 0 || index >= chunk.tiles.Length) return -1;

			return chunk.tiles[index];
		}

		private void PlaceTile(Chunk chunk, int x, int y, int tileID) {
			PlaceTile(chunk, GetTileIndex(x, y), tileID);
		}
		private void PlaceTile(Chunk chunk, int index, int tileID) {
			chunk.tiles[index] = tileID + 1;
		}

		private int GetPlayerChunkID() {
			return Mathf.FloorToInt(
				(m_player.transform.position.z + (playerCol.bounds.size.z / 2))
				/ (m_chunkLength * m_tileSize)
			);
		}

		public void LoopPosition(Vector3 moveAmount) {
			{
				int playerChunkID = GetPlayerChunkID();
				int countInFront = lastVisibleChunkID - playerChunkID;

				int diff = lastVisibleChunkID - lastSolidChunkID;
				lastVisibleChunkID = countInFront;
				lastSolidChunkID = lastVisibleChunkID - diff;
			}

			ren.LoopPosition(moveAmount);
		}
	}

	public class Chunk {
		public int[] tiles; // -1 is empty, 0 is taken but not the origin of the obstacle, and the rest are the obstacle ids + 1
		public int id;

		public Chunk(int _length, int _id) {
			tiles = new int[_length * 3];
			Array.Fill(tiles, -1);

			id = _id;
		}
	}
}
