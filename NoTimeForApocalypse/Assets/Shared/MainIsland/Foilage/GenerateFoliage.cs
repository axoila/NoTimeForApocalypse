﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GenerateFoliage : MonoBehaviour {

    public GameObject protoScrub;
    public Vector2 amount = Vector2.one * 2;
    public Vector2 random;
    public Sprite[] sprites;

    public Rect offset_size; //offset AND size

    public bool alwaysDraw = false;
    public int seed = 0;

    private Collider2D coll;

    new Transform camera;
    Vector2 cameraHalfExtents;

    GameObject[,] existingObjects;
    //Vector2 atomSize; //distance between objects
    Vector2 startIndex = Vector2.zero;
    Vector2 indexPos;

    private void Awake() {
        coll = GetComponent<Collider2D>();
    }

    // Use this for initialization
    void Start() {
        camera = Camera.main.transform;
        cameraHalfExtents = new Vector2(Camera.main.aspect * Camera.main.orthographicSize, Camera.main.orthographicSize);
        
        //Stopwatch sw = Stopwatch.StartNew();
        Generate();
        //print(this + " generated scrubs in " + sw.ElapsedMilliseconds);
    }

    private void Update() {
        UpdateObjects();
    }

    void Generate() {
        existingObjects = new GameObject[Mathf.FloorToInt((cameraHalfExtents.x * 2 + offset_size.width) * amount.x / coll.bounds.size.x), 
                                        Mathf.FloorToInt((cameraHalfExtents.y * 2 + offset_size.height) * amount.y / coll.bounds.size.y)];
        
        indexPos = new Vector2(Mathf.Floor((camera.position.x - cameraHalfExtents.x - offset_size.x - coll.bounds.min.x) * amount.x / coll.bounds.size.x),
                Mathf.Floor((camera.position.y - cameraHalfExtents.y - offset_size.y - coll.bounds.min.y) * amount.y / coll.bounds.size.y));

        //atomSize = new Vector2(coll.bounds.size.x / amount.x, coll.bounds.size.y / amount.y);

        for (int x = 0; x < existingObjects.GetLength(0); x++) {
            for (int y = 0; y < existingObjects.GetLength(1); y++) {
                Random.InitState((int)(indexPos.x + x + (indexPos.y + y) * amount.x)+ seed);
                Vector2 indexBasePos = new Vector2(indexPos.x + x, indexPos.y + y);
                Vector2 basePos = new Vector2(indexBasePos.x * coll.bounds.size.x / amount.x + coll.bounds.min.x,
                        indexBasePos.y * coll.bounds.size.y / amount.y + coll.bounds.min.y) +
                        new Vector2(Random.Range(-random.x, random.x), Random.Range(-random.y, random.y));

                existingObjects[x, y] = Instantiate(protoScrub, basePos, transform.rotation,transform);
                existingObjects[x, y].SetActive(coll.OverlapPoint(existingObjects[x, y].transform.position) || alwaysDraw);
                if (existingObjects[x, y].activeSelf)
                    existingObjects[x, y].GetComponent<SpriteRenderer>().sprite = sprites[(int)(sprites.Length * Random.value)];
            }
        }
        {//so I can hide the comment
            //print(i);

            /*//delete existing children
            //for (int i = 0; i < transform.childCount; i++)
            //    Destroy(transform.GetChild(i).gameObject);

            for (float x = coll.bounds.center.x - coll.bounds.extents.x; x <= coll.bounds.center.x + coll.bounds.extents.x; x += coll.bounds.extents.x * 2 / (amount.x - 1)) {
                for (float y = coll.bounds.center.y + coll.bounds.extents.y; y >= coll.bounds.center.y - coll.bounds.extents.y; y -= coll.bounds.extents.y * 2 / (amount.y - 1)) {
                    Vector2 newPos = new Vector2(x+Random.Range(-random.x, random.x), y + Random.Range(-random.y, random.y));
                    if (coll.OverlapPoint(newPos)) {
                        GameObject newScrub = Instantiate(protoScrub, newPos, transform.rotation, transform);

                        newScrub.GetComponent<SpriteRenderer>().sprite = sprites[(int)(sprites.Length * Random.value)];
                    }
                }
            }*/
        }
    }

    void UpdateObjects() {
        Vector2 currentIndexPos = new Vector2(Mathf.Floor((camera.position.x - cameraHalfExtents.x - offset_size.x - coll.bounds.min.x) * amount.x / coll.bounds.size.x),
                Mathf.Floor((camera.position.y - cameraHalfExtents.y - offset_size.y - coll.bounds.min.y) * amount.y / coll.bounds.size.y));
        //print("saved: " + indexPos + " | current: " + currentIndexPos);
        while(currentIndexPos != indexPos){
            if(currentIndexPos.x > indexPos.x) {

                for (int y=0;y<existingObjects.GetLength(1); y++) {
                    Random.InitState((int)(indexPos.x + existingObjects.GetLength(0) + (indexPos.y + y) * amount.x) + seed);
                    existingObjects[(int)startIndex.x, iAdd((int)startIndex.y, y, existingObjects.GetLength(1))].transform.position =
                            indexToWorldPos(new Vector2(indexPos.x + existingObjects.GetLength(0), indexPos.y + y)) + 
                            new Vector2(Random.Range(-random.x, random.x), Random.Range(-random.y, random.y));
                    existingObjects[(int)startIndex.x, iAdd((int)startIndex.y, y, existingObjects.GetLength(1))]
                            .SetActive(coll.OverlapPoint(existingObjects[(int)startIndex.x, iAdd((int)startIndex.y, y, existingObjects.GetLength(1))].transform.position) || alwaysDraw);
                    existingObjects[(int)startIndex.x, iAdd((int)startIndex.y, y, existingObjects.GetLength(1))].transform.localScale = Vector3.one;
                    if(existingObjects[(int)startIndex.x, iAdd((int)startIndex.y, y, existingObjects.GetLength(1))].activeSelf)
                        existingObjects[(int)startIndex.x, iAdd((int)startIndex.y, y, existingObjects.GetLength(1))].GetComponent<SpriteRenderer>().sprite = sprites[(int)(sprites.Length * Random.value)];
                }
                indexPos.x++;
                startIndex.x = iAdd((int)startIndex.x, 1, existingObjects.GetLength(0));
            }
            if (currentIndexPos.x < indexPos.x) {
                indexPos.x--;
                for (int y = 0; y < existingObjects.GetLength(1); y++) {
                    Random.InitState((int)(indexPos.x + (indexPos.y + y) * amount.x) + seed);
                    existingObjects[iAdd((int)startIndex.x, -1, existingObjects.GetLength(0)), iAdd((int)startIndex.y, y, existingObjects.GetLength(1))].transform.position =
                            indexToWorldPos(new Vector2(indexPos.x, indexPos.y + y)) +
                            new Vector2(Random.Range(-random.x, random.x), Random.Range(-random.y, random.y));
                    existingObjects[iAdd((int)startIndex.x, -1, existingObjects.GetLength(0)), iAdd((int)startIndex.y, y, existingObjects.GetLength(1))]
                            .SetActive(coll.OverlapPoint(existingObjects[iAdd((int)startIndex.x, -1, existingObjects.GetLength(0)), iAdd((int)startIndex.y, y, existingObjects.GetLength(1))].transform.position) || alwaysDraw);
                    existingObjects[iAdd((int)startIndex.x, -1, existingObjects.GetLength(0)), iAdd((int)startIndex.y, y, existingObjects.GetLength(1))].transform.localScale = Vector3.one;
                    if (existingObjects[iAdd((int)startIndex.x, -1, existingObjects.GetLength(0)), iAdd((int)startIndex.y, y, existingObjects.GetLength(1))].activeSelf)
                        existingObjects[iAdd((int)startIndex.x, -1, existingObjects.GetLength(0)), iAdd((int)startIndex.y, y, existingObjects.GetLength(1))].GetComponent<SpriteRenderer>().sprite = sprites[(int)(sprites.Length * Random.value)];
                }
                
                startIndex.x = iAdd((int)startIndex.x, -1, existingObjects.GetLength(0));
            }

            if (currentIndexPos.y > indexPos.y) {
                
                for (int x = 0; x < existingObjects.GetLength(0); x++) {
                    Random.InitState((int)(indexPos.x + x + (indexPos.y + existingObjects.GetLength(1)) * amount.x) + seed);
                    existingObjects[iAdd((int)startIndex.x, x, existingObjects.GetLength(0)), (int)startIndex.y].transform.position =
                            indexToWorldPos(new Vector2(indexPos.x + x, indexPos.y + existingObjects.GetLength(1))) +
                            new Vector2(Random.Range(-random.x, random.x), Random.Range(-random.y, random.y));
                    existingObjects[iAdd((int)startIndex.x, x, existingObjects.GetLength(0)), (int)startIndex.y]
                            .SetActive(coll.OverlapPoint(existingObjects[iAdd((int)startIndex.x, x, existingObjects.GetLength(0)), (int)startIndex.y].transform.position) || alwaysDraw);
                    existingObjects[iAdd((int)startIndex.x, x, existingObjects.GetLength(0)), (int)startIndex.y].transform.localScale = Vector3.one;
                    if (existingObjects[iAdd((int)startIndex.x, x, existingObjects.GetLength(0)), (int)startIndex.y].activeSelf)
                        existingObjects[iAdd((int)startIndex.x, x, existingObjects.GetLength(0)), (int)startIndex.y].GetComponent<SpriteRenderer>().sprite = sprites[(int)(sprites.Length * Random.value)];
                }
                indexPos.y++;
                startIndex.y = iAdd((int)startIndex.y, 1, existingObjects.GetLength(1));
            }
            if (currentIndexPos.y < indexPos.y) {
                indexPos.y--;
                for (int x = 0; x < existingObjects.GetLength(0); x++) {
                    Random.InitState((int)(indexPos.x + x + indexPos.y * amount.x) + seed);
                    existingObjects[iAdd((int)startIndex.x, x, existingObjects.GetLength(0)), iAdd((int)startIndex.y, -1, existingObjects.GetLength(1))].transform.position =
                            indexToWorldPos(new Vector2(indexPos.x + x, indexPos.y)) +
                            new Vector2(Random.Range(-random.x, random.x), Random.Range(-random.y, random.y));
                    existingObjects[iAdd((int)startIndex.x, x, existingObjects.GetLength(0)), iAdd((int)startIndex.y, -1, existingObjects.GetLength(1))]
                            .SetActive(coll.OverlapPoint(existingObjects[iAdd((int)startIndex.x, x, existingObjects.GetLength(0)), iAdd((int)startIndex.y, -1, existingObjects.GetLength(1))].transform.position) || alwaysDraw);
                    existingObjects[iAdd((int)startIndex.x, x, existingObjects.GetLength(0)), iAdd((int)startIndex.y, -1, existingObjects.GetLength(1))].transform.localScale = Vector3.one;
                    if (existingObjects[iAdd((int)startIndex.x, x, existingObjects.GetLength(0)), iAdd((int)startIndex.y, -1, existingObjects.GetLength(1))].activeSelf)
                        existingObjects[iAdd((int)startIndex.x, x, existingObjects.GetLength(0)), iAdd((int)startIndex.y, -1, existingObjects.GetLength(1))].GetComponent<SpriteRenderer>().sprite = sprites[(int)(sprites.Length * Random.value)];
                }
                
                startIndex.y = iAdd((int)startIndex.y, -1, existingObjects.GetLength(1));
            }
        }
    }

    int iAdd(int index, int summand, int max) {
        return ((index + summand) % max + max) % max;
    }

    Vector2 indexToWorldPos(Vector2 iPos) {
        return new Vector2(iPos.x * coll.bounds.size.x / amount.x + coll.bounds.min.x,
                iPos.y * coll.bounds.size.y / amount.y + coll.bounds.min.y);
    }

    /*void OnDrawGizmos() {
        if (!Application.isPlaying)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(indexToWorldPos(indexPos), 1);
    }*/
}