using System;
using System.Collections;
using System.Collections.Generic;
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

		private float playerStartOffset; // Because of the looping, the player being at Z: 0 might mean they're actually at the border of the chunk and not just the centre, this counters for it

		private ObstaclesRenderer ren;
		private Collider playerCol;
		private void Awake() {
			ren = GetComponent<ObstaclesRenderer>();
			playerCol = m_player.GetComponent<Collider>();
			ren.Init(obstacles, m_chunkLength, m_tileSize);

			lastChunk = new Chunk(m_chunkLength);
			playerStartOffset = (m_chunkLength * m_tileSize) / 2; // The player starts in the centre of the chunk

			Tick();
		}

		private void FixedUpdate() {
			Tick();
		}

		private void Tick() {
			GenerateChunksTick();
		}
		private void GenerateChunksTick() {
			int playerChunkID = Mathf.FloorToInt(
				(
					(m_player.transform.position.z + (playerCol.bounds.size.z / 2))
					+ playerStartOffset
				)
				/ (m_chunkLength * m_tileSize)
			);
			bool colliderUpdateNeeded = playerChunkID + m_physicsGenAhead > lastSolidChunkID;

			int chunksNeeded = (playerChunkID - lastVisibleChunkID) + m_visibleGenAhead;
			if (chunksNeeded > 0) {
				Chunk[] chunks = new Chunk[chunksNeeded];
				for (int i = 0; i < chunksNeeded; i++) {
					chunks[i] = GenerateChunk();
				}
				ren.Render(chunks, lastVisibleChunkID + 1, colliderUpdateNeeded);

				lastVisibleChunkID += chunksNeeded;
				if (colliderUpdateNeeded) { // The collider was just updated
					lastSolidChunkID = lastVisibleChunkID;
				}
			}
			else if (colliderUpdateNeeded) {
				ren.UpdateCollider();
				lastSolidChunkID = lastVisibleChunkID;
			}
		}

		private Chunk GenerateChunk() {
			Chunk chunk = futureChunks.Count == 0?
				new Chunk(m_chunkLength)
				: futureChunks.Dequeue()
			;

			chunk.tiles[GetTileIndex(0, 0)] = 1;
			chunk.tiles[GetTileIndex(2, 0)] = 1;
			//chunk.tiles[GetTileIndex(0, 5)] = 1;
			//chunk.tiles[GetTileIndex(2, 5)] = 1;

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
					Chunk chunk = y >= m_chunkLength? lastChunk : currentChunk;
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
			float movedBack = Mathf.Abs(moveAmount.z);
			float chunkWorldLength = m_chunkLength * m_tileSize;
			int chunksMovedBack = Mathf.FloorToInt(movedBack / chunkWorldLength);

			lastVisibleChunkID -= chunksMovedBack;
			lastSolidChunkID -= chunksMovedBack;
			//playerStartOffset = moved + ((chunksMovedBack + 0.5f) * chunkWorldLength);
			playerStartOffset = (movedBack % chunkWorldLength) + (lastVisibleChunkID * chunkWorldLength);
			Debug.Log(playerStartOffset);
			Debug.Log(lastVisibleChunkID);

			ren.LoopPosition(moveAmount);
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
