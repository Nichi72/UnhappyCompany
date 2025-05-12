using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("슬라이더")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider voiceVolumeSlider;
    
    [Header("음소거 토글")]
    [SerializeField] private Toggle muteToggle;
    
    [Header("슬라이더 값 텍스트 (선택 사항)")]
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private TextMeshProUGUI voiceVolumeText;
    
    [Header("설정")]
    [SerializeField] private bool playTestSoundOnSFXChange = true;
    [SerializeField] private bool autoSaveOnDisable = true;
    
    private void OnEnable()
    {
        // UI 초기화
        InitializeUI();
    }
    
    private void OnDisable()
    {
        if (autoSaveOnDisable)
        {
            SaveSettings();
        }
        
        // 이벤트 리스너 제거
        RemoveListeners();
    }
    
    private void InitializeUI()
    {
        // 현재 볼륨 설정으로 UI 업데이트
        masterVolumeSlider.value = AudioManager.instance.GetMasterVolume();
        musicVolumeSlider.value = AudioManager.instance.GetMusicVolume();
        sfxVolumeSlider.value = AudioManager.instance.GetSFXVolume();
        voiceVolumeSlider.value = AudioManager.instance.GetVoiceVolume();
        muteToggle.isOn = AudioManager.instance.IsMuted();
        
        // 텍스트 업데이트 (값이 할당된 경우에만)
        UpdateVolumeTexts();
        
        // 이벤트 리스너 등록
        AddListeners();
    }
    
    private void AddListeners()
    {
        // 이벤트 리스너 등록
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
        muteToggle.onValueChanged.AddListener(OnMuteToggled);
    }
    
    private void RemoveListeners()
    {
        // 이벤트 리스너 제거
        masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        voiceVolumeSlider.onValueChanged.RemoveListener(OnVoiceVolumeChanged);
        muteToggle.onValueChanged.RemoveListener(OnMuteToggled);
    }
    
    private void UpdateVolumeTexts()
    {
        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.RoundToInt(masterVolumeSlider.value * 100) + "%";
        
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(musicVolumeSlider.value * 100) + "%";
        
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(sfxVolumeSlider.value * 100) + "%";
        
        if (voiceVolumeText != null)
            voiceVolumeText.text = Mathf.RoundToInt(voiceVolumeSlider.value * 100) + "%";
    }
    
    private void OnMasterVolumeChanged(float value)
    {
        AudioManager.instance.SetMasterVolume(value);
        
        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.instance.SetMusicVolume(value);
        
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.instance.SetSFXVolume(value);
        
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
        
        // 효과음 변경 시 테스트 사운드 재생
        if (playTestSoundOnSFXChange)
        {
            AudioManager.instance.PlaySettingsTestSound();
        }
    }
    
    private void OnVoiceVolumeChanged(float value)
    {
        AudioManager.instance.SetVoiceVolume(value);
        
        if (voiceVolumeText != null)
            voiceVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
    }
    
    private void OnMuteToggled(bool isOn)
    {
        AudioManager.instance.SetMute(isOn);
    }
    
    /// <summary>
    /// 설정 초기화 (버튼에 연결)
    /// </summary>
    public void ResetToDefaults()
    {
        masterVolumeSlider.value = 1.0f;
        musicVolumeSlider.value = 1.0f;
        sfxVolumeSlider.value = 1.0f;
        voiceVolumeSlider.value = 1.0f;
        muteToggle.isOn = false;
        
        // 텍스트 업데이트
        UpdateVolumeTexts();
        
        // 테스트 사운드 재생
        AudioManager.instance.PlaySettingsTestSound();
    }
    
    /// <summary>
    /// 설정 저장 (버튼에 연결 가능)
    /// </summary>
    public void SaveSettings()
    {
        AudioManager.instance.SaveAudioSettings();
    }
    
    /// <summary>
    /// 설정 창 열기
    /// </summary>
    public void OpenSettingsPanel()
    {
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 설정 창 닫기
    /// </summary>
    public void CloseSettingsPanel()
    {
        SaveSettings();
        gameObject.SetActive(false);
    }
    [ContextMenu("PlayTestSound")]
    public void PlayTestSound()
    {
        AudioManager.instance.PlaySettingsTestSound();
    }
}