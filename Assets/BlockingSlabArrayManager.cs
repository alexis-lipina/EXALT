using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingSlabArrayManager : MonoBehaviour
{
    [SerializeField] float CyclePeriod;
    [SerializeField] float SlabHeight;
    [SerializeField] float RaiseDuration;
    [SerializeField] FiringChamberManager FCManager;

    [SerializeField] List<EnvironmentPhysics> Slabs;
    [SerializeField] List<TriggerVolume> SlabSafetyVolumes;
    List<SpriteRenderer> SlabBackWallShadowSprites;
    int NumberOfBlockingSlabs = 7;
    float TimeSinceStart = 0;
    List<int> SlabsToRaise;
    Vector3 SlabSprite0LocalTransform;
    Vector3 SlabSprite1LocalTransform;

    // Start is called before the first frame update
    void Start()
    {
        Slabs = new List<EnvironmentPhysics>();
        SlabSafetyVolumes = new List<TriggerVolume>();
        SlabBackWallShadowSprites = new List<SpriteRenderer>();
        SlabsToRaise = new List<int>();
        EnvironmentPhysics[] array = GetComponentsInChildren<EnvironmentPhysics>();
        for (int i = 0; i < array.Length; i++)
        {
            Slabs.Add(array[i]);
            SlabSafetyVolumes.Add(array[i].GetComponentInChildren<TriggerVolume>());
            SlabBackWallShadowSprites.Add(array[i].GetComponent<FiringChamberShadowedGradients>().GetBackWallShadow());
        }
        SlabSprite0LocalTransform = Slabs[0].GetComponentsInChildren<SpriteRenderer>()[0].gameObject.transform.localPosition;
        SlabSprite1LocalTransform = Slabs[0].GetComponentsInChildren<SpriteRenderer>()[1].gameObject.transform.localPosition;

        StartCoroutine(CycleSlabs());
    }

    IEnumerator CycleSlabs()
    {
        bool HasChangedSlabs = false;
        while (true)
        {
            //1 second 
            TimeSinceStart += Time.deltaTime;
            TimeSinceStart %= CyclePeriod;

            if (TimeSinceStart < 1.0f) // 1 second after blast, slabs stay up
            {
                UpdateBlockingSlabElevation(1.0f);
            }
            else if (TimeSinceStart < 1.0f + RaiseDuration) // lower blocks
            {
                UpdateBlockingSlabElevation((1.0f + RaiseDuration - TimeSinceStart) / RaiseDuration);
                HasChangedSlabs = false;
            }
            else if (TimeSinceStart < CyclePeriod - 1.0f - RaiseDuration) //hold at bottom
            {
                UpdateBlockingSlabElevation(0.0f);
                if (!HasChangedSlabs)
                {
                    SelectRandomSlabs();
                    HasChangedSlabs = true;
                }
            }
            else if (TimeSinceStart < CyclePeriod - 1.0f)
            {
                UpdateBlockingSlabElevation((TimeSinceStart - 5) / RaiseDuration);
            }
            else if (TimeSinceStart > CyclePeriod - 1.0f)
            {
                UpdateBlockingSlabElevation(1.0f);
            }
            
            yield return new WaitForEndOfFrame();
        }
    }

    void UpdateBlockingSlabElevation(float NormalizedElevation) // 0...1
    {
        foreach (int index in SlabsToRaise)
        {
            Slabs[index].TopHeight = NormalizedElevation * SlabHeight;
            Slabs[index].BottomHeight = NormalizedElevation * SlabHeight - SlabHeight;
            Slabs[index].GetComponentsInChildren<SpriteRenderer>()[0].gameObject.transform.localPosition = SlabSprite0LocalTransform + Vector3.up * SlabHeight * NormalizedElevation;
            Slabs[index].GetComponentsInChildren<SpriteRenderer>()[1].gameObject.transform.localPosition = SlabSprite1LocalTransform + Vector3.up * SlabHeight * NormalizedElevation;
            SlabBackWallShadowSprites[index].transform.localScale = new Vector3(0.125f, 128.0f * (1.0f-NormalizedElevation), 1.0f);
            //SlabBackWallShadowSprites[index].transform.localScale = new Vector3(01f, 0.1f, 0.1f);
            SlabBackWallShadowSprites[index].transform.position = new Vector3(
                SlabBackWallShadowSprites[index].transform.position.x, 
                26.0f + NormalizedElevation * 4.0f, 
                SlabBackWallShadowSprites[index].transform.position.z
                );
        }
    }

    void SelectRandomSlabs()
    {
        foreach (int index in SlabsToRaise)
        {
            FCManager.RemoveShieldingVolume(SlabSafetyVolumes[index]);
            Slabs[index].GetComponent<FiringChamberShadowedGradients>().ShowGradients();
        }
        SlabsToRaise.Clear();

        for (int i = 0; i < NumberOfBlockingSlabs; i++)
        {
            int thisIndex = Mathf.FloorToInt(Random.Range(0.0f, Slabs.Count - 0.001f));
            if (SlabsToRaise.Contains(thisIndex))
            {
                i--; // not the best pattern, but whatever
            }
            else
            {
                SlabsToRaise.Add(thisIndex);
            }
        }

        foreach (int index in SlabsToRaise)
        {
            FCManager.AddShieldingVolume(SlabSafetyVolumes[index]);
            Slabs[index].GetComponent<FiringChamberShadowedGradients>().HideGradients();
        }
    }
}
