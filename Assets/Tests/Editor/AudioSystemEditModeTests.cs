using AudioEnum;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSystemEditModeTests
{
    #region Fake Objects

    private class FakeInstance : IAudioInstance
    {
        public bool IsPlaying { get; private set; }
        public object Clip { get; private set; }
        public float Volume { get; private set; }
        public bool Loop { get; private set; }
        public float Pitch { get; private set; }
        public Vector3 Position { get; private set; }

        public void Play() => IsPlaying = true;
        public void Stop() => IsPlaying = false;

        public void SetClip(object clip) => Clip = clip;
        public void SetLoop(bool loop) => Loop = loop;
        public void SetPitch(float pitch) => Pitch = pitch;
        public void SetVolume(float volume) => Volume = volume;
        public void SetPosition(Vector3 position) => Position = position;
        public void SetOutputAudioMixerGroup(AudioMixerGroup audioMixerGroup) { }
        public void Set2D() { }
        public void Set3D(float minDistance, float maxDistance) { }
    }

    private class FakePool : IAudioSourcePool
    {
        public int ReleaseCallCount = 0;
        public List<IAudioInstance> Handles = new();

        public IAudioInstance Get()
        {
            var instance = new FakeInstance();
            Handles.Add(instance);
            return instance;
        }

        public void Release(IAudioInstance instance)
        {
            ReleaseCallCount++;
            instance.Stop();
        }

        public void ReleaseAll()
        {
            foreach (var h in Handles)
                h.Stop();
        }
    }

    private class FakeRandom : IRandom
    {
        public int Range(int min, int max) => min;
    }
    #endregion

    private AudioRepository repository;
    private AudioService service;
    private FakePool pool;

    [SetUp]
    public void SetUp()
    {
        repository = CreateTestRepository();
        var controller = CreateMixerController();
        service = new AudioService(repository, controller, 3f, 15f);
        pool = new();
    }

    private AudioRepository CreateTestRepository()
    {
        AudioDatabase db = AssetLoader.FindAndLoadByName<AudioDatabase>("TestAudioDatabase");
        return new AudioRepository(db, new FakeRandom());
    }

    private AudioMixerController CreateMixerController()
    {
        AudioMixer audioMixer = AssetLoader.FindAndLoadByName<AudioMixer>("AudioMixer");
        return new AudioMixerController(audioMixer);
    }

    #region SFX 테스트
    [Test]
    public void SFX_플레이()
    {
        var instance = pool.Get();

        var active = service.Play(
            AudioCategory.Sfx,
            AudioName.Test_Sfx,
            instance,
            pool,
            idx: 0,
            loop: false,
            pitch: 1f,
            is3D: false);

        Assert.IsNotNull(active);
        Assert.AreEqual(1, service.Actives.Count);
        Assert.IsTrue(instance.IsPlaying);
    }

    [Test]
    public void SFX_플레이_완료()
    {
        var instance = pool.Get();

        var active = service.Play(
            AudioCategory.Sfx,
            AudioName.Test_Sfx,
            instance,
            pool,
            0,
            false,
            1f,
            false);

        Assert.AreEqual(1, service.Actives.Count);

        // 강제로 종료 상태 만들기
        instance.Stop();

        service.Tick();

        Assert.AreEqual(0, service.Actives.Count);
        Assert.AreEqual(1, pool.ReleaseCallCount);
    }

    [Test]
    public void SFX_풀_적용()
    {
        var i1 = pool.Get();
        var i2 = pool.Get();

        service.Play(AudioCategory.Sfx, AudioName.Test_Sfx, i1, pool, 0, false, 1f, false);
        service.Play(AudioCategory.Sfx, AudioName.Test_Sfx, i2, pool, 0, false, 1f, false);

        Assert.AreEqual(2, service.Actives.Count);
    }

    [Test]
    public void SFX_부분종료_정리()
    {
        var i1 = pool.Get();
        var i2 = pool.Get();

        service.Play(AudioCategory.Sfx, AudioName.Test_Sfx, i1, pool, 0, false, 1f, false);
        service.Play(AudioCategory.Sfx, AudioName.Test_Sfx, i2, pool, 0, false, 1f, false);

        i1.Stop();

        service.Tick();

        Assert.AreEqual(1, service.Actives.Count);
        Assert.AreEqual(1, pool.ReleaseCallCount);
    }

    [Test]
    public void SFX_Pool_Null_안전성()
    {
        var instance = new FakeInstance();

        Assert.DoesNotThrow(() =>
        {
            service.Play(AudioCategory.Sfx, AudioName.Test_Sfx, instance, null, 0, false, 1f, false);
        });
    }

    [Test]
    public void 잘못된_오디오명_예외처리()
    {
        var instance = pool.Get();

        Assert.DoesNotThrow(() =>
        {
            service.Play(AudioCategory.Sfx, (AudioName)999, instance, pool, 0, false, 1f, false);
        });
    }
    #endregion

    #region BGM 테스트
    [Test]
    public void BGM_플레이_단일_소스_동작_확인()
    {
        var instance = new FakeInstance();

        var result = service.Play(
            AudioCategory.Bgm,
            AudioName.Test_Bgm,
            instance,
            null,
            0,
            true,
            1f,
            false);

        Assert.IsNull(result);
        Assert.AreEqual(0, service.Actives.Count);
        Assert.IsTrue(instance.IsPlaying);
    }
    #endregion
}