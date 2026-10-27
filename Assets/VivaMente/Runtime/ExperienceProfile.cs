using UnityEngine;

namespace VivaMente
{
    [CreateAssetMenu(menuName = "VivaMente/Perfil de experiência", fileName = "MeuTema")]
    public sealed class ExperienceProfile : ScriptableObject
    {
        public string roomTitle = "Minha sala de vinil";
        [TextArea] public string conversationPrompt = "Que música você gostaria de ouvir aqui?";
        public Color roomColor = new Color(.26f,.38f,.35f);
        [Range(0,.8f)] public float initialVolume = .2f;
        public bool allowSyntheticDemo = true;
        public AudioClip[] tracks = new AudioClip[0];
        public string[] trackTitles = new string[0];
    }
}
