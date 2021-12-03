using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactManager : MonoBehaviour {

    public List<ArtifactSlot> artifactSlots = new List<ArtifactSlot>();
    
    public void eventTrigger(ArtifactTriggerType type, Unit unit, int value = 1) {
        foreach(ArtifactSlot artifactSlot in this.artifactSlots) {
            Artifact artifact = artifactSlot.getArtifact();
            if(artifact != null) artifact.eventTrigger(type, unit, value);
        }
    }

    public void addArtifact(Artifact artifact) {
        foreach(ArtifactSlot artifactSlot in this.artifactSlots) {
            Artifact a = artifactSlot.getArtifact();
            if(a == null) artifactSlot.addArtifact(artifact);
        }
    }

    public void resetTriggers() {
        foreach(ArtifactSlot artifactSlot in this.artifactSlots) {
            Artifact artifact = artifactSlot.getArtifact();
            if(artifact != null) artifact.trigger.isTriggered = false;
        }
    }
}
