using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Could've made this ad-hoc for the final boss, but decided to make it generic in case I become insane and want to make more content. Plus it shouldnt be too hard.
public class BossHealthBarManager : MonoBehaviour
{

    [SerializeField] ExaltText BossNameText;
    [SerializeField] Sprite OnHealth;
    [SerializeField] Sprite OffHealth;
    [SerializeField] Sprite FlashHealth;
    [SerializeField] Transform HealthChunkPrefab;

    private EntityPhysics bossEntityPhysics;
    public RectTransform healthLayoutGroup;
    public List<Image> healthSegments; // should have at least one, which is copied to make the rest
    private int displayedHealthAmount = 0;


    // Start is called before the first frame update
    void Start()
    {
    }

    public void SetupForBoss(EntityPhysics newBossEntityPhysics, string bossName)
    {
        bossEntityPhysics = newBossEntityPhysics;
        for (int i = 0; i < bossEntityPhysics.GetMaxHealth(); i++)
        {
            if (i+1 > healthSegments.Count)
            {
                healthSegments.Add(Instantiate(healthSegments[0], healthLayoutGroup).GetComponent<Image>());
            }
            healthSegments[i].sprite = OffHealth;
            healthSegments[i].color = new Color(1, 1, 1, 0);

        }
        BossNameText.SetText(bossName);
    }

    public void DramaticAppearance(float healthFlashInDuration)
    {
        StartCoroutine(DramaticAppearanceCoroutine(healthFlashInDuration));
    }
    
    private IEnumerator DramaticAppearanceCoroutine(float healthFlashInDuration)
    {
        displayedHealthAmount = 0;
        float timeBetweenFlashes = healthFlashInDuration / healthSegments.Count;
        for (int i = 0; i < bossEntityPhysics.GetMaxHealth(); i++)
        {
            if (bossEntityPhysics.GetCurrentHealth() >= i+1)
            {
                healthSegments[i].color = new Color(1, 1, 1, 1);
                healthSegments[i].sprite = FlashHealth;
                yield return new WaitForEndOfFrame();
                healthSegments[i].sprite = OnHealth;
                yield return new WaitForSeconds(timeBetweenFlashes);
                displayedHealthAmount++;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (bossEntityPhysics.GetCurrentHealth() < displayedHealthAmount)
        {
            StartCoroutine(TakeDamageFlash());
        }
    }

    private IEnumerator TakeDamageFlash()
    {
        int firstIndex = Mathf.Clamp(bossEntityPhysics.GetCurrentHealth(), 0, bossEntityPhysics.GetMaxHealth());
        int lastIndex = Mathf.Clamp(displayedHealthAmount, 0, bossEntityPhysics.GetMaxHealth());
        for (int i = firstIndex; i < lastIndex; i++)
        {
            healthSegments[i].sprite = FlashHealth;
        }
        yield return new WaitForEndOfFrame();
        for (int i = firstIndex; i < lastIndex; i++)
        {
            healthSegments[i].sprite = OffHealth;
        }
        displayedHealthAmount = firstIndex;

    }
}
