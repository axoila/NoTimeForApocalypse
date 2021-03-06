﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerPhysics))]
[RequireComponent(typeof(PlayerWalk))]
public class PlayerDash : MonoBehaviour {

	public float speed;
	public float duration;
    public AnimationCurve dynamic;
    public AudioSource dashSound;
    public AudioClip dashClip;
    public DirectionalSprite[] dashAnims;

    private PlayerPhysics phys;
	private PlayerWalk walk;
    private PlayerHP health;

	// Use this for initialization
	void Awake(){
        phys = GetComponent<PlayerPhysics>();
        walk = GetComponent<PlayerWalk>();
        health = GetComponent<PlayerHP>();
        //controller.dash = this;
    }

    void LateUpdate(){
        Dash();
    }
	
	// Update is called once per frame
	public void Dash(){
        //upgrade 0 is dash
        if (Input.GetButtonDown("Action") && Time.timeScale > 0){
            if(StaticSafeSystem.current.hasUpgrade(0)){
                StartCoroutine(DashAction());
            }
            if(StaticSafeSystem.current.hasUpgrade(2)){
                StartCoroutine(InvulnerabilityAction());
            }
        }
    }
    private IEnumerator DashAction() {
        foreach(DirectionalSprite ds in dashAnims)
            ds.enabled = true;
        enabled = false;
        //walk.enabled = false;
        float timer = 0;
        while (timer < duration){
            phys.velocity = walk.direction * dynamic.Evaluate(timer / duration) * speed;
            phys.velocityGoal = phys.velocity;
            timer += Time.deltaTime;
            yield return null;
        }
        phys.velocity = Vector2.zero;
        //walk.enabled = true;
        enabled = true;
    }
    private IEnumerator InvulnerabilityAction(){
        if(!health) yield break;
        health.enabled = false;
        yield return new WaitForSeconds(duration);
        health.enabled = true;
    }
}
