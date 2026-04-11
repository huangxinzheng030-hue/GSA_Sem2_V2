using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class FrictionChange : MonoBehaviour
{
    [Header("Trigger")]
    public string playerTag = "Player";

    [Header("Targets")]
    public Collider[] targetColliders;

    [Header("New Friction")]
    [Range(0f, 1f)] public float newDynamicFriction = 0.6f;
    [Range(0f, 1f)] public float newStaticFriction = 0.6f;
    public PhysicsMaterialCombine frictionCombine = PhysicsMaterialCombine.Average;

    [Header("Timing")]
    public float startDelay = 0f;
    public float duration = 3f;
    public bool restoreAfterDuration = true;

    [Header("Mode")]
    public bool triggerOnlyOnce = false;

    private bool hasTriggered = false;
    private bool isRunning = false;

    private class MaterialBackup
    {
        public Collider col;
        public PhysicsMaterial originalMaterial;
        public PhysicsMaterial runtimeMaterial;
    }

    private List<MaterialBackup> backups = new List<MaterialBackup>();

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (triggerOnlyOnce && hasTriggered) return;
        if (isRunning) return;

        StartCoroutine(ChangeFrictionRoutine());
    }

    private IEnumerator ChangeFrictionRoutine()
    {
        isRunning = true;
        hasTriggered = true;

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        backups.Clear();

        for (int i = 0; i < targetColliders.Length; i++)
        {
            Collider col = targetColliders[i];
            if (col == null) continue;

            MaterialBackup backup = new MaterialBackup();
            backup.col = col;
            backup.originalMaterial = col.sharedMaterial;

            PhysicsMaterial newMat = new PhysicsMaterial(col.name + "_RuntimeFrictionMat");
            newMat.dynamicFriction = newDynamicFriction;
            newMat.staticFriction = newStaticFriction;
            newMat.frictionCombine = frictionCombine;

            if (backup.originalMaterial != null)
            {
                newMat.bounciness = backup.originalMaterial.bounciness;
                newMat.bounceCombine = backup.originalMaterial.bounceCombine;
            }

            col.material = newMat;
            backup.runtimeMaterial = newMat;

            backups.Add(backup);
        }

        if (duration > 0f)
            yield return new WaitForSeconds(duration);

        if (restoreAfterDuration)
        {
            RestoreOriginalMaterials();
        }

        isRunning = false;
    }

    public void RestoreOriginalMaterials()
    {
        for (int i = 0; i < backups.Count; i++)
        {
            if (backups[i].col != null)
            {
                backups[i].col.material = backups[i].originalMaterial;
            }

            if (backups[i].runtimeMaterial != null)
            {
                Destroy(backups[i].runtimeMaterial);
            }
        }

        backups.Clear();
    }
}
