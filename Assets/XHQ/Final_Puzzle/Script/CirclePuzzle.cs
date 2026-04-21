using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePuzzle : MonoBehaviour
{
    public delegate void OnComplete();
    public OnComplete onComplete;
    public Transform[] circleList;

    [HideInInspector]
    public int[] circleRotation;

    private AudioSource audioSource;
    public AudioClip drag_sound;
    public AudioClip complete_sound;

    public bool shakeCamera = true;
    [HideInInspector]
    public Vector3 saveRotation;

    [HideInInspector]
    public bool gamePause = false;

    void Start ()
    {
        circleRotation = new int[circleList.Length];

        audioSource = GetComponent<AudioSource>();

        saveRotation = Camera.main.transform.localRotation.eulerAngles;

        Setup();
    }

    public void Setup ()
    {
        for (int i = 0; i < circleList.Length; i++)
        {
            circleRotation[i] = Random.Range(3, 20);
        }
    }

    private float playSoundDragTime = 0;
    private float shakeDragTime = 0;
    public void PlayDragSound(int value)
    {
        if (playSoundDragTime <= 0)
        {
            audioSource.pitch = 1.25f - (value * 0.15f);
            audioSource.PlayOneShot(drag_sound);
            playSoundDragTime = 0.08f + (value * 0.025f);
            shakeDragTime = ((0.1f + (value * 0.015f)) * 5);
        }
    }

    void Update()
    {
        if (playSoundDragTime > 0)
        {
            playSoundDragTime -= Time.deltaTime;
        }
        if (shakeDragTime > 0)
        {
            shakeDragTime -= Time.deltaTime;

            if (shakeDragTime <= 0)
            {
                CheckWin();
            }

            if (shakeCamera)
            {
                Vector3 newRand = new Vector3();
                newRand.x = UnityEngine.Random.Range(-0.2f, 0.2f) * shakeDragTime + saveRotation.x;
                newRand.y = UnityEngine.Random.Range(-0.2f, 0.2f) * shakeDragTime + saveRotation.y;
                newRand.z = UnityEngine.Random.Range(-0.2f, 0.2f) * shakeDragTime + saveRotation.z;
                Camera.main.transform.rotation = Quaternion.Euler(newRand);
            }
        }

        for (int i = 0; i < circleList.Length; i++)
        {
            Quaternion toRotation = Quaternion.Euler(-90, 0, circleRotation[i] * 18);
            circleList[i].localRotation = Quaternion.Lerp(circleList[i].localRotation, toRotation, 0.15f - (i * 0.025f));
        }
    }

    public void CheckWin()
    {
        int score = 0;
        int lastRotation = 0;
        for (int i = 0; i < circleRotation.Length; i++)
        {
            if (i == 0)
            {
                int aa = Mathf.FloorToInt((float)circleRotation[i] / 20f);
                aa = circleRotation[i] - (aa * 20);
                lastRotation = aa;
                score++;
            }
            else
            {
                int aa = Mathf.FloorToInt((float)circleRotation[i] / 20f);
                aa = circleRotation[i] - (aa * 20);
                if (lastRotation == aa)
                {
                    score++;
                }
            }
        }
        if (score == circleRotation.Length)
        {
            audioSource.pitch = 1;
            audioSource.PlayOneShot(complete_sound);
            onComplete();
        }
    }
}