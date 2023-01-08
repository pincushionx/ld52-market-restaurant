using UnityEngine;
using UnityEngine.UI;

namespace Pincushion.LD52
{
    public class PauseMenuController : MonoBehaviour
    {
        // Parent overlay
        private OverlayController overlay;

        // Components
        public Slider volumeSlider;
        public Slider musicVolumeSlider;

        private void Start()
        {
            volumeSlider.value = AudioListener.volume;

            AudioSource music = overlay.Scene.Sound.GetAudioSource("Music");
            musicVolumeSlider.value = music.volume;
        }

        public void Init(OverlayController overlay)
        {
            this.overlay = overlay;
        }

        public void ResumeClicked()
        {
            overlay.ShowPauseMenu(false);
            overlay.Scene.Paused = false;
        }
        public void StartGameClicked()
        {
            //GameManager.Instance.StartGame();
        }
        public void RestartLevelClicked()
        {
            //overlay.scene.RestartLevel();
        }
        public void ExitClicked()
        {
            Application.Quit();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; 
            #endif
        }

        public void VolumeChanged(float value)
        {
            AudioListener.volume = volumeSlider.value;
        }
        public void MusicVolumeChanged(float value)
        {
            AudioSource music = overlay.Scene.Sound.GetAudioSource("Music");
            music.volume = musicVolumeSlider.value;
        }
    }
}