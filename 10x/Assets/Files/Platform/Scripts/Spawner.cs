using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] int cnt;

    private void Start()
    {
        for (int i = 0; i < cnt; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-7, 7), Random.Range(-3, 3), 0);
            GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        }
    }
}