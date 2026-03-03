using AudioEnum;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

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

    private AudioRepository CreateTestAdapter()
    {
        AudioDatabase db = AssetLoader.FindAndLoadByName<AudioDatabase>("TestAudioDatabase");
        return new AudioRepository(db, new FakeRandom());
    }

    [Test]
    public void SFX_플레이()
    {
        var adapter = CreateTestAdapter();
        var player = new AudioService(adapter, 3f, 15f);

        var pool = new FakePool();
        var instance = pool.Get();

        var active = player.Play(
            AudioCategory.Sfx,
            AudioName.Test_Sfx,
            instance,
            pool,
            idx: 0,
            loop: false,
            pitch: 1f,
            is3D: false);

        Assert.IsNotNull(active);
        Assert.AreEqual(1, player.Actives.Count);
        Assert.IsTrue(instance.IsPlaying);
    }

    [Test]
    public void SFX_플레이_완료()
    {
        var adapter = CreateTestAdapter();
        var player = new AudioService(adapter, 3f, 15f);

        var pool = new FakePool();
        var instance = pool.Get();

        var active = player.Play(
            AudioCategory.Sfx,
            AudioName.Test_Sfx,
            instance,
            pool,
            0,
            false,
            1f,
            false);

        Assert.AreEqual(1, player.Actives.Count);

        // 강제로 종료 상태 만들기
        instance.Stop();

        player.Tick();

        Assert.AreEqual(0, player.Actives.Count);
        Assert.AreEqual(1, pool.ReleaseCallCount);
    }

    [Test]
    public void BGM_플레이_단일_소스_동작_확인()
    {
        var adapter = CreateTestAdapter();
        var player = new AudioService(adapter, 3f, 15f);

        var instance = new FakeInstance();

        var result = player.Play(
            AudioCategory.Bgm,
            AudioName.Test_Bgm,
            instance,
            null,
            0,
            true,
            1f,
            false);

        Assert.IsNull(result);
        Assert.AreEqual(0, player.Actives.Count);
        Assert.IsTrue(instance.IsPlaying);
    }
}