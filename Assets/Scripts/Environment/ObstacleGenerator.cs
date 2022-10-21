using Array = System.Array;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

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
		[SerializeField] private GeneratorData m_generatorData;

		private Queue<Chunk> futureChunks = new Queue<Chunk>(); // Sometimes some extra chunks will need to be partially generated if something from a generated chunk extends into them
		private Chunk lastChunk;
		private int lastSolidChunkID = -1; // The id of the furthest away chunk with collision
		private int lastVisibleChunkID = -1;
		private int generatingChunkID;

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


		private Chunk currentChunk; // So it doesn't have to be passed around during calls to generation related functions
		private Chunk GenerateChunk() {
			Chunk chunk = futureChunks.Count == 0?
				new Chunk(m_chunkLength, generatingChunkID)
				: futureChunks.Dequeue()
			;
			currentChunk = chunk;

			if (generatingChunkID == 0) {
				FillRect(0, m_chunkLength - m_generatorData.clearStartLength, 3, m_generatorData.clearStartLength, -1); // Reserve an empty rectangle where the player spawns
			}

			int rockCount = Random.Range(m_generatorData.rockCount.x, m_generatorData.rockCount.y);
			for (int i = 0; i < rockCount; i++) {
				GetRandomPos(out int x, out int y);
				if ((! IsLaneClearBehind(x, y, 3)) || (! IsLaneClearAhead(x, y, 3))) continue;

				PlaceIfEmpty(x, y, 0);
			}

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
				Chunk[] chunks = new Chunk[chunksNeeded];
				generatingChunkID = lastVisibleChunkID;
				for (int i = 0; i < chunksNeeded; i++) {
					generatingChunkID++;
					chunks[i] = GenerateChunk();
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

		private int[] GetFreeLanesAhead(int startY, int lengthForward) {
			return GetFreeLanesBehind(startY, -lengthForward);
		}
		private int[] GetFreeLanesBehind(int startY, int lengthBack) {
			// The ground is viewed in 2D top-down. Y: 0 is the top of the current chunk, Y: ((chunkLength * 2) -  1) is the bottom of the last chunk
			int freeCount = 0;
			int[] freeLanes = new int[3];

			for (int x = 0; x < 3; x++) {				
				if (IsLaneClearBehind(x, startY, lengthBack)) {
					freeLanes[freeCount] = x;
					freeCount++;
				}
			}

			Array.Resize(ref freeLanes, freeCount);
			return freeLanes;
		}
		private bool IsLaneClearAhead(int x, int startY, int lengthForward) {
			return IsLaneClearBehind(x, startY, -lengthForward);
		}
		private bool IsLaneClearBehind(int x, int startY, int lengthBack) {
			int dir = lengthBack > 0? 1 : -1;
			int endY = startY + lengthBack;

			for (int y = startY; dir == 1? y < endY : y > endY; y += dir) {
				if (! IsTileEmpty(x, y)) return false;
			}
			return true;
		}

		public static int GetTileIndex(int x, int y) {
			return (y * 3) + x;
		}
		private void IndexToXY(int index, out int x, out int y) {
			x = IndexToX(index);
			y = IndexToY(index);
		}
		private int IndexToX(int index) {
			return index % 3;
		}
		private int IndexToY(int index) {
			return Mathf.FloorToInt(index / 3);
		}
		private void GetChunkAndYPos(int y, out Chunk chunkOut, out int yOut) {
			GetChunkAndYPos(y, out chunkOut, out yOut, out int relativeChunkID);
		}
		private void GetChunkAndYPos(int y, out Chunk chunkOut, out int yOut, out int relativeChunkID) {
			if (y < 0) {
				int futureIndex = Mathf.FloorToInt((-y - 1) / m_chunkLength);
				chunkOut = futureIndex < futureChunks.Count?
					futureChunks.ElementAt(futureIndex) // A little slow but there shouldn't be that many items
					: null
				;
				relativeChunkID = futureIndex;
			}
			else if (y >= m_chunkLength * 2) {
				chunkOut = null;
				relativeChunkID = -Mathf.FloorToInt(y / m_chunkLength);
			}
			else if (y >= m_chunkLength) {
				chunkOut = lastChunk;
				relativeChunkID = -1;
			}
			else {
				chunkOut = currentChunk;
				relativeChunkID = 0;
			}
			yOut = y % m_chunkLength;
			if (yOut < 0) yOut += m_chunkLength;
		}
		private int GetTile(int index) {
			IndexToXY(index, out int x, out int y);
			return GetTile(x, y);
		}
		private int GetTile(int x, int y) {
			GetChunkAndYPos(y, out Chunk chunk, out y);
			if (chunk == null) return -1;

			return chunk.tiles[GetTileIndex(x, y)];
		}
		private bool IsTileEmpty(int index) {
			IndexToXY(index, out int x, out int y);
			return IsTileEmpty(x, y);
		}
		private bool IsTileEmpty(int x, int y) {
			int tileID = GetTile(x, y);
			return tileID == -1 || tileID == 1;
		}

		private void FillRect(int startX, int startY, int width, int height, int tileID) {
			int endX = startX + width;
			int endY = startY + height;
			for (int y = startY; y < endY; y++) {
				for (int x = startX; x < endX; x++) {
					PlaceTile(x, y, tileID);
				}
			}
		}
		private bool PlaceIfEmpty(int index, int tileID) {
			IndexToXY(index, out int x, out int y);
			return PlaceIfEmpty(GetTileIndex(x, y), tileID);
		}
		private bool PlaceIfEmpty(int x, int y, int tileID) {
			if (! IsTileEmpty(x, y)) return false;

			PlaceTile(x, y, tileID);
			return true;
		}
		private void PlaceTile(int index, int tileID) {
			IndexToXY(index, out int x, out int y);
			PlaceTile(x, y, tileID);
		}
		private void PlaceTile(int x, int y, int tileID) {
			GetChunkAndYPos(y, out Chunk chunk, out y, out int relativeChunkID);
			if (chunk == null) {
				if (relativeChunkID < 0) return; // Can't place in previous chunks

				for (int chunkID = futureChunks.Count; chunkID < relativeChunkID; chunkID++) { // Create any future chunks in between to get to the position in the queue for the new one that's needed
					chunk = new Chunk(m_chunkLength, chunkID); // This works because the needed chunk is made last
					futureChunks.Enqueue(chunk);
				}
			}

			chunk.tiles[GetTileIndex(x, y)] = tileID + 1;
		}

		private int GetPlayerChunkID() {
			return Mathf.FloorToInt(
				(m_player.transform.position.z + (playerCol.bounds.size.z / 2))
				/ (m_chunkLength * m_tileSize)
			);
		}
		private void GetRandomPos(out int x, out int y) {
			x = Random.Range(0, 3);
			y = Random.Range(0, m_chunkLength);
		}
		private int GetRandomIndex() {
			return Random.Range(0, m_chunkLength * 3);
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
