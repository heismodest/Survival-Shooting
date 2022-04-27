using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

//음악, 사운드를 재생하는 함수를 담고있는 클래스
    public enum AudioChannel {Master, Sfx, Music}

    public float masterVolumePercent { get; private set; }   //= 1f; 다른 클래스에서 사용 가능하도록 변경
    public float sfxVolumePercent { get; private set; }   //= 1;
    public float musicVolumePercent { get; private set; }   //= 0.2f;

    AudioSource sfx2DSource;    //3D 게임환경 고려 2D 사운드 재생 가능하도록
    AudioSource[] musicSources;
    int activeMusicSourceIndex;

    public static AudioManager instance;

    Transform audioListener;
    Transform PlayerT;

    SoundLibrary library;   //사운드 이름으로 플레이시키기 위한 라이브러리 클래스 가져오기

    void Awake()    //시작하면 음악 소스 배열을 만들어서, audio manager에 children object로 추가
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            library = GetComponent<SoundLibrary> ();    //사운드 라이브러리 가져오기

            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject ("Music source " + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }
            GameObject newSfx2DSource = new GameObject ("2D sfx source");
            sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();
            newSfx2DSource.transform.parent = transform;
            audioListener = FindObjectOfType<AudioListener>().transform;

            if (FindObjectOfType<Player>() != null)
            {
                PlayerT = FindObjectOfType<Player>().transform;
            }

            masterVolumePercent = PlayerPrefs.GetFloat ("master vol", 1);
            sfxVolumePercent = PlayerPrefs.GetFloat ("sfx vol", 1);
            musicVolumePercent = PlayerPrefs.GetFloat ("music vol", 1);
        }
    }

    void Update()
    {
        if (PlayerT != null)    //플레이어가 죽은 상태에서 사운드가 들리지 않게 하기 위해
        {
            audioListener.position = PlayerT.position;  //빈 오브젝트에 오디오리스너 컴포넌트를 추가해놓고 플레이어 위치에서 들리도록
        }
    }

    public void SetVolume (float volumePercent, AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.Sfx:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;                
        }
        musicSources [0].volume = musicVolumePercent * masterVolumePercent;
        musicSources [1].volume = musicVolumePercent * masterVolumePercent;

        PlayerPrefs.SetFloat ("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat ("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat ("music vol", musicVolumePercent);
        PlayerPrefs.Save ();
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1)   //BGM 재생
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources [activeMusicSourceIndex].clip = clip;
        musicSources [activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossfade(fadeDuration));
    }

    public void PlaySound(AudioClip clip, Vector3 pos)  //사운드 재생
    {
        if(clip != null)
        {
        AudioSource.PlayClipAtPoint (clip, pos, sfxVolumePercent * masterVolumePercent);
        }
    }

    public void PlaySound(string soundName, Vector3 pos)
    {

        PlaySound (library.GetClipFromName(soundName), pos);
    }

    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent);
    }


    IEnumerator AnimateMusicCrossfade(float duration)
    {
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent); //인터폴레이트
            musicSources[1-activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);
            yield return null;
        }
    }

}
