using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sounds
{
    BOW_DRAW,
    ARROW_HIT,
    ARROW_DEFLECT,
    STAB,
    COIN,
    WALK,
    SHOVE
}

public class Sound : MonoBehaviour
{
    public AudioSource[] sources;
    public AudioClip[] clips;
    public Sounds sound;

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.FindGameObjectsWithTag("Audio").Length > 1) Destroy(this.gameObject);
        else DontDestroyOnLoad(this.gameObject);

        for(int i = 0; i < sources.Length; i++)
        {
            sources[i].clip = clips[i];
            sources[i].volume = 0.6f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            playSound();
        }
    }

    public void playSound()
    {
        int i = -1;

        switch(sound)
        {
            case Sounds.BOW_DRAW:
                i = 0;
                break;
            case Sounds.ARROW_HIT:
                i = 1;
                break;
            case Sounds.ARROW_DEFLECT:
                i = 2;
                break;
            case Sounds.COIN:
                i = 3;
                break;
            case Sounds.STAB:
                i = 4;
                break;
            case Sounds.WALK:
                i = 5;
                break;
            case Sounds.SHOVE:
                i = 6;
                break;
            default:
                Debug.Log("Sound missing from playSound cases! Sound: " + sound.ToString());
                break;
        }

        if(i == -1)
        {
            Debug.Log("Sound did not get set!");
            return;
        }

        if(sources[i].isPlaying)
        {
            sources[i].Stop();
        }
        sources[i].Play();
    }
}
