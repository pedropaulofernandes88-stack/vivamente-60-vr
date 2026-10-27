using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using VivaMente.Core;

namespace VivaMente
{
    public sealed class MemoryGarden : MonoBehaviour
    {
        private readonly MemorySession session = new MemorySession();
        private readonly Renderer[] flowers = new Renderer[4];
        private readonly Color[] colors = { new Color(.85f,.3f,.25f), new Color(.95f,.7f,.1f), new Color(.35f,.4f,.85f), new Color(.65f,.3f,.75f) };
        private TextMesh status, pauseLabel, paceLabel, levelLabel;
        private int length = 2;
        private float pace = 1.5f;
        private int[] sequence;
        private Coroutine presentation;
        private string message = "Sente-se confortavelmente.\nSelecione Começar para observar as flores.";

        private void Start()
        {
            var rig = new GameObject("Origem sentada");
            rig.transform.position = new Vector3(0, 1.25f, 0);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(rig.transform, false);
            var camera = cameraObject.GetComponent<Camera>();
            camera.nearClipPlane = .05f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.73f,.87f,.87f);
            rig.AddComponent<GardenInput>().View = camera;
            var subsystems = new System.Collections.Generic.List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            foreach (var subsystem in subsystems) subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);
            var light = new GameObject("Luz suave").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(45,-30,0);
            RenderSettings.ambientLight = Color.white * .7f;
            Shape("Jardim", PrimitiveType.Cube, new Vector3(0,-.08f,2), new Vector3(12,.1f,12), new Color(.24f,.43f,.3f));
            Label("VivaMente 60+", new Vector3(0,2.25f,3), .12f);
            Label("JARDIM DA MEMÓRIA", new Vector3(0,2.02f,3), .065f);
            status = Label(message, new Vector3(0,1.75f,3), .055f);
            for (int i = 0; i < 4; i++)
            {
                int id = i;
                float x = (i - 1.5f) * .52f;
                Shape("Caule", PrimitiveType.Cylinder, new Vector3(x,.65f,2.6f), new Vector3(.035f,.25f,.035f), new Color(.15f,.35f,.16f));
                var flower = Shape("Flor " + (i+1), PrimitiveType.Sphere, new Vector3(x,1.03f,2.6f), Vector3.one * .36f, colors[i]);
                flowers[i] = flower.GetComponent<Renderer>();
                flower.AddComponent<GardenAction>().Activate = () => Choose(id);
                Label((i+1).ToString(), new Vector3(x,1.03f,2.39f), .1f);
            }
            Button("Começar", new Vector3(-.65f,.38f,2.5f), Begin);
            pauseLabel = Button("Pausar", new Vector3(0,.38f,2.5f), TogglePause);
            Button("Encerrar", new Vector3(.65f,.38f,2.5f), End);
            paceLabel = Button("Ritmo: calmo", new Vector3(-.5f,.12f,2.8f), () => {
                if (sequence != null) return;
                pace = pace == 1.5f ? 2.5f : 1.5f;
                paceLabel.text = pace == 1.5f ? "Ritmo: calmo" : "Ritmo: lento";
            });
            levelLabel = Button("Flores: 2", new Vector3(.5f,.12f,2.8f), () => {
                if (sequence != null) return;
                length = length == 5 ? 2 : length + 1;
                levelLabel.text = "Flores: " + length;
            });
        }
        private void Begin()
        {
            if (sequence != null) return;
            sequence = new int[length];
            for (int i = 0; i < length; i++) sequence[i] = Random.Range(0,4);
            session.StartRound(sequence);
            presentation = StartCoroutine(ShowSequence());
        }
        private IEnumerator ShowSequence()
        {
            SetMessage("Observe a ordem das flores.\nDepois, selecione na mesma ordem.");
            yield return WaitActive(1f);
            foreach (int id in sequence)
            {
                flowers[id].material.color = Color.white;
                flowers[id].transform.localScale = Vector3.one * .44f;
                yield return WaitActive(pace);
                flowers[id].material.color = colors[id];
                flowers[id].transform.localScale = Vector3.one * .36f;
                yield return WaitActive(.6f);
            }
            session.FinishShowing();
            SetMessage("Sua vez. Selecione a primeira flor.\nVocê tem o tempo que precisar.");
            presentation = null;
        }
        private IEnumerator WaitActive(float seconds)
        {
            float elapsed = 0;
            while (elapsed < seconds || session.IsPaused)
            {
                if (!session.IsPaused) elapsed += Time.deltaTime;
                yield return null;
            }
        }
        private void Choose(int id)
        {
            if (sequence == null || session.IsPaused || presentation != null) return;
            bool correct = session.Select(id);
            if (session.CurrentIndex == sequence.Length)
            {
                SetMessage("Sequência concluída! Tentativas extras: " + session.Mistakes + "\nDescanse ou escolha Começar para uma nova rodada.");
                sequence = null;
            }
            else SetMessage(correct ? "Muito bem. Qual é a próxima flor?" : "Vamos tentar novamente, sem pressa.\nSelecione a próxima flor da sequência.");
        }
        private void SetMessage(string value)
        {
            message = value;
            status.text = session.IsPaused ? "Atividade pausada.\nRetome quando estiver confortável." : message;
        }
        private void TogglePause()
        {
            if (sequence == null) return;
            session.SetPaused(!session.IsPaused);
            pauseLabel.text = session.IsPaused ? "Retomar" : "Pausar";
            SetMessage(message);
        }
        private void OnApplicationFocus(bool focus)
        {
            if (!focus && sequence != null && !session.IsPaused) TogglePause();
        }
        private void End()
        {
            if (presentation != null) StopCoroutine(presentation);
            presentation = null;
            sequence = null;
            session.Reset();
            for (int i = 0; i < 4; i++)
            {
                flowers[i].material.color = colors[i];
                flowers[i].transform.localScale = Vector3.one * .36f;
            }
            pauseLabel.text = "Pausar";
            SetMessage("Atividade encerrada. Faça uma pausa.\nQuando quiser, ajuste o ritmo e escolha Começar.");
        }
        internal static GameObject Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = new Material(Shader.Find("Standard")) { color = color };
            return go;
        }
        internal static TextMesh Label(string value, Vector3 position, float size)
        {
            var go = new GameObject(value);
            go.transform.position = position;
            var label = go.AddComponent<TextMesh>();
            label.text = value;
            label.fontSize = 64;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(.08f,.14f,.12f);
            return label;
        }
        internal static TextMesh Button(string text, Vector3 position, System.Action action)
        {
            var go = Shape(text, PrimitiveType.Cube, position, new Vector3(.58f,.19f,.06f), new Color(.94f,.94f,.84f));
            go.AddComponent<GardenAction>().Activate = action;
            return Label(text, position - new Vector3(0,0,.04f), .045f);
        }
    }
}
