using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OctreeNode {

	// Max objects allowed in a single OctreeNode
	public static int s_maxObjectLimit = 1;

	// Singleton for OctreeNode
	#region Singleton
	static OctreeNode _octreeRoot;
	static public OctreeNode OctreeRoot {
		get {
			if (_octreeRoot == null) {
				_octreeRoot = new OctreeNode(null, Vector3.zero, 15f, new List<OctreeItem>());
			}
			return _octreeRoot;
		}
	}

	[RuntimeInitializeOnLoadMethod]
	static bool Init() {
		return OctreeRoot == null;
	}
	#endregion

	GameObject octantGO;
	LineRenderer octantLineRenderer;

	// Current nodes dimension halved
	public float halfDimentionLength;

	// The center point of the node
	private Vector3 _pos;

	public OctreeNode parent;
	public List<OctreeItem> containedItems = new List<OctreeItem>();

	private OctreeNode[] _childrenNodes = new OctreeNode[8];

	public OctreeNode[] ChildrenNodes {
		get {
			return _childrenNodes;
		}
	}

	public void EraseChildrenNodes() {
		_childrenNodes = new OctreeNode[8];
	}

	// Constructor
	public OctreeNode(OctreeNode parent, Vector3 thisChildPos, float thisChildHalfLength, List<OctreeItem> potentialItems) {
		this.parent = parent;
		halfDimentionLength = thisChildHalfLength;
		_pos = thisChildPos;

		octantGO = new GameObject { hideFlags = HideFlags.HideInHierarchy };
		octantLineRenderer = octantGO.AddComponent<LineRenderer>();
		octantLineRenderer.material = (Material)Resources.Load("OctreeMaterial");

		FillCubeVisualizeCoords();

		foreach (OctreeItem item in potentialItems) {
			ProcessItem(item);
		}
	}

	public bool ProcessItem(OctreeItem item) {
		if (ContainsItemPosition(item.transform.position)) {
			if (ReferenceEquals(ChildrenNodes[0], null)) {
				PushItem(item);
				return true;
			} else {
				for (int i = 0; i < ChildrenNodes.Length; i++) {
					if (ChildrenNodes[i].ProcessItem(item)) {
						return true;
					}
				}
			}
		}

		return false;
	}

	private void PushItem(OctreeItem item) {
		if (!containedItems.Contains(item)) {
			containedItems.Add(item);
			item.my_ownerNodes.Add(this);
		}

		if (containedItems.Count > s_maxObjectLimit) {
			Split();
		}
	}

	private void Split() {
		for (int i = 0; i < containedItems.Count; i++) {
			containedItems[i].my_ownerNodes.Remove(this);
		}

		Vector3 positionVector = new Vector3(halfDimentionLength / 2, halfDimentionLength / 2, halfDimentionLength / 2);

		// Create the top 4 children
		for (int i = 0; i < 4; i++) {
			_childrenNodes[i] = new OctreeNode(this, _pos + positionVector, halfDimentionLength / 2, containedItems);
			positionVector = Quaternion.Euler(0f, -90f, 0f) * positionVector;
		}

		positionVector = new Vector3(halfDimentionLength / 2, -halfDimentionLength / 2, halfDimentionLength / 2);

		// Create bottom 4 children
		for (int i = 4; i < 8; i++) {
			_childrenNodes[i] = new OctreeNode(this, _pos + positionVector, halfDimentionLength / 2, containedItems);
			positionVector = Quaternion.Euler(0f, -90f, 0f) * positionVector;
		}

		containedItems.Clear();
	}

	public void AttemptReduceSubdivisions(OctreeItem escapedItem) {
		if (!ReferenceEquals(this, OctreeRoot) && !SiblingsChildrenNodesPresentTooManyItems()) {
			for (int i = 0; i < parent.ChildrenNodes.Length; i++) {
				parent.ChildrenNodes[i].KillNode(parent.ChildrenNodes.Where(x => !ReferenceEquals(x, this)).ToArray());
			}

			parent.EraseChildrenNodes();
		} else {
			containedItems.Remove(escapedItem);
			escapedItem.my_ownerNodes.Remove(this);
		}
	}

	private bool SiblingsChildrenNodesPresentTooManyItems() {
		List<OctreeItem> legacyItems = new List<OctreeItem>();

		foreach (OctreeNode sibling in parent.ChildrenNodes) {
			if (!ReferenceEquals(sibling.ChildrenNodes[0], null)) {
				return true;
			}

			legacyItems.AddRange(sibling.containedItems.Where(x => !legacyItems.Contains(x)));
		}

		if (legacyItems.Count > s_maxObjectLimit + 1) {
			return true;
		}

		return false;
	}

	private void KillNode(OctreeNode[] obsoleteSiblingNodes) {
		for (int i = 0; i < containedItems.Count; i++) {
			containedItems[i].my_ownerNodes = containedItems[i].my_ownerNodes.Except(obsoleteSiblingNodes).ToList();
			containedItems[i].my_ownerNodes.Remove(this);

			containedItems[i].my_ownerNodes.Add(parent);
			parent.containedItems.Add(containedItems[i]);
		}

		UnityEngine.Object.Destroy(octantGO);
	}

	public bool ContainsItemPosition(Vector3 itemPos) {
		if (itemPos.x > _pos.x + halfDimentionLength || itemPos.x < _pos.x - halfDimentionLength) {
			return false;
		}

		if (itemPos.y > _pos.y + halfDimentionLength || itemPos.y < _pos.y - halfDimentionLength) {
			return false;
		}

		if (itemPos.z > _pos.z + halfDimentionLength || itemPos.z < _pos.z - halfDimentionLength) {
			return false;
		}

		return true;
	}

	private void FillCubeVisualizeCoords() {
		Vector3[] cubeCoords = new Vector3[8];
		Vector3 corner = new Vector3(halfDimentionLength, halfDimentionLength, halfDimentionLength);

		for (int i = 0; i < 4; i++) {
			cubeCoords[i] = _pos + corner;
			corner = Quaternion.Euler(0f, 90f, 0f) * corner;
		}

		corner = new Vector3(halfDimentionLength, -halfDimentionLength, halfDimentionLength);
		for (int i = 4; i < 8; i++) {
			cubeCoords[i] = _pos + corner;
			corner = Quaternion.Euler(0f, 90f, 0f) * corner;
		}

		octantLineRenderer.useWorldSpace = true;
		octantLineRenderer.positionCount = 16;
		octantLineRenderer.startWidth = 0.1f;
		octantLineRenderer.endWidth = 0.1f;

		octantLineRenderer.SetPosition(0, cubeCoords[0]);
		octantLineRenderer.SetPosition(1, cubeCoords[1]);
		octantLineRenderer.SetPosition(2, cubeCoords[2]);
		octantLineRenderer.SetPosition(3, cubeCoords[3]);
		octantLineRenderer.SetPosition(4, cubeCoords[0]);
		octantLineRenderer.SetPosition(5, cubeCoords[4]);
		octantLineRenderer.SetPosition(6, cubeCoords[5]);
		octantLineRenderer.SetPosition(7, cubeCoords[1]);

		octantLineRenderer.SetPosition(8, cubeCoords[5]);
		octantLineRenderer.SetPosition(9, cubeCoords[6]);
		octantLineRenderer.SetPosition(10, cubeCoords[2]);
		octantLineRenderer.SetPosition(11, cubeCoords[6]);
		octantLineRenderer.SetPosition(12, cubeCoords[7]);
		octantLineRenderer.SetPosition(13, cubeCoords[3]);
		octantLineRenderer.SetPosition(14, cubeCoords[7]);
		octantLineRenderer.SetPosition(15, cubeCoords[4]);

	}
}
