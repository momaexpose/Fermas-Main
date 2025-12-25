using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    public int currentSlot = -1;
    public float autosaveInterval = 300f; // 5 minutes

    string SavePath(int slot) =>
        Application.persistentDataPath + "/save_slot_" + slot + ".json";

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(AutoSaveRoutine());
    }

    IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autosaveInterval);
            SaveGame();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            SaveGame();
    }

    public bool SlotExists(int slot)
    {
        return File.Exists(SavePath(slot));
    }

    public SaveData LoadSlot(int slot)
    {
        if (!SlotExists(slot)) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath(slot)));
    }

    public void SaveGame()
    {
        if (currentSlot == -1) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        SaveData data = new SaveData();
        data.saveName = LoadSlot(currentSlot)?.saveName ?? "Unnamed Save";
        data.sceneName = SceneManager.GetActiveScene().name;

        data.playerPosition = player.transform.position;
        data.playerRotation = player.transform.rotation;

        // TODO:
        // data.inventory = player.GetComponent<Inventory>().GetSaveData();
        // data.health = player.GetComponent<Health>().currentHealth;

        File.WriteAllText(SavePath(currentSlot),
            JsonUtility.ToJson(data, true));
    }

    public void StartNewGame(int slot, string saveName)
    {
        currentSlot = slot;

        SaveData data = new SaveData();
        data.saveName = saveName;
        data.sceneName = "OrasIntro";

        File.WriteAllText(SavePath(slot),
            JsonUtility.ToJson(data, true));

        SceneManager.LoadScene("OrasIntro");
    }

    public void LoadGame(int slot)
    {
        currentSlot = slot;
        SaveData data = LoadSlot(slot);
        SceneManager.LoadScene(data.sceneName);
    }

    public void ApplyLoadedData()
    {
        SaveData data = LoadSlot(currentSlot);
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        player.transform.position = data.playerPosition;
        player.transform.rotation = data.playerRotation;

        // TODO:
        // player.GetComponent<Inventory>().Load(data.inventory);
        // player.GetComponent<Health>().Set(data.health);
    }
}
