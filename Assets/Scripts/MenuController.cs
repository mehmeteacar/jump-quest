using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject canvas2;
    private Fade fade;
    private GameObject[] music;
    private AudioSource sound;
    private bool started;

    void Awake()
    {
        DontDestroyOnLoad(canvas2);
        fade = GameObject.FindGameObjectWithTag("Blank").GetComponent<Fade>();
        sound = GameObject.Find("ButtonStart").GetComponent<AudioSource>();
        music = GameObject.FindGameObjectsWithTag("Music");
        DontDestroyOnLoad(music[0]);

        foreach(Transform t in canvas2.transform)
        {
            t.gameObject.SetActive(false);
        }
        
        Variables.currentLevel = 1;
        started = false;
    }

    void Start()
    {
        if (music.Length > 1) {
            Destroy(music[1]);
        }
    }

    public void ButtonStartPressed()
    {
        if (!started)
        {
            started = true;
            sound.Play();
            fade.FadeScreen(Variables.fadeDuration, StartGame);
        }
    }

    public void StartGame()
    {
        canvas2.transform.GetChild(1).gameObject.SetActive(true);
        SceneManager.LoadScene($"Level{Variables.currentLevel}", LoadSceneMode.Single);
    }
}
