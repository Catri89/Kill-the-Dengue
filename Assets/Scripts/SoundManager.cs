using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static AudioSource audioSource;
    public GameObject soundPanel;
    private Slider soundSlider;
    private Toggle soundToggle;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
        soundSlider = soundPanel.GetComponentInChildren<Slider>();
        soundToggle = soundPanel.GetComponentInChildren<Toggle>();
        if (PlayerPrefs.HasKey("SoundVolume")) {
            audioSource.volume = PlayerPrefs.GetFloat("SoundVolume");
            soundSlider.value = audioSource.volume;
        } else {
            audioSource.volume = audioSource.maxDistance;
            soundSlider.value = audioSource.volume;
        }
    }

    public void SoundToggleChange() {
        if (!soundToggle.isOn) {
            audioSource.volume = 0;
            soundSlider.value = 0;
        } else {
            if (soundSlider.value == 0) {
                audioSource.volume = audioSource.maxDistance;
                soundSlider.value = soundSlider.maxValue;
            }
        }
    }

    public void SoundSliderChange() {
        audioSource.volume = soundSlider.value;
        soundToggle.isOn = (soundSlider.value > 0);
    }

    public static void PlaySound(AudioClip audio) {
        audioSource.clip = audio;
        //Debug.Log("volumen " + audioSource.volume);
        audioSource.Play();
    }

    public void UpdateSoundPref() {
        PlayerPrefs.SetFloat("SoundVolume", soundSlider.value);
    }
}
