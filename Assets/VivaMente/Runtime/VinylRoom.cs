using System;
using UnityEngine;
using UnityEngine.XR;

namespace VivaMente
{
    public sealed class VinylRoom : MonoBehaviour
    {
        public ExperienceProfile Profile;
        private AudioSource player;
        private AudioClip[] tracks;
        private Transform record;
        private TextMesh status, volumeLabel, playLabel;
        private int index;
        private bool paused;
        private AudioClip generatedClip;

        private void Start()
        {
            var rig = new GameObject("Origem sentada");
            rig.transform.position = new Vector3(0,1.25f,0);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(rig.transform, false);
            var camera = cameraObject.GetComponent<Camera>();
            camera.nearClipPlane = .05f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Profile != null ? Profile.roomColor : new Color(.26f,.38f,.35f);
            rig.AddComponent<GardenInput>().View = camera;
            var subsystems = new System.Collections.Generic.List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            foreach (var system in subsystems) system.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
            var light = new GameObject("Luz da sala").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(40,-30,0);
            RenderSettings.ambientLight = Color.white * .7f;
            MemoryGarden.Shape("Mesa de madeira", PrimitiveType.Cube, new Vector3(0,.65f,2.5f), new Vector3(2.3f,.14f,1), new Color(.38f,.2f,.1f));
            MemoryGarden.Shape("Toca-discos", PrimitiveType.Cube, new Vector3(0,.79f,2.5f), new Vector3(1.3f,.14f,.8f), new Color(.62f,.42f,.23f));
            record = MemoryGarden.Shape("Disco de vinil", PrimitiveType.Cylinder, new Vector3(-.15f,.89f,2.5f), new Vector3(.66f,.015f,.66f), new Color(.035f,.035f,.035f)).transform;
            var center = MemoryGarden.Shape("Selo", PrimitiveType.Cylinder, new Vector3(-.15f,.908f,2.5f), new Vector3(.23f,.004f,.23f), new Color(.8f,.45f,.17f));
            center.transform.SetParent(record, true);
            MemoryGarden.Shape("Braço", PrimitiveType.Cube, new Vector3(.38f,.94f,2.5f), new Vector3(.025f,.025f,.44f), Color.gray).transform.rotation = Quaternion.Euler(0,25,0);
            MemoryGarden.Label(Profile != null ? Profile.roomTitle : "Minha sala de vinil", new Vector3(0,2.2f,3), .09f).color = Color.white;
            MemoryGarden.Label(Profile != null ? Profile.conversationPrompt : "Que música faz parte da sua história?", new Vector3(0,1.98f,3), .05f).color = Color.white;
            status = MemoryGarden.Label("", new Vector3(0,1.6f,3), .05f);
            status.color = Color.white;
            player = gameObject.AddComponent<AudioSource>();
            player.playOnAwake = false;
            player.spatialBlend = 0;
            player.volume = Profile != null ? Mathf.Clamp(Profile.initialVolume,0,.8f) : .2f;
            tracks = Profile != null ? Profile.tracks : Array.Empty<AudioClip>();
            if ((tracks == null || tracks.Length == 0) && (Profile == null || Profile.allowSyntheticDemo))
            {
                generatedClip = CreateDemo();
                tracks = new[] { generatedClip };
            }
            tracks = tracks ?? Array.Empty<AudioClip>();
            MemoryGarden.Button("Anterior", new Vector3(-.65f,1.2f,2.5f), () => ChangeTrack(-1));
            playLabel = MemoryGarden.Button("Ouvir", new Vector3(0,1.2f,2.5f), PlayPause);
            MemoryGarden.Button("Próxima", new Vector3(.65f,1.2f,2.5f), () => ChangeTrack(1));
            MemoryGarden.Button("Volume −", new Vector3(-.65f,.4f,2.5f), () => Volume(-.1f));
            MemoryGarden.Button("Encerrar", new Vector3(0,.4f,2.5f), Stop);
            MemoryGarden.Button("Volume +", new Vector3(.65f,.4f,2.5f), () => Volume(.1f));
            volumeLabel = MemoryGarden.Label("", new Vector3(0,.15f,2.5f), .045f);
            volumeLabel.color = Color.white;
            Volume(0);
            ShowTrack();
        }
        private void Update()
        {
            if (player != null && player.isPlaying) record.Rotate(0, 200f * Time.deltaTime, 0);
            else if (playLabel != null && !paused) playLabel.text = "Ouvir";
        }
        private void ShowTrack()
        {
            string title = tracks.Length == 0 ? "Nenhuma faixa disponível" : tracks[index] == null ? "Faixa não configurada" : tracks[index].name;
            if (generatedClip != null) title = "Melodia sintética de demonstração";
            else if (Profile != null && index < Profile.trackTitles.Length && !string.IsNullOrWhiteSpace(Profile.trackTitles[index])) title = Profile.trackTitles[index];
            status.text = title + "\nEscolha Ouvir. Você pode parar a qualquer momento.";
        }
        private void ChangeTrack(int step)
        {
            Stop();
            if (tracks.Length > 0) index = (index + step + tracks.Length) % tracks.Length;
            ShowTrack();
        }
        private void PlayPause()
        {
            if (tracks.Length == 0 || tracks[index] == null) return;
            if (player.isPlaying) { player.Pause(); paused = true; playLabel.text = "Retomar"; }
            else
            {
                if (paused) player.UnPause();
                else { player.clip = tracks[index]; player.Play(); }
                paused = false;
                playLabel.text = "Pausar";
            }
        }
        private void Stop()
        {
            player.Stop();
            paused = false;
            playLabel.text = "Ouvir";
            status.text = "Som encerrado.\nFaça uma pausa ou escolha outra faixa.";
        }
        private void Volume(float delta)
        {
            player.volume = Mathf.Clamp(player.volume + delta,0,.8f);
            volumeLabel.text = "Volume: " + Mathf.RoundToInt(player.volume * 100) + "%";
        }
        private void OnApplicationFocus(bool focus)
        {
            if (!focus && player != null && player.isPlaying) PlayPause();
        }
        private void OnDestroy()
        {
            if (generatedClip != null) Destroy(generatedClip);
        }
        private static AudioClip CreateDemo()
        {
            // Pequena sequência sintetizada, sem gravação comercial ou serviço externo.
            const int rate = 22050;
            float[] notes = { 261.63f,329.63f,392f,329.63f,293.66f,349.23f,440f,349.23f,329.63f,392f,523.25f,392f,293.66f,349.23f,261.63f,0 };
            var samples = new float[rate * notes.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (i % rate) / (float)rate;
                float envelope = Mathf.Min(1,t * 30) * Mathf.Max(0,1-t) * .22f;
                samples[i] = Mathf.Sin(2 * Mathf.PI * notes[i/rate] * t) * envelope;
            }
            var clip = AudioClip.Create("Demonstração sintetizada", samples.Length,1,rate,false);
            clip.SetData(samples,0);
            return clip;
        }
    }
}
