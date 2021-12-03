using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactSlot : MonoBehaviour {

    public GameObject artifact;

    public void addArtifact(Artifact artifact) {
        artifact.transform.SetParent(this.transform);
        this.artifact = artifact.gameObject;
        this.artifact.transform.localPosition = new Vector3(0, 0, 0);
        this.artifact.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public Artifact getArtifact() {
        if(this.artifact != null) return this.artifact.GetComponent<Artifact>();
        else return null;
    }
    
}
