using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuBG : MonoBehaviour
{
        public GameObject[] objectToSpawn;
        public Collider2D mouseCollider2D;
        public float spawnInterval = 1f; //生成的時間
        public Vector2 spawnAreaMin;
        public Vector2 spawnAreaMax;
        private GameObject spawnedObject;

        private void Start()
        {
        InvokeRepeating("SpawnObject", 0f, spawnInterval);
        InvokeRepeating("SpawnObject", 0.2f, spawnInterval);
        
        }

        //生隨機位置和隨機方塊
    void SpawnObject()
        {
        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomY = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        Vector2 randomPosition = new Vector2(randomX, randomY);

        int i = Random.Range(0, 7);
        
        spawnedObject = Instantiate(objectToSpawn[i], randomPosition, Quaternion.identity);

        }

        void Update()
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            mouseCollider2D.transform.position = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0f);

    }
    }
