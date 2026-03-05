using AudioEnum;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSystemEditModeTests
{
    #region Fake Objects
    private class FakeInstance : IAudioInstance
    {
        public bool IsPlaying { get; private set; }
        public bool Set3DCalled { get; private set; }
        public bool Set2DCalled { get; private set; }

        public object Clip { get; private set; }
        public float Volume { get; private set; }
        public bool Loop { get; private set; }
        public float Pitch { get; private set; }
        public Vector3 Position { get; private set; }

        public void Play() => IsPlaying = true;
        public void Pause() => IsPlaying = false;
        public void UnPause() => IsPlaying = true;
        public void Stop() => IsPlaying = false;

        public void SetClip(object clip) => Clip = clip;
        public void SetLoop(bool loop) => Loop = loop;
        public void SetPitch(float pitch) => Pitch = pitch;
        public void SetVolume(float volume) => Volume = volume;
        public void SetPosition(Vector3 position) => Position = position;
        public void SetOutputAudioMixerGroup(AudioMixerGroup audioMixerGroup) { }

        public void Set2D() => Set2DCalled = true;
        public void Set3D(float minDistance, float maxDistance) => Set3DCalled = true;
    }

    private class FakePool : IAudioSourcePool
    {
        public int ReleaseCallCount = 0;

        public IAudioInstance Get() => new FakeInstance();

        public void Release(IAudioInstance instance)
        {
            ReleaseCallCount++;
            instance.Stop();
        }

        public void ReleaseAll() { }
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
        AudioMixer mixer = AssetLoader.FindAndLoadByName<AudioMixer>("AudioMixer");
        return new AudioMixerController(mixer);
    }

    #region SFX Tests
    [Test]
    public void PlaySfx_StartsPlaying_AndReturnsHandle()
    {
        var instance = pool.Get();

        var result = service.Play(
            AudioCategory.Sfx,
            AudioName.Test_Sfx,
            instance,
            pool,
            0,
            false,
            1f,
            false);

        Assert.IsNotNull(result);
        Assert.IsTrue(instance.IsPlaying);
    }

    [Test]
    public void StoppedSfx_IsReleasedOnUpdate()
    {
        var instance = pool.Get();

        service.Play(
            AudioCategory.Sfx,
            AudioName.Test_Sfx,
            instance,
            pool,
            0,
            false,
            1f,
            false);

        instance.Stop();

        service.Update();

        Assert.AreEqual(1, pool.ReleaseCallCount);
    }

    [Test]
    public void MultipleSfx_StopOne_ReleasesOnlyStoppedInstance()
    {
        var i1 = pool.Get();
        var i2 = pool.Get();

        service.Play(AudioCategory.Sfx, AudioName.Test_Sfx, i1, pool, 0, false, 1f, false);
        service.Play(AudioCategory.Sfx, AudioName.Test_Sfx, i2, pool, 0, false, 1f, false);

        i1.Stop();

        service.Update();

        Assert.AreEqual(1, pool.ReleaseCallCount);
        Assert.IsTrue(i2.IsPlaying);
    }

    [Test]
    public void SpatialFlag_True_CallsSet3D()
    {
        var instance = (FakeInstance)pool.Get();

        service.Play(
            AudioCategory.Sfx,
            AudioName.Test_Sfx,
            instance,
            pool,
            0,
            false,
            1f,
            true);

        Assert.IsTrue(instance.Set3DCalled);
        Assert.IsFalse(instance.Set2DCalled);
    }

    [Test]
    public void SpatialFlag_False_CallsSet2D()
    {
        var instance = (FakeInstance)pool.Get();

        service.Play(
            AudioCategory.Sfx,
            AudioName.Test_Sfx,
            instance,
            pool,
            0,
            false,
            1f,
            false);

        Assert.IsTrue(instance.Set2DCalled);
        Assert.IsFalse(instance.Set3DCalled);
    }

    [Test]
    public void InvalidAudioName_DoesNotThrow()
    {
        var instance = pool.Get();

        Assert.DoesNotThrow(() =>
        {
            service.Play(
                AudioCategory.Sfx,
                (AudioName)999,
                instance,
                pool,
                0,
                false,
                1f,
                false);
        });
    }
    #endregion

    #region BGM Tests
    [Test]
    public void PlayBgm_DoesNotRegisterToPool_AndReturnsNull()
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
        Assert.IsTrue(instance.IsPlaying);
    }
    #endregion
}