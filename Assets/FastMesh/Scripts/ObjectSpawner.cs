using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] objectsToSpawn; // Spawn edilecek nesneler
    public int pairCount = 6; // Çift sayısı (her çift 2 nesne)
    public Vector3 spawnArea = new Vector3(5, 0, 5); // Spawn alanının boyutları
    public float minSpawnHeight = 5f; // Minimum spawn yüksekliği
    public float maxSpawnHeight = 10f; // Maksimum spawn yüksekliği
    public Vector3 areaBounds = new Vector3(15, 15, 8); // Oyun alanı sınırları
    public LayerMask groundLayer; // Yerin bulunduğu katman
    public float respawnDelay = 2f; // Nesneler yok olduktan sonra yeniden oluşturulma süresi

    private Dictionary<string, List<GameObject>> spawnedObjectsByType = new Dictionary<string, List<GameObject>>();
    private bool isRespawning = false;

    void Start()
    {
        Physics.gravity = new Vector3(0, -9.81f, 0); // Varsayılan yerçekimi
        SpawnObjects();  // Başlangıçta nesneleri oluştur
    }

    void Update()
    {
        // Sahnedeki "Matchable" tag'ine sahip nesneleri kontrol et
        GameObject[] spawnedObjects = GameObject.FindGameObjectsWithTag("Matchable");

        // Eğer hiç nesne yoksa ve respawn işlemi yapılmadıysa
        if (spawnedObjects.Length == 0 && !isRespawning)
        {
            isRespawning = true;
            Invoke("RespawnObjects", respawnDelay);  // Belirli bir süre sonra yeniden spawn işlemi başlatılır
        }
    }

    // Nesneleri oluşturma fonksiyonu
    void SpawnObjects()
    {
        for (int i = 0; i < pairCount; i++)
        {
            // Rastgele bir nesne seç
            GameObject selectedObject = objectsToSpawn[Random.Range(0, objectsToSpawn.Length)];

            // Nesnenin türünü kontrol et
            string objectType = selectedObject.GetComponent<ItemType>().type;

            // Nesne türü ile eşleşen nesneleri Dictionary'den al
            if (!spawnedObjectsByType.ContainsKey(objectType))
            {
                spawnedObjectsByType[objectType] = new List<GameObject>();
            }

            // Her tür için 2 nesne oluştur (çift)
            for (int j = 0; j < 2; j++)
            {
                GameObject obj = Instantiate(selectedObject);

                // Rastgele pozisyon belirle
                Vector3 randomPosition = new Vector3(
                    Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                    Random.Range(minSpawnHeight, maxSpawnHeight),
                    Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
                );

                // Yere oturma işlemi (raycast kullanarak zemine yerleştir)
                RaycastHit hit;
                if (Physics.Raycast(randomPosition, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                {
                    randomPosition.y = hit.point.y + 0.5f; // Zeminle temas ettikten sonra nesneyi 0.5 birim yukarıda yerleştir
                }

                obj.transform.position = randomPosition;

                // Rigidbody bileşenini kontrol et ve ekle
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = obj.AddComponent<Rigidbody>();
                }

                // Rigidbody ayarlarını uygula
                rb.useGravity = true;  // Yerçekiminin etkili olması
                rb.isKinematic = false; // Fiziksel etkileşim için kinematik olmamalı
                rb.mass = Random.Range(0.5f, 2f);  // Kütleyi daha doğal hale getirmek için biraz azaltıyoruz
                rb.linearDamping = 2f;  // Yüksek doğrusal sürtünme (kaymayı engeller)
                rb.angularDamping = 3f; // Döndürme sürtünmesi (daha yavaş döner)

                // Nesneye sınır kontrolü için script ekle
                if (obj.GetComponent<ObjectBoundary>() == null)
                {
                    obj.AddComponent<ObjectBoundary>().Initialize(areaBounds);
                }

                // Drag özelliğini ekle
                if (obj.GetComponent<DragObject>() == null)
                {
                    obj.AddComponent<DragObject>();
                }

                // Çarpışma özelliklerini ayarla (friction ve bounciness)
                Collider col = obj.GetComponent<Collider>();
                if (col != null)
                {
                    // Yeni PhysicsMaterial oluştur
                    PhysicsMaterial material = new PhysicsMaterial();
                    material.dynamicFriction = 0.5f; // Orta düzeyde sürtünme
                    material.staticFriction = 0.5f;  // Nesnelerin birbirleriyle daha fazla kaymasını engelleyecek
                    material.bounciness = 0.1f; // Düşük zıplama (bounciness)
                    col.material = material;
                }

                // Nesneyi tür listesine ekle
                spawnedObjectsByType[objectType].Add(obj);

                // Tag ekleyerek "Matchable" olarak işaretleyin
                obj.tag = "Matchable";
            }
        }
    }

    // Nesneleri yeniden oluşturma fonksiyonu
    void RespawnObjects()
    {
        // Tüm nesneleri yok et
        GameObject[] spawnedObjects = GameObject.FindGameObjectsWithTag("Matchable");
        foreach (var obj in spawnedObjects)
        {
            Destroy(obj);
        }

        spawnedObjectsByType.Clear(); // Nesneleri temizle

        // Nesneleri yeniden oluştur
        SpawnObjects();
        isRespawning = false; // Yeniden spawn işlemi tamamlandı
    }
}

public class ObjectBoundary : MonoBehaviour
{
    private Vector3 bounds;

    public void Initialize(Vector3 areaBounds)
    {
        bounds = areaBounds / 2; // Sınırların yarısı
    }

    void Update()
    {
        // Sınır içinde kalma kontrolü
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -bounds.x, bounds.x);
        pos.y = Mathf.Clamp(pos.y, 0, bounds.y); // Yükseklik sınırı
        pos.z = Mathf.Clamp(pos.z, -bounds.z, bounds.z);

        transform.position = pos;
    }
}
