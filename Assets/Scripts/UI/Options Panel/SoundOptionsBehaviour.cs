namespace B1NARY.UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI; 
    using UnityEngine.Audio;
	using B1NARY.Audio;

   
    public class SoundOptionsBehaviour : MonoBehaviour 
    {
        //  Sliders for Inspector
        [SerializeField]  AudioMixer mixer ;
        [SerializeField] Slider masterSlider;
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;
        [SerializeField] Slider uiSlider;
        [SerializeField] Slider playerSlider;
        [SerializeField] Slider npcSlider;
        [SerializeField] Slider cameosSlider;
        //  Adding the slider components for inspector for VA. Sub Group of the Voice Channel 
        //  Each Cast Member has their own Audio Source
         [SerializeField] Slider candiiSlider;
         [SerializeField] Slider asterellaSlider;
         [SerializeField] Slider benosphereSlider;
         [SerializeField] Slider comradeDelSlider;
         [SerializeField] Slider fefeSlider;
         [SerializeField] Slider virtuallyLewdSlider;
         [SerializeField] Slider kittyMcpancakesSlider;
         [SerializeField] Slider natsumiMoeSlider;
         [SerializeField] Slider projektMelodySlider;
         [SerializeField] Slider szycroticSlider;
         [SerializeField] Slider silvervaleSlider;
         [SerializeField] Slider watchingLizardSlider;

        //  Strings Might need to be public to transfer Scenes and Save to Audio Manager. Video Source https://www.youtube.com/watch?v=pbuJUaO-wpY&t=383s
        const string MIXER_MASTER = "Master";
        const string MIXER_MUSIC = "Music";
        const string MIXER_SFX = "SFX";
        const string MIXER_UI = "UI";
        const string MIXER_PLAYER = "Player";
        const string MIXER_NPC = "NPC";
        const string MIXER_CAMEOS = "Cameos"; 
        const string MIXER_CANDII = "Candii"; 
        const string MIXER_ASTERELLA = "Asterella";
        const string MIXER_BENOSPHERE = "Benosphere";
        const string MIXER_DEL = "Del";
        const string MIXER_FEFE = "Fefe";
        const string MIXER_VL = "VL";
        const string MIXER_KITTY = "Kitty";
        const string MIXER_MOE = "Moe";
        const string MIXER_MELODY = "Melody";
        const string MIXER_SZYCROTIC = "Szycrotic";
        const string MIXER_SILVERVALE = "Silvervale";
        const string MIXER_LIZZY = "Lizzy";       
        //  Sliders Assignment to Mixer
        void Awake()
        {
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            uiSlider.onValueChanged.AddListener(SetUIVolume);
            playerSlider.onValueChanged.AddListener(SetPlayerVolume);
            npcSlider.onValueChanged.AddListener(SetNPCVolume);
            cameosSlider.onValueChanged.AddListener(SetCameosVolume);
            candiiSlider.onValueChanged.AddListener(SetCandiiVolume);
            asterellaSlider.onValueChanged.AddListener(SetAsterellaVolume);
            benosphereSlider.onValueChanged.AddListener(SetBenosphereVolume);
            comradeDelSlider.onValueChanged.AddListener(SetDelVolume);
            fefeSlider.onValueChanged.AddListener(SetFefeVolume);
            virtuallyLewdSlider.onValueChanged.AddListener(SetVLVolume);
            kittyMcpancakesSlider.onValueChanged.AddListener(SetKittyVolume);
            natsumiMoeSlider.onValueChanged.AddListener(SetMoeVolume);
            projektMelodySlider.onValueChanged.AddListener(SetMelodyVolume);
            szycroticSlider.onValueChanged.AddListener(SetSzycroticVolume);
            silvervaleSlider.onValueChanged.AddListener(SetSilvervaleVolume);
            watchingLizardSlider.onValueChanged.AddListener(SetLizzyVolume);



        }
         // Logrithmic Volume Values. Need these set up like this. Slider Values in inspector need to be around .1 for lowest with 1 the highest. If 0, will break channel.
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
        void SetCandiiVolume(float value)
        {
            mixer.SetFloat(MIXER_CANDII, Mathf.Log10(value) * 100);
        }
         void SetAsterellaVolume(float value)
        {
            mixer.SetFloat(MIXER_ASTERELLA, Mathf.Log10(value) * 100);
        }
         void SetBenosphereVolume(float value)
        {
            mixer.SetFloat(MIXER_BENOSPHERE, Mathf.Log10(value) * 100);
        }
         void SetDelVolume(float value)
        {
            mixer.SetFloat(MIXER_DEL, Mathf.Log10(value) * 100);
        }
        void SetFefeVolume(float value)
        {
            mixer.SetFloat(MIXER_FEFE, Mathf.Log10(value) * 100);
        }
         void SetVLVolume(float value)
        {
            mixer.SetFloat(MIXER_VL, Mathf.Log10(value) * 100);
        }
        void SetKittyVolume(float value)
        {
            mixer.SetFloat(MIXER_KITTY, Mathf.Log10(value) * 100);
        }
        void SetMoeVolume(float value)
        {
            mixer.SetFloat(MIXER_MOE, Mathf.Log10(value) * 100);
        }
        void SetMelodyVolume(float value)
        {
            mixer.SetFloat(MIXER_MELODY, Mathf.Log10(value) * 100);
        }
        void SetSzycroticVolume(float value)
        {
            mixer.SetFloat(MIXER_SZYCROTIC, Mathf.Log10(value) * 100);
        }
         void SetSilvervaleVolume(float value)
        {
            mixer.SetFloat(MIXER_SILVERVALE, Mathf.Log10(value) * 100);
        }
        void SetLizzyVolume(float value)
        {
            mixer.SetFloat(MIXER_LIZZY, Mathf.Log10(value) * 100);
        }
        
        
    }
    



    
 
}

