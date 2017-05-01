﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Inklewriter;
using Inklewriter.Player;
using Inklewriter.Unity;
using System;

//namespace Inklewriter.Unity {
public class NPC : MonoBehaviour, IOptionHolder {

    public Transform textAnchor;

    private NpcUi ui;
    private TagTracker tags;

    public string dialogueFile;

    private GameObject inRange = null;
    private StoryPlayer player = null;
    private PlayChunk chunk = null;
    private int chunkProgress = 0;
    private bool inControl = false;
    private bool isChoosing = false;

    void Awake() {
        ui = GameObject.
            FindGameObjectWithTag("NPCDialogueBox").
            GetComponent<NpcUi>();
        tags = GameObject.FindWithTag("InfoTags").GetComponent<TagTracker>();
    }

    void Start() {
        var resource = Resources.Load(dialogueFile) as TextAsset;
        if (!resource) {
            Debug.LogWarning("Inklewriter story could not be loaded: " + dialogueFile);
            return;
        }
        string storyJson = resource.text;
        StoryModel model = StoryModel.Create(storyJson);
        player = new StoryPlayer(model, new UnityMarkupConverter());

    }

    private void Update() {
        if (Input.GetButtonDown("Fire1") && inRange != null) {
            if (inControl) {
                chunkProgress++;
                if (chunkProgress >= chunk.Paragraphs.Count) {
                    if (chunk.IsEnd) {
                        Release();
                    } else {
                        List<BlockContent<Option>> o = chunk.Options;
                        foreach(BlockContent<Option> i in o) {
                            List<string> con = i.Content.IfConditions;
                            foreach(string c in con) {
                                if (!tags.isTag(c)) {
                                    o.Remove(i);
                                    break;
                                }
                            }
                        }
                        string[] sOptions = new string[o.Count];
                        for(int i=0;i<o.Count;i++) {
                            sOptions[i] = o[i].Content.Text;
                        }
                        ui.Connect(this);
                        ui.ShowOptions(sOptions);
                        isChoosing = true;
                    }
                } else {
                    ui.Show("", chunk.Paragraphs[chunkProgress].Text);
                }
            } else {
                inRange.GetComponent<PlayerController>().enabled = false;
                inControl = true;
                chunk = player.CreateFirstChunk();
                chunkProgress = 0;
                ui.Show("", chunk.Paragraphs[chunkProgress].Text);
            }
        }

        if (Input.GetButtonDown("Cancel") && inRange != null) {
            Release();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Player") {
            ui.Show("[Action]", "");
            ui.SetActive(textAnchor == null ? transform : textAnchor, true);
            inRange = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.tag == "Player") {
            ui.SetActive(textAnchor == null ? transform : textAnchor, false);
            inRange = null;
        }
    }

    void Release() {
        inRange.GetComponent<PlayerController>().enabled = true;
        inControl = false;
        ui.Show("[Action]", "");
    }

    public void ChooseOption(int index) {
        if (!isChoosing)
            return;

        chunk = player.CreateChunkForOption(chunk.Options[index].Content);
        isChoosing = false;
        chunkProgress = 0;
        ui.Show("", chunk.Paragraphs[chunkProgress].Text);
    }
}