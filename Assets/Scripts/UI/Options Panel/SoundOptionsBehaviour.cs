﻿using System;
namespace B1NARY.UI
{
    using UnityEngine;
    using UnityEngine.UI; 
    using UnityEngine.Audio;
	using B1NARY.Audio;

   
    public class SoundOptionsBehaviour : MonoBehaviour 
    {
        //Sliders for Inspector
        [SerializeField]  AudioMixer mixer ;
        [SerializeField] Slider masterSlider;
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;
        [SerializeField] Slider uiSlider;
        [SerializeField] Slider playerSlider;
        [SerializeField] Slider npcSlider;
        [SerializeField] Slider cameosSlider;
        //Adding the slider components for inspector for VA but not attached.
         [SerializeField] Slider candiiSlider;
         [SerializeField] Slider asterellaSlider;
         [SerializeField] Slider benosphereSlider;
         [SerializeField] Slider comradeDelSlider;
         [SerializeField] Slider fefeSlider;
         [SerializeField] Slider ironmouseSlider;
         [SerializeField] Slider kittyMcpancakesSlider;
         [SerializeField] Slider natsumiMoeSlider;
         [SerializeField] Slider projektMelodySlider;
         [SerializeField] Slider szycroticSlider;
         [SerializeField] Slider silvervaleSlider;
         [SerializeField] Slider watchingLizardSlider;

        //Strings Might need to be public to transfer Scenes and Save to Audio Manager. Video Source https://www.youtube.com/watch?v=pbuJUaO-wpY&t=383s
        const string MIXER_MASTER = "Master";
        const string MIXER_MUSIC = "Music";
        const string MIXER_SFX = "SFX";
        const string MIXER_UI = "UI";
        const string MIXER_PLAYER = "Player";
        const string MIXER_NPC = "NPC";
        const string MIXER_CAMEOS = "Cameos"; 
        //Sliders Assignment to Mixer
        void Awake()
        {
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            uiSlider.onValueChanged.AddListener(SetUIVolume);
            playerSlider.onValueChanged.AddListener(SetPlayerVolume);
            npcSlider.onValueChanged.AddListener(SetNPCVolume);
            cameosSlider.onValueChanged.AddListener(SetCameosVolume);
        }
         //Logrithmic Volume Values. Need these set up like this. Slider Values in inspector need to be around .1 for lowest with 1 the highest. If 0, will break channel.
        void SetMasterVolume(float value)
        {
            mixer.SetFloat(MIXER_MASTER, Mathf.Log10(value) * 100);
        }
         void SetMusicVolume(float value)
        {
            mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(value) * 100);
        }
         void SetSFXVolume(float value)
        {
            mixer.SetFloat(MIXER_SFX, Mathf.Log10(value) * 100);
        }
        void SetUIVolume(float value)
        {
            mixer.SetFloat(MIXER_UI, Mathf.Log10(value) * 100);
        }
        void SetPlayerVolume(float value)
        {
            mixer.SetFloat(MIXER_PLAYER, Mathf.Log10(value) * 100);
        }
         void SetNPCVolume(float value)
        {
            mixer.SetFloat(MIXER_NPC, Mathf.Log10(value) * 100);
        }
         void SetCameosVolume(float value)
        {
            mixer.SetFloat(MIXER_CAMEOS, Mathf.Log10(value) * 100);
        }
    }
    



    
 
}

