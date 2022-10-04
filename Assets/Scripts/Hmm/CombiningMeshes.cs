using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tests {
	[RequireComponent(typeof(MeshCollider))]
	public class CombiningMeshes : MonoBehaviour {
		[SerializeField] private GameObject m_tile;

		private int count;
		private List<CombineInstance> combineQueue = new List<CombineInstance>();

		private MeshCollider col;
		private void Awake() {
			col = gameObject.AddComponent<MeshCollider>();
		}

		private void AddPrefab(GameObject prefab, Vector3 pos) {
			GameObject ob = Instantiate(prefab, pos, Quaternion.identity);
			ob.transform.parent = transform;

			MeshCollider obCol = ob.GetComponent<MeshCollider>();
			CombineInstance combineItem = new() {
				mesh = obCol.sharedMesh,
				transform = ob.transform.localToWorldMatrix
			};
			combineQueue.Add(combineItem);
			Destroy(obCol);
		}

		private void FixedUpdate() {
			AddPrefab(m_tile, new Vector3(0, 0, count));
			count++;

			if (combineQueue.Count != 0) {
				Mesh mesh = new Mesh();
				if (col.sharedMesh) combineQueue.Add(new() {
					mesh = col.sharedMesh,
					transform = transform.localToWorldMatrix
				});
				CombineInstance[] combineItems = combineQueue.ToArray();

				mesh.CombineMeshes(combineItems);
				col.sharedMesh = mesh;
				combineQueue.Clear();
			}
		}
	}
}