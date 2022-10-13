using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Environment {
	public class ObstaclesRenderer : MonoBehaviour {
		[Header("Collision")]
		[SerializeField] private Mesh m_groundMesh;
		[SerializeField] private Transform m_groundTran;

		private class VisibleObject {
			public GameObject ob;
			public int tileID;

			public VisibleObject(GameObject _ob, int _tileID) {
				ob = _ob;
				tileID = _tileID;
			}
		}
		private List<VisibleObject> visibleObjects = new List<VisibleObject>();
		private bool movedBack;

		private List<ObstacleData> obstacles;
		private int chunkLength;
		private float tileSize;

		private MeshCollider col;
		public void Init(List<ObstacleData> _obstacles, int _chunkLength, float _tileSize) {
			col = GetComponent<MeshCollider>();

			obstacles = _obstacles;
			chunkLength = _chunkLength;
			tileSize = _tileSize;
		}

		public void Render(Chunk[] chunks, int startChunkID, bool shouldUpdateCollider) {
			Vector3 counterOffset = Vector3.zero;
			if (shouldUpdateCollider) ResetContainerLoopPos(); // If it's going to be reset, it needs to be done before the new objects so they don't get offset
			else counterOffset.z = -transform.position.z; // If it's not being reset yet, the offset still needs to be accounted for

			for (int i = 0; i < chunks.Length; i++) {
				int chunkID = i + startChunkID;
				for (int pos = 0; pos < chunks[i].tiles.Length; pos++) {
					int tileID = chunks[i].tiles[pos];
					if (tileID == -1 || tileID == 0) continue;
					tileID--;

					ObstacleData obstacle = obstacles[tileID];

					GameObject tile = Instantiate(obstacle.prefab, transform);
					tile.transform.localPosition = IndexToWorldPos(pos, chunkID) + obstacle.offset + counterOffset;

					visibleObjects.Add(new VisibleObject(tile, tileID));
				}
			}

			if (shouldUpdateCollider) UpdateCollider();
		}

		private Vector3 IndexToWorldPos(int index, int chunkID) {
			return new Vector3(
				((index % 3) - 1) * tileSize,
				0,
				(chunkID * chunkLength * tileSize) - ((Mathf.Floor(index / 3) - (chunkLength / 2)) * tileSize)
			);
		}

		public void UpdateCollider() {
			Debug.Log("A");
			ResetContainerLoopPos();

			Mesh mesh = new Mesh();

			CombineInstance[] combine = new CombineInstance[transform.childCount + 1];
			combine[0] = new CombineInstance { // This will get temporarilly offset after a loop but before the next collider update, but since the visibile mesh is fine and it's quite big, it should be fine
				mesh = m_groundMesh,
				transform = m_groundTran.localToWorldMatrix
			};
			int i = 0;
			foreach (VisibleObject visOb in visibleObjects) {
				combine[i + 1] = new CombineInstance {
					mesh = obstacles[visOb.tileID].collider,
					transform = visOb.ob.transform.localToWorldMatrix
				};
				i++;
			}
			mesh.CombineMeshes(combine);

			col.sharedMesh = mesh;
		}

		public void LoopPosition(Vector3 moveAmount) {
			transform.position += moveAmount;
			movedBack = true;
		}
		private void ResetContainerLoopPos() {
			if (movedBack) {
				Vector3 moveAmount = transform.position;
				transform.position = Vector3.zero;
				foreach (VisibleObject visOb in visibleObjects) {
					visOb.ob.transform.position += moveAmount;
				}
				movedBack = false;
			}
		}
	}
}