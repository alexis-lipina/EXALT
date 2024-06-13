using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(SpriteRenderer))]
public class HailShard : MonoBehaviour
{
    private SpriteRenderer HailShardSprite;
    private Animation HailShardAnim;
    [SerializeField] string FallAnimationName;
    [SerializeField] bool CanKillPlayer = false; // I like the idea of these being threats that weaken you but which can't actually kill you. Makes them a perceived threat but not actually lethal in final phase
    [SerializeField] float FallAnimationDuration;
    [SerializeField] string ImpactAnimationName;
    [SerializeField] float ImpactAnimationDuration;
    public EntityPhysics playerPhys;


    // Start is called before the first frame update
    void Start()
    {
        HailShardSprite = GetComponent<SpriteRenderer>();
        //HailShardAnim = GetComponent<Animation>();
        StartCoroutine(PlayHailShard());
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator PlayHailShard()
    {
        //HailShardAnim.Play(FallAnimationName);
        //HailShardAnim.GetClip(FallAnimationName);
        //HailShardAnim.Play();
        yield return new WaitForSeconds(FallAnimationDuration);
        // TODO : could be cool if this gave you vertical camera jolt based on proximity
        Collider2D[] hitobjects = Physics2D.OverlapBoxAll(transform.position, new Vector2(2, 1.5f), 0);
        foreach (Collider2D hit in hitobjects)
        {
            EntityPhysics hitEntity = hit.gameObject.GetComponent<EntityPhysics>();
            if (hit.tag == "Friend")
            {
                if (!(hitEntity.GetCurrentHealth() == 1 && !CanKillPlayer)) // annoying to read. this just makes sure it doesnt kill the player if it shouldnt
                {
                    hitEntity.Inflict(1);
                }
                Debug.Log("Hit player!");
            }
        }
        float distanceToPlayer = ((Vector2)playerPhys.transform.position - (Vector2)transform.position).sqrMagnitude;
        Camera.main.GetComponent<CameraScript>().Jolt(Mathf.Lerp(3.0f, 0.1f, distanceToPlayer * 0.05f), Vector2.down);
        PlayerHandler playerhandler = (PlayerHandler)playerPhys.Handler;
        playerhandler.Vibrate(Mathf.Lerp(3.0f, 0.0f, distanceToPlayer * 0.05f), 0.1f);
        
        //HailShardAnim.Play(ImpactAnimationName);
        //HailShardAnim.GetClip(ImpactAnimationName);
        //HailShardAnim.Play();
        yield return new WaitForSeconds(ImpactAnimationDuration);
        Destroy(this.gameObject);
    }
}
