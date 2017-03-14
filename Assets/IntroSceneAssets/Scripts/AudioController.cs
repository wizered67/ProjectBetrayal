using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController inst;

    public AudioSource[] mySrcs;

	// Use this for initialization
	void Awake ()
    {
        inst = this;
        DontDestroyOnLoad(this.gameObject);
        mySrcs = GetComponents<AudioSource>();
    }
    
    public static void Play(string sound)
    {
        foreach (AudioSource s in inst.mySrcs)
        {
            if (s.clip.name == sound)
            {
                s.Play();
            }
        }
    }
}
