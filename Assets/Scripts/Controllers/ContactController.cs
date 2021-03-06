using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContactController : MonoBehaviour {
    private bool inBrawl;
    private GameObject brawlUI;
    private List<GameObject> mobsInBrawl;
    public int difficulty;
    public float sectionTimerDefault;
    private PlayerController playerController;
    public int mobCount;
    private List<KeyCode> keys;
    public int counter;
    private int keyIndex;
    private float timer;

    void Start() {
        difficulty = 4;
        inBrawl = false;
        brawlUI = transform.GetChild(0).gameObject;
        mobsInBrawl = new List<GameObject>();
        playerController = transform.parent.parent.gameObject.GetComponent<PlayerController>();
        keys = new List<KeyCode>() {
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.LeftArrow,
            KeyCode.RightArrow
        };
    }

    void Update() {
        mobCount = mobsInBrawl.Count;
    }
    public void updateBrawlStatus(GameObject mob) {
        mobsInBrawl.Add(mob);
        mobCount++;
        if(mobCount == 1 && !inBrawl) {
            startBrawl();
        };
    }

    public void startBrawl() {
        inBrawl = true;
        brawlUI.SetActive(true);
        StartCoroutine("manageMobsAttackingPlayer");
    }

    public IEnumerator manageMobsAttackingPlayer() {
        keyIndex = singleMobAttack();
        counter = 0;
        timer = 0f;
        while(mobsInBrawl.Count > 0) {
            if(timer > 1.5f) {
                handleBrawlComplete(-10f);
                yield return new WaitForSeconds (0.2f);
            }
            if(Input.GetKey(keys[keyIndex])) {
                handleBrawlComplete(-5f);
                yield return new WaitForSeconds (0.2f);
            } else if(Input.GetKey(keys[0]) || Input.GetKey(keys[1]) || Input.GetKey(keys[2]) || Input.GetKey(keys[3])) {
                handleBrawlComplete(-10f);
                yield return new WaitForSeconds (0.2f);
            }
            yield return new WaitForSeconds (0.01f);
            timer += 0.01f;
        }
        playerController.canMove = true;
        brawlUI.SetActive(false);
        inBrawl = false;
        yield return null;
    }

    private void handleBrawlComplete(float health) {
        completeStageOfBrawl(keyIndex);
        keyIndex = singleMobAttack();
        playerController.updateHealth(health);
    }

    private void completeStageOfBrawl(int keyIndex) {
        timer = 0;
        brawlUI.transform.GetChild(keyIndex).gameObject.SetActive(false);
        counter++;
        if(difficulty / counter == 1) {
            counter = 0;
            Destroy(mobsInBrawl[0]);
            mobsInBrawl.Remove(mobsInBrawl[0]);
        }
    }

    public int singleMobAttack() {
        var rand = new System.Random((int)System.DateTime.Now.Ticks);
        var keyIndex = rand.Next(0, keys.Count);
        brawlUI.transform.GetChild(keyIndex).gameObject.SetActive(true);
        return keyIndex;
    }
}
