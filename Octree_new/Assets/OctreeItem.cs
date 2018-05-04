using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeItem : MonoBehaviour {

	public List<OctreeNode> my_ownerNodes = new List<OctreeNode>();
	private Vector3 _prevPos;

	// Use this for initialization
	void Start() {
		_prevPos = transform.position;
	}

	// Update is called once per frame
	void FixedUpdate() {
		if (transform.position != _prevPos) {
			RefreshOwners();
			_prevPos = transform.position;
		}
	}

	private void RefreshOwners() {
		OctreeNode.OctreeRoot.ProcessItem(this);

		List<OctreeNode> survivedNodes = new List<OctreeNode>();
		List<OctreeNode> obsoleteNodes = new List<OctreeNode>();

		for (int i = 0; i < my_ownerNodes.Count; i++) {
			if (!my_ownerNodes[i].ContainsItemPosition(transform.position)) {
				obsoleteNodes.Add(my_ownerNodes[i]);
			} else {
				survivedNodes.Add(my_ownerNodes[i]);
			}
		}

		my_ownerNodes = survivedNodes;

		for (int i = 0; i < obsoleteNodes.Count; i++) {
			obsoleteNodes[i].AttemptReduceSubdivisions(this);
		}
	}
}
