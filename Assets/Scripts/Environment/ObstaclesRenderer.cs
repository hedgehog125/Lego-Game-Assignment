using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Environment {
	public class ObstaclesRenderer : MonoBehaviour {
		[SerializeField] private float m_tileSize;

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

		private List<ObstacleData> obstacles;
		private float chunkLength;

		private MeshCollider col;
		public void Init(List<ObstacleData> _obstacles, float _chunkLength) {
			col = GetComponent<MeshCollider>();

			obstacles = _obstacles;
			chunkLength = _chunkLength;
		}

		public void Render(Chunk[] chunks) {
			for (int i = 0; i < chunks.Length; i++) {
				for (int pos = 0; pos < chunks[i].tiles.Length; pos++) {
					int tileID = chunks[i].tiles[pos];
					if (tileID == -1 || tileID == 0) continue;
					tileID--;

					ObstacleData obstacle = obstacles[tileID];

					GameObject tile = Instantiate(obstacle.prefab, transform);
					tile.transform.localPosition = IndexToWorldPos(pos) + obstacle.offset;

					visibleObjects.Add(new VisibleObject(tile, tileID));
				}
			}

			UpdateCollider(); // TODO: only call when the end of the collider is near
		}

		private Vector3 IndexToWorldPos(int index) {
			return new Vector3(((index % 3) - 1) * m_tileSize, 0, -((Mathf.Floor(index / 3) - (chunkLength / 2)) * m_tileSize));
		}

		private void UpdateCollider() {
			Mesh mesh = new Mesh();

			CombineInstance[] combine = new CombineInstance[transform.childCount + 1];
			combine[0] = new CombineInstance {
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
	}
}