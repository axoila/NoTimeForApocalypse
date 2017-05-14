﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : Hitable {

	public float walkSpeed = 1;
	public GameObject hitPrefab;
	public GameObject hitOrigin;
    [NonSerialized] public bool slowed = false;

	Rigidbody2D rigid;

	float direction = 0;
	float hitCooldown = 0;

    public DirectionalSprite defaultAnim;
    public DirectionalSprite hitAnim;

    private HpDisplay hpDisplay;
    public int maxHp = 7;
    private int hp = -1;
    public float iFrames = 1; //actually in seconds but y'know
    private float iTimer;

    public Collider2D land;

    public PauseMenu pause;
    public AudioSource attackAudio;
    public AudioSource walkAudio;
    public AudioClip defaultWalk;
    public AudioClip mudWalk;

	// Use this for initialization
	void Awake () {
		rigid = GetComponent<Rigidbody2D> ();
        hpDisplay = GameObject.FindWithTag("HpDisplay").GetComponent<HpDisplay>();

        if (hp < 0)
            hp = maxHp;
	}
	
	// Update is called once per frame
	void Update () {
		Movement();
		Punch();

        iTimer -= Time.deltaTime;
	}

    private void FixedUpdate(){
        slowed = false;
    }

    private void OnDisable() {
        rigid.velocity = Vector2.zero;
    }

    private void Movement() {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input.magnitude > 1)
            input.Normalize();
        input.Scale(new Vector2(1, 2f/3f));
		Vector2 velocity = input * walkSpeed;
        if(Input.GetButton("God")) velocity *= 10; // speed cheat
		rigid.velocity = slowed?velocity*0.4f:velocity;

        walkAudio.mute = velocity == Vector2.zero;
		if (velocity != Vector2.zero) { //this used to drive the hit direction
			direction = Mathf.Atan2 (-velocity.x, velocity.y);
		}

        if (!land.OverlapPoint(transform.position) && hp >= 0 && !Input.GetButton("God")) {
            Die("You drowned");
        }

        if(slowed && walkAudio.clip != mudWalk){
            int pos = walkAudio.timeSamples;
            walkAudio.clip = mudWalk;
            walkAudio.timeSamples = pos;
            try{
                walkAudio.Play();
            } catch {
                walkAudio.time = 0;
                walkAudio.Play();
            }
        } 
        if(!slowed && walkAudio.clip != defaultWalk){
            int pos = walkAudio.timeSamples;
            walkAudio.clip = defaultWalk;
            walkAudio.timeSamples = pos;
            try{
                walkAudio.Play();
            } catch {
                walkAudio.time = 0;
                walkAudio.Play();
            }
        }

        //print(walkAudio.time);
	}

	private void Punch() {
		hitCooldown -= Time.deltaTime;
		if (Input.GetButtonDown ("Fire1") && hitCooldown < 0) {
            GameObject closest = getClosestWithTag("Enemy");
            Vector2 direction = closest==null ? Vector2.zero : (Vector2)(closest.transform.position - hitOrigin.transform.position);
            float angle = direction==Vector2.zero?this.direction*Mathf.Rad2Deg:getAngle(direction);
			GameObject newHit = Instantiate(hitPrefab, hitOrigin.transform.position, Quaternion.AngleAxis(angle, Vector3.forward));
            newHit.GetComponent<HitParticle>().source = gameObject;
			hitCooldown = 0.5f;

            defaultAnim.enabled = false;
            hitAnim.angle = -angle * Mathf.Deg2Rad;
            hitAnim.enabled = true;

            attackAudio.pitch = UnityEngine.Random.Range(0.9f, 1.3f);
            attackAudio.Play();
        }
	}

    public override void Hit(GameObject source, float damage = 0, float directionAngle = 0) {
        if (hp < 0) return;
        if (iTimer < 0) {
            iTimer = iFrames;
            hp -= (int)damage;
            hpDisplay.setHP(hp);
        }
        if (hp <= 0)
            Die();
    }

    public GameObject getClosestWithTag(string tag) {
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float distance = -1;
        foreach(GameObject go in objectsWithTag) {
            if(closest == null || (go.transform.position - transform.position).magnitude < distance) {
                closest = go;
                distance = (go.transform.position - transform.position).magnitude;
            }
        }
        return distance<20?closest:null;
    }

    float getAngle(Vector2 fromVector2) {
        return Mathf.Atan2(-fromVector2.x, fromVector2.y) * Mathf.Rad2Deg ;
    }

    public void Die(string deathMessage = "you died") {
        hp = -1;
        hpDisplay.setHP(0);
        pause.death = true;
        pause.Pause(deathMessage);
    }
}
