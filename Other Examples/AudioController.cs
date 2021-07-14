using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {
    [HideInInspector] public AudioSource audioSound;
    [HideInInspector] public AudioSource audioMusic;

    [Header("Music")]
    public AudioClip bossMusic;
    public AudioClip howToMusic;
    public AudioClip menuMusic;
    public AudioClip racingMusic;
    public AudioClip shooterMusic;
    public AudioClip rhythmMusic;
    public AudioClip platformerMusic;
    public AudioClip gravityMusic;
    public AudioClip mazeMusic;
    public AudioClip ballbounceMusic;
    public AudioClip puzzleMusic;

    [Header("General")]
    public AudioClip countdown;
    public AudioClip healthReset;
    public AudioClip playerCollect;
    public AudioClip playerDamage;
    public AudioClip playerDeath;
    public AudioClip respawn;
    public AudioClip winTune;

    [Header("Level Select")]
    public AudioClip cameraMove;
    public AudioClip selectBack;
    public AudioClip selectConfirm;
    public AudioClip selectMove;

    [Header("Final Boss")]
    public AudioClip bossAttack;
    public AudioClip bossDamage;
    public AudioClip bossPowerUp;
    public AudioClip cubeFlip;

    [Header("Racing")]
    public AudioClip driftSkid;
    public AudioClip drive;
    public AudioClip lapComplete;
    public AudioClip playerBump;

    [Header("Shooter")]
    public AudioClip enemySpawn;
    public AudioClip lockAim;
    public AudioClip machineGunShoot;
    public AudioClip pistolShoot;
    public AudioClip playerShoot;
    public AudioClip sniperShoot;

    [Header("Rhythm")]
    public AudioClip playerHitNote;
    public AudioClip playerMiss;
    public AudioClip playerPressKey;

    [Header("Platformer")]
    public AudioClip playerJump;
    public AudioClip springBoost;

    [Header("Gravity")]
    public AudioClip switchGravity;

    [Header("Maze")]
    public AudioClip crusherActivate;
    public AudioClip crusherSmash;
    public AudioClip playerDash;

    [Header("BallBounce")]
    public AudioClip ballBounce;
    public AudioClip ballHit;
    public AudioClip blockBreak;
    public AudioClip paddleSwing;

    [Header("Puzzle")]
    public AudioClip obstacleAppear;
    public AudioClip playerMove;
    public AudioClip tileSwitch;

    public static AudioController Instance { get; private set; } = null;
    private void Awake() {
        Instance = this;
        audioSound = GetComponent<AudioSource>();
    }

    void OnEnable() {
        audioSound = GetComponent<AudioSource>();
        audioMusic = GameObject.Find("Music").GetComponent<AudioSource>();
    }

    public void PlaySoundOnce(AudioClip ac) {
        if (!audioSound.isPlaying)
            audioSound.PlayOneShot(ac);
    }
    public void PlayMusic(AudioClip ac) {
        if (audioMusic.isPlaying)
            audioMusic.Stop();
        audioMusic.clip = ac;
        audioMusic.Play(0);
    }
}
